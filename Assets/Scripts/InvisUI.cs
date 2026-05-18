using UnityEngine;
using UnityEngine.UI;

public class InvisUI : MonoBehaviour
{
    [Tooltip("Filled stealth meter image (child of FillTrack)")]
    public Image invisBarFill;

    private RectTransform rootRect;
    private RectTransform fillRect;
    private static Sprite frameSprite;
    private static Sprite fillSprite;

    private const float BarWidth = 210f;
    private const float BarHeight = 28f;
    private const float InnerPad = 4f;

    // Matches HealthUI top-left layout (3 hearts at 100px each)
    private const float HealthOffsetX = 20f;
    private const float HealthOffsetY = 20f;
    private const float HeartRowWidth = 300f;
    private const float HeartRowHeight = 100f;
    private const float GapBelowHearts = 10f;

    private void Awake()
    {
        EnsureSprites();
        SetupHierarchy();
        ApplyVisualStyle();
    }

    private void Start()
    {
        LayoutBar();
    }

    private void Update()
    {
        if (GameManager.Instance == null || fillRect == null) return;

        float fill = Mathf.Clamp01(
            GameManager.Instance.currentInvisEnergy / GameManager.Instance.maxInvisEnergy);

        float innerWidth = BarWidth - InnerPad * 2f;
        fillRect.sizeDelta = new Vector2(Mathf.Max(0f, innerWidth * fill), BarHeight - InnerPad * 2f);
    }

    private void SetupHierarchy()
    {
        rootRect = (RectTransform)transform;

        Image frame = GetComponent<Image>();
        if (frame == null)
            frame = gameObject.AddComponent<Image>();

        frame.raycastTarget = false;
        frame.sprite = frameSprite;
        frame.type = Image.Type.Simple;
        frame.preserveAspect = false;
        frame.color = Color.white;

        Transform track = transform.Find("FillTrack");
        RectTransform trackRect;
        if (track == null)
        {
            GameObject trackGo = new GameObject("FillTrack", typeof(RectTransform), typeof(RectMask2D));
            trackGo.layer = 5;
            trackRect = trackGo.GetComponent<RectTransform>();
            trackRect.SetParent(rootRect, false);
        }
        else
        {
            trackRect = (RectTransform)track;
            if (track.GetComponent<RectMask2D>() == null)
                track.gameObject.AddComponent<RectMask2D>();
        }

        Stretch(trackRect, InnerPad);

        if (invisBarFill == null)
        {
            Transform fill = trackRect.Find("InvisiBarFill");
            if (fill != null)
                invisBarFill = fill.GetComponent<Image>();
        }

        if (invisBarFill == null)
        {
            GameObject fillGo = new GameObject("InvisiBarFill", typeof(RectTransform), typeof(Image));
            fillGo.layer = 5;
            invisBarFill = fillGo.GetComponent<Image>();
            fillRect = fillGo.GetComponent<RectTransform>();
            fillRect.SetParent(trackRect, false);
        }
        else
        {
            fillRect = invisBarFill.rectTransform;
            if (fillRect.parent != trackRect)
                fillRect.SetParent(trackRect, false);
        }

        fillRect.anchorMin = new Vector2(0f, 0.5f);
        fillRect.anchorMax = new Vector2(0f, 0.5f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchoredPosition = Vector2.zero;
    }

    private void ApplyVisualStyle()
    {
        if (invisBarFill == null) return;

        invisBarFill.sprite = fillSprite;
        invisBarFill.type = Image.Type.Tiled;
        invisBarFill.pixelsPerUnitMultiplier = 1f;
        invisBarFill.color = Color.white;
        invisBarFill.raycastTarget = false;
        invisBarFill.material = null;
    }

    private void LayoutBar()
    {
        rootRect.anchorMin = new Vector2(0f, 1f);
        rootRect.anchorMax = new Vector2(0f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.sizeDelta = new Vector2(BarWidth, BarHeight);

        RectTransform healthRow = FindHealthRowRect();
        if (healthRow != null && TryMeasureHeartRow(healthRow, out float centerX, out float bottomY))
        {
            rootRect.anchoredPosition = new Vector2(centerX, bottomY - GapBelowHearts);
            return;
        }

        rootRect.anchoredPosition = new Vector2(
            HealthOffsetX + HeartRowWidth * 0.5f,
            -(HealthOffsetY + HeartRowHeight + GapBelowHearts));
    }

    private static bool TryMeasureHeartRow(RectTransform healthRow, out float centerXInParent, out float bottomYInParent)
    {
        centerXInParent = 0f;
        bottomYInParent = 0f;

        RectTransform parent = healthRow.parent as RectTransform;
        if (parent == null) return false;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(healthRow);

        float totalWidth = 0f;
        float maxHeight = 0f;
        int activeChildren = 0;

        HorizontalLayoutGroup layout = healthRow.GetComponent<HorizontalLayoutGroup>();
        float spacing = layout != null ? layout.spacing : 0f;

        for (int i = 0; i < healthRow.childCount; i++)
        {
            RectTransform child = healthRow.GetChild(i) as RectTransform;
            if (child == null || !child.gameObject.activeInHierarchy) continue;

            if (activeChildren > 0)
                totalWidth += spacing;

            totalWidth += child.rect.width;
            maxHeight = Mathf.Max(maxHeight, child.rect.height);
            activeChildren++;
        }

        if (activeChildren == 0) return false;

        // Health row uses a small layout box; hearts extend horizontally from its pivot.
        centerXInParent = healthRow.anchoredPosition.x + totalWidth * 0.5f;
        bottomYInParent = healthRow.anchoredPosition.y - maxHeight;
        return true;
    }

    private static RectTransform FindHealthRowRect()
    {
        GameObject healthRow = GameObject.Find("HealthUI");
        return healthRow != null ? healthRow.transform as RectTransform : null;
    }

    private static void Stretch(RectTransform rect, float padding)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(padding, padding);
        rect.offsetMax = new Vector2(-padding, -padding);
    }

    private static void EnsureSprites()
    {
        if (frameSprite != null && fillSprite != null) return;

        frameSprite = CreateSprite(BuildFrameTexture(48, 14), 16f);
        fillSprite = CreateSprite(BuildFillTexture(4, 8), 16f);
    }

    private static Texture2D BuildFrameTexture(int width, int height)
    {
        Color32[] pixels = new Color32[width * height];
        Color32 border = new Color32(0, 0, 0, 255);
        Color32 innerBorder = new Color32(24, 20, 36, 255);
        Color32 fill = new Color32(42, 36, 58, 255);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool edge = x == 0 || y == 0 || x == width - 1 || y == height - 1;
                bool inset = x == 1 || y == 1 || x == width - 2 || y == height - 2;
                if (edge) pixels[y * width + x] = border;
                else if (inset) pixels[y * width + x] = innerBorder;
                else pixels[y * width + x] = fill;
            }
        }

        return CreateTexture(width, height, pixels);
    }

    private static Texture2D BuildFillTexture(int width, int height)
    {
        Color32[] pixels = new Color32[width * height];
        Color32 highlight = new Color32(188, 150, 255, 255);
        Color32 mid = new Color32(124, 82, 198, 255);
        Color32 shadow = new Color32(62, 38, 110, 255);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (y == height - 1) pixels[y * width + x] = shadow;
                else if (y == height - 2) pixels[y * width + x] = mid;
                else pixels[y * width + x] = highlight;
            }
        }

        return CreateTexture(width, height, pixels);
    }

    private static Texture2D CreateTexture(int width, int height, Color32[] pixels)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.SetPixels32(pixels);
        texture.Apply();
        return texture;
    }

    private static Sprite CreateSprite(Texture2D texture, float pixelsPerUnit)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }
}
