using UnityEngine;

/// <summary>
/// Travels in a straight line, ignoring the player and other rocks. On hitting a
/// solid (non-trigger) collider, spawns a DistractionPulse and despawns.
/// </summary>
public class RockProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float remainingLifetime;
    private float pulseRadius;
    private float pulseDuration;
    private Transform owner;
    private bool detonated;
    private SpriteRenderer spriteRenderer;

    private static Sprite rockSprite;

    public void Launch(Vector2 dir, float speed, float lifetime, float pulseRadius, float pulseDuration, Transform owner)
    {
        this.direction = dir.normalized;
        this.speed = speed;
        this.remainingLifetime = lifetime;
        this.pulseRadius = pulseRadius;
        this.pulseDuration = pulseDuration;
        this.owner = owner;

        EnsureVisual();
    }

    private void EnsureVisual()
    {
        if (spriteRenderer != null) return;
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        if (rockSprite == null)
            rockSprite = CreateRockSprite();

        spriteRenderer.sprite = rockSprite;
        spriteRenderer.color = new Color(0.65f, 0.62f, 0.55f, 1f);
        spriteRenderer.sortingLayerName = "Decor";
        spriteRenderer.sortingOrder = 55;
        transform.localScale = Vector3.one * 0.35f;
    }

    private void Update()
    {
        if (detonated) return;
        transform.Rotate(0f, 0f, 540f * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (detonated) return;
        if (GameManager.IsGamePaused) return;

        remainingLifetime -= Time.fixedDeltaTime;
        Vector2 step = direction * speed * Time.fixedDeltaTime;
        Vector2 currentPos = transform.position;

        RaycastHit2D[] hits = Physics2D.RaycastAll(currentPos, direction, step.magnitude);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D col = hits[i].collider;
            if (col == null || col.isTrigger) continue;
            if (owner != null && (col.transform == owner || col.transform.IsChildOf(owner))) continue;
            if (col.GetComponent<RockProjectile>() != null) continue;

            Detonate(hits[i].point);
            return;
        }

        transform.position = currentPos + step;

        if (remainingLifetime <= 0f)
            Detonate(transform.position);
    }

    private void Detonate(Vector2 point)
    {
        if (detonated) return;
        detonated = true;

        DistractionPulse.Spawn(point, pulseRadius, pulseDuration);
        Destroy(gameObject);
    }

    private static Sprite CreateRockSprite()
    {
        const int size = 16;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color32[] pixels = new Color32[size * size];
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - center.x) / (size * 0.45f);
                float dy = (y - center.y) / (size * 0.45f);
                bool inside = dx * dx + dy * dy <= 1f;
                pixels[y * size + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
    }
}
