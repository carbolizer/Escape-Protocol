using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float patrolSpeed = 2f;
    public float patrolDistance = 4f;
    public float waitTime = 1.5f;

    [Header("Vision Detection")]
    public float hearingRadius = 5f;
    [Tooltip("Half-angle of the forward detection cone in degrees")]
    public float visionHalfAngle = 55f;
    public float investigateSpeed = 3.5f;

    [Header("Movement Feel")]
    public float acceleration = 18f;

    private Vector2 startPos;
    private Vector2 patrolTarget;
    private bool movingToTarget = true;
    private float waitTimer;
    private Vector2 currentVelocity;

    private Rigidbody2D rb;
    private PlayerController player;
    private Transform visionForward;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.linearDamping = 0f;

        startPos = transform.position;
        patrolTarget = startPos + Vector2.right * patrolDistance;
        player = FindAnyObjectByType<PlayerController>();
        visionForward = FindLinkedVisionTransform();
    }

    private Transform FindLinkedVisionTransform()
    {
        ConeSweep[] cones = FindObjectsByType<ConeSweep>(FindObjectsSortMode.None);
        Transform best = null;
        float bestDistance = 2.5f;

        for (int i = 0; i < cones.Length; i++)
        {
            float distance = Vector2.Distance(transform.position, cones[i].transform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = cones[i].transform;
            }
        }

        return best != null ? best : transform;
    }

    private void FixedUpdate()
    {
        if (GameManager.IsGamePaused || player == null)
        {
            rb.linearVelocity = Vector2.zero;
            currentVelocity = Vector2.zero;
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        bool canDetectPlayer = distanceToPlayer <= hearingRadius &&
                               player.isMoving &&
                               !player.isHidden &&
                               IsPlayerInVisionArc();

        Vector2 desiredVelocity = canDetectPlayer ? InvestigateVelocity() : PatrolVelocity();
        currentVelocity = Vector2.MoveTowards(currentVelocity, desiredVelocity, acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = currentVelocity;

        if (currentVelocity.sqrMagnitude > 0.05f)
            FaceDirection(currentVelocity.normalized);
    }

    private Vector2 InvestigateVelocity()
    {
        Vector2 direction = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
        return direction * investigateSpeed;
    }

    private Vector2 PatrolVelocity()
    {
        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            return Vector2.zero;
        }

        Vector2 target = movingToTarget ? patrolTarget : startPos;
        Vector2 direction = (target - (Vector2)transform.position).normalized;

        if (Vector2.Distance(transform.position, target) < 0.1f)
        {
            movingToTarget = !movingToTarget;
            waitTimer = waitTime;
            return Vector2.zero;
        }

        return direction * patrolSpeed;
    }

    private void FaceDirection(Vector2 dir)
    {
        if (dir == Vector2.zero) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
            sprite.flipY = Mathf.Abs(angle) > 90f;
    }

    private bool IsPlayerInVisionArc()
    {
        Vector2 toPlayer = (Vector2)player.transform.position - (Vector2)transform.position;
        if (toPlayer.sqrMagnitude < 0.0001f) return false;

        Vector2 forward = visionForward.right;
        if (forward.sqrMagnitude < 0.0001f)
            forward = transform.right;

        float dot = Vector2.Dot(forward.normalized, toPlayer.normalized);
        return dot >= Mathf.Cos(visionHalfAngle * Mathf.Deg2Rad);
    }
}
