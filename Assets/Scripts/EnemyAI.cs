using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float patrolSpeed = 2f;
    public float patrolDistance = 4f; // How far they walk before turning around
    public float waitTime = 1.5f;     // How long they stand still at the end of a patrol

    [Header("Hearing System")]
    public float hearingRadius = 5f;
    public float investigateSpeed = 3.5f; // They walk faster when they hear you

    private Vector2 startPos;
    private Vector2 patrolTarget;
    private bool movingToTarget = true;
    private float waitTimer = 0f;

    private Rigidbody2D rb;
    private PlayerController player;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
        // Set the patrol point to the right of where they start
        patrolTarget = startPos + Vector2.right * patrolDistance;

        // Find the player in the scene automatically
        player = FindAnyObjectByType<PlayerController>();
    }

    void Update()
    {
        if (player == null) return;

        // How far away is the player?
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        // Did the enemy hear the player? (Close enough + moving + not hiding)
        bool canHearPlayer = distanceToPlayer <= hearingRadius && player.isMoving && !player.isHidden;

        if (canHearPlayer)
        {
            Investigate();
        }
        else
        {
            Patrol();
        }
    }

    void Investigate()
    {
        // Walk straight toward the player
        Vector2 direction = (player.transform.position - transform.position).normalized;
        rb.linearVelocity = direction * investigateSpeed;
        FaceDirection(direction);
    }

    void Patrol()
    {
        // If we are waiting, stop moving and count down the timer
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Figure out if we are walking toward the target or back to the start
        Vector2 target = movingToTarget ? patrolTarget : startPos;
        Vector2 direction = (target - (Vector2)transform.position).normalized;

        rb.linearVelocity = direction * patrolSpeed;
        FaceDirection(direction);

        // If we reach our destination, wait, and then turn around
        if (Vector2.Distance(transform.position, target) < 0.1f)
        {
            movingToTarget = !movingToTarget;
            waitTimer = waitTime;
        }
    }

    // This makes the entire enemy (and their vision cone/backstab zone) rotate!
    void FaceDirection(Vector2 dir)
    {
        if (dir != Vector2.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // Prevent the sprite from doing a handstand when facing left
            if (Mathf.Abs(angle) > 90)
            {
                GetComponent<SpriteRenderer>().flipY = true;
            }
            else
            {
                GetComponent<SpriteRenderer>().flipY = false;
            }
        }
    }
}