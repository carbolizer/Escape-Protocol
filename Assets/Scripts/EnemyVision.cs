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
    private Collider2D visionCollider;
    private readonly Collider2D[] overlapResults = new Collider2D[8];

    private void Awake()
    {
        ConeSweep sweep = GetComponent<ConeSweep>();
        visionForward = sweep != null ? sweep.transform : transform;
        visionCollider = GetComponent<Collider2D>();
        ownerCollider = visionCollider != null ? visionCollider : GetComponentInParent<Collider2D>();
    }

    private void Update()
    {
        if (GameManager.IsGamePaused) return;

        if (contactTimer > 0f)
            contactTimer -= Time.deltaTime;

        if (visionCollider == null)
            visionCollider = GetComponent<Collider2D>();

        if (visionCollider == null)
            return;

        int count = Physics2D.OverlapCollider(visionCollider, new ContactFilter2D().NoFilter(), overlapResults);
        for (int i = 0; i < count; i++)
        {
            Collider2D hit = overlapResults[i];
            if (hit == null) continue;

            PlayerController player = hit.GetComponentInParent<PlayerController>();
            if (player == null) continue;

            TryDamagePlayer(hit, player);
            return;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponentInParent<PlayerController>();
        if (player == null) return;

        TryDamagePlayer(collision, player);
    }

    private void TryDamagePlayer(Collider2D playerCollider, PlayerController player)
    {
        if (GameManager.IsGamePaused) return;
        if (player == null || player.isHidden) return;
        if (contactTimer > 0f) return;
        if (!HasLineOfSightTo(playerCollider)) return;

        PlayerHealth health = player.GetComponent<PlayerHealth>();
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
}