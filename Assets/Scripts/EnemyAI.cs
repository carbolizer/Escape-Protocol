using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float patrolSpeed = 2f;
    public float patrolDistance = 4f;
    public float waitTime = 1.5f;

    [Header("Vision Detection")]
    public float hearingRadius = 4.5f;
    [Tooltip("Half-angle of the forward detection cone in degrees")]
    public float visionHalfAngle = 55f;
    public float investigateSpeed = 3.5f;
    [Tooltip("How long to keep chasing after spotting the player")]
    public float aggroPersistTime = 3.5f;
    [Tooltip("Minimum detection radius enforced for threat level")]
    public float minAggroRadius = 6f;
    [Tooltip("Layers that block enemy line of sight (walls, doors, props)")]
    public LayerMask sightBlockerMask = ~0;
    [Tooltip("Show a visible vision cone in front of the enemy at runtime")]
    public bool showVisionConeVisual = true;

    [Header("Melee Threat")]
    public float meleeRange = 1.05f;
    public int meleeDamage = 1;
    public float meleeDamageCooldown = 0.7f;
    [Tooltip("How long enemy freezes when player becomes invisible")]
    public float invisLostFreezeTime = 1f;

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
    private Collider2D enemyCollider;
    private Collider2D playerCollider;
    private float aggroTimer;
    private float meleeCooldownTimer;
    private float disengageFreezeTimer;
    private bool wasPlayerHidden;
    private EnemyVisionConeVisual visionConeVisual;
    private float distractionTimer;
    private Vector2 distractionTarget;

    public bool IsDistracted => distractionTimer > 0f;

    public float EffectiveVisionRange => Mathf.Max(hearingRadius, minAggroRadius);
    public float VisionHalfAngle => visionHalfAngle;
    public Transform VisionForward => visionForward != null ? visionForward : transform;
    public LayerMask SightBlockerMask => sightBlockerMask;
    public bool IsAggro => aggroTimer > 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<Collider2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.linearDamping = 0f;

        startPos = transform.position;
        patrolTarget = startPos + Vector2.right * patrolDistance;
        player = FindAnyObjectByType<PlayerController>();
        if (player != null)
            playerCollider = player.GetComponent<Collider2D>();
        wasPlayerHidden = player != null && player.isHidden;
        visionForward = FindLinkedVisionTransform();

        if (showVisionConeVisual)
            visionConeVisual = EnemyVisionConeVisual.AttachTo(this);
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

        if (playerCollider == null)
            playerCollider = player.GetComponent<Collider2D>();

        bool becameHiddenThisFrame = !wasPlayerHidden && player.isHidden;
        wasPlayerHidden = player.isHidden;

        if (becameHiddenThisFrame)
        {
            disengageFreezeTimer = invisLostFreezeTime;
            aggroTimer = 0f;
            rb.linearVelocity = Vector2.zero;
            currentVelocity = Vector2.zero;
        }

        if (disengageFreezeTimer > 0f)
        {
            disengageFreezeTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = Vector2.zero;
            currentVelocity = Vector2.zero;

            if (disengageFreezeTimer <= 0f)
                TurnBackToPatrol();

            return;
        }

        if (distractionTimer > 0f)
        {
            distractionTimer -= Time.fixedDeltaTime;
            UpdateDistractedMovement();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        float effectiveAggroRadius = Mathf.Max(hearingRadius, minAggroRadius);
        bool canDetectPlayer = distanceToPlayer <= effectiveAggroRadius &&
                               !player.isHidden &&
                               IsPlayerInVisionArc() &&
                               HasLineOfSightToPlayer();

        if (canDetectPlayer)
            aggroTimer = aggroPersistTime;
        else if (aggroTimer > 0f)
            aggroTimer -= Time.fixedDeltaTime;

        bool isAggro = aggroTimer > 0f && !player.isHidden;
        Vector2 desiredVelocity = isAggro ? InvestigateVelocity() : PatrolVelocity();
        currentVelocity = Vector2.MoveTowards(currentVelocity, desiredVelocity, acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = currentVelocity;

        if (currentVelocity.sqrMagnitude > 0.05f)
            FaceDirection(currentVelocity.normalized);

        if (meleeCooldownTimer > 0f)
            meleeCooldownTimer -= Time.fixedDeltaTime;

        TryMeleeDamagePlayer();
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

    private void TryMeleeDamagePlayer()
    {
        if (player == null || player.isHidden || meleeCooldownTimer > 0f) return;
        if (!IsPlayerInMeleeRange()) return;
        if (!IsPlayerInVisionArc()) return;
        if (!HasLineOfSightToPlayer()) return;

        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health == null) return;

        health.TakeDamage(meleeDamage);
        meleeCooldownTimer = meleeDamageCooldown;
    }

    private bool HasLineOfSightToPlayer()
    {
        Vector2 origin = transform.position;
        Vector2 target = player.transform.position;
        Vector2 delta = target - origin;
        float distance = delta.magnitude;
        if (distance < 0.001f) return true;

        RaycastHit2D[] hits = Physics2D.LinecastAll(origin, target, sightBlockerMask);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D col = hits[i].collider;
            if (col == null || col.isTrigger) continue;
            if (col == enemyCollider || col.transform.IsChildOf(transform)) continue;
            if (col == playerCollider || col.GetComponentInParent<PlayerController>() != null) continue;

            return false;
        }

        return true;
    }

    private bool IsPlayerInMeleeRange()
    {
        if (enemyCollider != null && playerCollider != null)
        {
            Vector2 enemyPoint = enemyCollider.ClosestPoint(playerCollider.bounds.center);
            Vector2 playerPoint = playerCollider.ClosestPoint(enemyCollider.bounds.center);
            return Vector2.Distance(enemyPoint, playerPoint) <= meleeRange;
        }

        return Vector2.Distance(transform.position, player.transform.position) <= meleeRange;
    }

    private void TurnBackToPatrol()
    {
        movingToTarget = !movingToTarget;
        waitTimer = Mathf.Max(waitTimer, 0.2f);
        FaceDirection(-transform.right);
    }

    /// <summary>
    /// Called by distraction sources (rocks). Pulls the enemy's attention toward a point
    /// for a short investigation period.
    /// </summary>
    public void DistractTowards(Vector2 source, float duration)
    {
        distractionTarget = source;
        distractionTimer = Mathf.Max(distractionTimer, duration);
        aggroTimer = 0f;

        Vector2 facing = source - (Vector2)transform.position;
        if (facing.sqrMagnitude > 0.0001f)
            FaceDirection(facing.normalized);
    }

    private void UpdateDistractedMovement()
    {
        Vector2 toSource = distractionTarget - (Vector2)transform.position;
        Vector2 desiredVelocity;

        if (toSource.magnitude < 0.6f)
        {
            desiredVelocity = Vector2.zero;
        }
        else
        {
            desiredVelocity = toSource.normalized * (investigateSpeed * 0.55f);
        }

        currentVelocity = Vector2.MoveTowards(currentVelocity, desiredVelocity, acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = currentVelocity;

        if (currentVelocity.sqrMagnitude > 0.05f)
            FaceDirection(currentVelocity.normalized);

        if (distractionTimer <= 0f)
        {
            // Resume normal patrol on next tick.
            waitTimer = Mathf.Max(waitTimer, 0.25f);
        }
    }
}
