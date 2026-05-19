using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    [Tooltip("Half-angle in front of the vision cone that can spot the player")]
    public float visionHalfAngle = 55f;

    private Transform visionForward;

    private void Awake()
    {
        ConeSweep sweep = GetComponent<ConeSweep>();
        visionForward = sweep != null ? sweep.transform : transform;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (!IsTargetInFront(collision.transform.position)) return;

        PlayerController player = collision.GetComponent<PlayerController>();
        if (player == null || player.isHidden) return;

        PlayerHealth health = collision.GetComponent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(1);
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