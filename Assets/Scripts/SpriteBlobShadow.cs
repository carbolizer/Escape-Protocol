using UnityEngine;

[DisallowMultipleComponent]
public class SpriteBlobShadow : MonoBehaviour
{
    [SerializeField] private Vector2 localOffset = new Vector2(0f, -0.09f);
    [SerializeField] private float baseScale = 1.15f;
    [SerializeField] private float moveStretch = 0.12f;
    [SerializeField] private float alpha = 0.45f;

    private Transform shadowTransform;
    private SpriteRenderer shadowRenderer;
    private SpriteRenderer sourceRenderer;
    private static Sprite sharedShadowSprite;

    private void Awake()
    {
        sourceRenderer = GetComponent<SpriteRenderer>();
        EnsureShadow();
    }

    private void LateUpdate()
    {
        if (shadowTransform == null) return;

        shadowTransform.localPosition = localOffset;

        float stretch = 0f;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            stretch = Mathf.Clamp01(rb.linearVelocity.magnitude / 6f) * moveStretch;

        float scaleX = baseScale + stretch;
        float scaleY = baseScale - stretch * 0.5f;
        shadowTransform.localScale = new Vector3(scaleX, scaleY, 1f);

        if (sourceRenderer != null)
            shadowRenderer.sortingOrder = sourceRenderer.sortingOrder - 1;
    }

    private void EnsureShadow()
    {
        if (sharedShadowSprite == null)
            sharedShadowSprite = CreateShadowSprite();

        Transform existing = transform.Find("BlobShadow");
        if (existing != null)
        {
            shadowTransform = existing;
            shadowRenderer = existing.GetComponent<SpriteRenderer>();
            return;
        }

        GameObject shadowGo = new GameObject("BlobShadow");
        shadowGo.layer = gameObject.layer;
        shadowTransform = shadowGo.transform;
        shadowTransform.SetParent(transform, false);
        shadowTransform.localPosition = localOffset;
        shadowTransform.localRotation = Quaternion.identity;
        shadowTransform.localScale = Vector3.one * baseScale;

        shadowRenderer = shadowGo.AddComponent<SpriteRenderer>();
        shadowRenderer.sprite = sharedShadowSprite;
        shadowRenderer.color = new Color(0f, 0f, 0f, alpha);
        shadowRenderer.sortingLayerID = sourceRenderer != null ? sourceRenderer.sortingLayerID : 0;
        shadowRenderer.sortingOrder = sourceRenderer != null ? sourceRenderer.sortingOrder - 1 : -1;
    }

    private static Sprite CreateShadowSprite()
    {
        const int size = 16;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        Color32[] pixels = new Color32[size * size];
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radiusX = size * 0.42f;
        float radiusY = size * 0.28f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - center.x) / radiusX;
                float dy = (y - center.y) / radiusY;
                float dist = dx * dx + dy * dy;
                byte a = dist <= 1f ? (byte)(200 * (1f - dist)) : (byte)0;
                pixels[y * size + x] = new Color32(0, 0, 0, a);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
    }
}
