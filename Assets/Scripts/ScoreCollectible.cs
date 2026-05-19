using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(CircleCollider2D))]
public class ScoreCollectible : MonoBehaviour
{
    public int pointValue = 75;
    [SerializeField] private Sprite collectibleSpriteAsset;

    private SpriteRenderer spriteRenderer;
    private float spinSpeed = 120f;
    private float baseVisualScale = 1f;
    private static Sprite collectibleSprite;

    private void Awake()
    {
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.35f;

        EnsureVisual();
    }

    private void OnEnable()
    {
        EnsureVisual();
    }

    private void Update()
    {
        if (!Application.isPlaying) return;

        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
        if (spriteRenderer != null)
        {
            float pulse = 0.9f + Mathf.Sin(Time.time * 4f) * 0.1f;
            spriteRenderer.transform.localScale = Vector3.one * baseVisualScale * pulse;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterCollectible(pointValue);

        Destroy(gameObject);
    }

    private void EnsureVisual()
    {
        // Always render on the root collectible object so dragging in scene view
        // changes the real collectible position (not a child visual offset).
        SpriteRenderer rootRenderer = GetComponent<SpriteRenderer>();
        if (rootRenderer == null)
            rootRenderer = gameObject.AddComponent<SpriteRenderer>();

        Transform legacyVisual = transform.Find("CollectibleVisual");
        if (legacyVisual != null)
        {
            SpriteRenderer legacyRenderer = legacyVisual.GetComponent<SpriteRenderer>();
            if (legacyRenderer != null)
            {
                if (rootRenderer.sprite == null)
                    rootRenderer.sprite = legacyRenderer.sprite;

                rootRenderer.color = legacyRenderer.color;
                rootRenderer.sortingLayerID = legacyRenderer.sortingLayerID;
                rootRenderer.sortingOrder = legacyRenderer.sortingOrder;

                if (!Application.isPlaying && legacyVisual.localPosition.sqrMagnitude > 0.0001f)
                    transform.position += legacyVisual.localPosition;
            }

            if (Application.isPlaying)
                Destroy(legacyVisual.gameObject);
            else
                DestroyImmediate(legacyVisual.gameObject);
        }

        spriteRenderer = rootRenderer;

        if (spriteRenderer.sprite == null)
        {
            if (collectibleSpriteAsset != null)
            {
                spriteRenderer.sprite = collectibleSpriteAsset;
            }
            else
            {
                if (collectibleSprite == null)
                    collectibleSprite = CreateFallbackSprite();
                spriteRenderer.sprite = collectibleSprite;
            }
        }

        baseVisualScale = CalculateBaseVisualScale(spriteRenderer.sprite);
        spriteRenderer.color = new Color(1f, 0.95f, 0.4f, 1f);
        ApplyVisibleSorting(spriteRenderer);
    }

    private static float CalculateBaseVisualScale(Sprite sprite)
    {
        const float targetWorldSize = 0.9f;
        if (sprite == null) return 2f;

        Vector2 size = sprite.bounds.size;
        float maxDimension = Mathf.Max(size.x, size.y);
        if (maxDimension <= 0.0001f) return 2f;

        return targetWorldSize / maxDimension;
    }

    private static void ApplyVisibleSorting(SpriteRenderer target)
    {
        EnemyAI enemy = FindAnyObjectByType<EnemyAI>();
        if (enemy != null)
        {
            SpriteRenderer enemyRenderer = enemy.GetComponent<SpriteRenderer>();
            if (enemyRenderer != null)
            {
                target.sortingLayerID = enemyRenderer.sortingLayerID;
                target.sortingOrder = enemyRenderer.sortingOrder + 1;
                return;
            }
        }

        target.sortingLayerName = "Default";
        target.sortingOrder = 120;
    }

    private static Sprite CreateFallbackSprite()
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
                float dx = Mathf.Abs(x - center.x) / (size * 0.42f);
                float dy = Mathf.Abs(y - center.y) / (size * 0.42f);
                byte a = dx + dy <= 1f ? (byte)255 : (byte)0;
                pixels[y * size + x] = new Color32(255, 255, 255, a);
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
    }
}
