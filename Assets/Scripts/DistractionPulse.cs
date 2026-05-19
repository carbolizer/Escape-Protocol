using UnityEngine;

/// <summary>
/// Spawned where a rock detonates. Distracts every EnemyAI within radius for the
/// supplied duration, while drawing a quickly fading ring on screen for feedback.
/// </summary>
public class DistractionPulse : MonoBehaviour
{
    private float maxRadius;
    private float distractionDuration;
    private float lifetime;
    private float elapsed;
    private SpriteRenderer ring;
    private static Sprite ringSprite;

    public static DistractionPulse Spawn(Vector2 position, float radius, float duration)
    {
        GameObject go = new GameObject("DistractionPulse");
        go.transform.position = position;

        DistractionPulse pulse = go.AddComponent<DistractionPulse>();
        pulse.Initialize(radius, duration);
        return pulse;
    }

    private void Initialize(float radius, float duration)
    {
        maxRadius = Mathf.Max(0.5f, radius * 0.5f);
        distractionDuration = Mathf.Max(0.5f, duration);
        lifetime = 0.55f;
        elapsed = 0f;

        ApplyDistraction();
        BuildRing();
    }

    private void ApplyDistraction()
    {
        Vector2 origin = transform.position;
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);

        for (int i = 0; i < enemies.Length; i++)
        {
            EnemyAI enemy = enemies[i];
            if (enemy == null) continue;

            float distance = Vector2.Distance(origin, enemy.transform.position);
            if (distance > maxRadius) continue;

            enemy.DistractTowards(origin, distractionDuration);
        }
    }

    private void BuildRing()
    {
        if (ringSprite == null)
            ringSprite = CreateRingSprite();

        GameObject visual = new GameObject("PulseRing");
        visual.transform.SetParent(transform, false);
        ring = visual.AddComponent<SpriteRenderer>();
        ring.sprite = ringSprite;
        ring.color = new Color(1f, 0.85f, 0.4f, 0.85f);
        ring.sortingLayerName = "Decor";
        ring.sortingOrder = 70;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / lifetime);
        float currentRadius = Mathf.Lerp(0.15f, maxRadius, t);

        if (ring != null)
        {
            ring.transform.localScale = Vector3.one * currentRadius * 2f;
            Color c = ring.color;
            c.a = (1f - t) * 0.85f;
            ring.color = c;
        }

        if (elapsed >= lifetime)
            Destroy(gameObject);
    }

    private static Sprite CreateRingSprite()
    {
        const int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color32[] pixels = new Color32[size * size];
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float outer = size * 0.5f;
        float inner = size * 0.42f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                byte alpha = 0;
                if (dist <= outer && dist >= inner)
                {
                    float band = 1f - Mathf.Abs(dist - (inner + outer) * 0.5f) / ((outer - inner) * 0.5f);
                    alpha = (byte)Mathf.Clamp(band * 255f, 0, 255);
                }
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }
}
