using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    [Tooltip("Half-angle in front of the vision cone that can spot the player")]
    public float visionHalfAngle = 55f;
    [Tooltip("Repeated contact damage interval while player remains in trigger")]
    public float contactDamageInterval = 0.7f;
    [Tooltip("Layers that block enemy line of sight")]
    public LayerMask sightBlockerMask = ~0;

    private Transform visionForward;
    private float contactTimer;
    private Collider2D ownerCollider;

    private void Awake()
    {
        ConeSweep sweep = GetComponent<ConeSweep>();
        visionForward = sweep != null ? sweep.transform : transform;
        ownerCollider = GetComponentInParent<Collider2D>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (GameManager.IsGamePaused) return;
        if (!collision.CompareTag("Player")) return;
        if (!IsTargetInFront(collision.transform.position)) return;
        if (!HasLineOfSightTo(collision)) return;

        PlayerController player = collision.GetComponent<PlayerController>();
        if (player == null || player.isHidden) return;

        if (contactTimer > 0f)
        {
            contactTimer -= Time.deltaTime;
            return;
        }

        PlayerHealth health = collision.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(1);
            contactTimer = contactDamageInterval;
        }
    }

    private bool HasLineOfSightTo(Collider2D target)
    {
        Vector2 origin = transform.position;
        Vector2 destination = target.bounds.center;
        if ((destination - origin).sqrMagnitude < 0.0001f) return true;

        RaycastHit2D[] hits = Physics2D.LinecastAll(origin, destination, sightBlockerMask);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D col = hits[i].collider;
            if (col == null || col.isTrigger) continue;
            if (col == target || col.transform.IsChildOf(target.transform)) continue;
            if (ownerCollider != null && (col == ownerCollider || col.transform.IsChildOf(ownerCollider.transform))) continue;
            return false;
        }

        return true;
    }

    private bool IsTargetInFront(Vector3 targetPosition)
    {
        Vector2 toTarget = (Vector2)targetPosition - (Vector2)transform.position;
        if (toTarget.sqrMagnitude < 0.0001f) return false;

        Vector2 forward = visionForward.right;
        if (forward.sqrMagnitude < 0.0001f)
            forward = transform.right;

        float dot = Vector2.Dot(forward.normalized, toTarget.normalized);
        return dot >= Mathf.Cos(visionHalfAngle * Mathf.Deg2Rad);
    }
}