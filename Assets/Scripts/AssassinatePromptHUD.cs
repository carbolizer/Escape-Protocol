using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AssassinatePromptHUD : MonoBehaviour
{
    public static AssassinatePromptHUD Instance { get; private set; }

    private Canvas promptCanvas;
    private RectTransform panelRect;
    private TextMeshProUGUI promptText;
    private float pulseTimer;
    private static Sprite whiteSprite;

    public static void EnsureExists()
    {
        if (Instance != null) return;

        GameObject go = new GameObject("AssassinatePromptHUD");
        DontDestroyOnLoad(go);
        go.AddComponent<AssassinatePromptHUD>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (promptCanvas == null) return;

        PlayerController player = FindAnyObjectByType<PlayerController>();
        bool show = player != null &&
                    player.canExecute &&
                    player.executionTarget != null &&
                    !GameManager.IsGamePaused;

        promptCanvas.gameObject.SetActive(show);
        if (!show) return;

        pulseTimer += Time.unscaledDeltaTime * 5f;
        float pulse = 1f + Mathf.Sin(pulseTimer) * 0.05f;
        panelRect.localScale = Vector3.one * pulse;
    }

    private void BuildUI()
    {
        GameObject canvasGo = new GameObject("AssassinatePromptCanvas");
        canvasGo.transform.SetParent(transform, false);

        promptCanvas = canvasGo.AddComponent<Canvas>();
        promptCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        promptCanvas.sortingOrder = 320;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();
        canvasRect.localScale = Vector3.one;

        GameObject panelGo = new GameObject("PromptPanel", typeof(RectTransform), typeof(Image));
        panelGo.layer = 5;
        panelRect = panelGo.GetComponent<RectTransform>();
        panelRect.SetParent(canvasRect, false);
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 140f);
        panelRect.sizeDelta = new Vector2(680, 88);

        Image panelImage = panelGo.GetComponent<Image>();
        panelImage.sprite = GetWhiteSprite();
        panelImage.type = Image.Type.Sliced;
        panelImage.color = new Color(0.06f, 0.04f, 0.1f, 0.94f);
        panelImage.raycastTarget = false;

        Outline outline = panelGo.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.3f, 0.45f, 1f);
        outline.effectDistance = new Vector2(4f, -4f);

        GameObject textGo = new GameObject("PromptText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.layer = 5;
        RectTransform textRect = textGo.GetComponent<RectTransform>();
        textRect.SetParent(panelRect, false);
        Stretch(textRect);

        promptText = textGo.GetComponent<TextMeshProUGUI>();
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (font != null)
            promptText.font = font;
        else if (TMP_Settings.defaultFontAsset != null)
            promptText.font = TMP_Settings.defaultFontAsset;

        promptText.text = "PRESS  E  TO ASSASSINATE";
        promptText.fontSize = 42;
        promptText.fontStyle = FontStyles.Bold;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.color = new Color(1f, 0.95f, 0.98f, 1f);
        promptText.outlineWidth = 0.3f;
        promptText.outlineColor = new Color(0.5f, 0.05f, 0.12f, 1f);
        promptText.raycastTarget = false;

        promptCanvas.gameObject.SetActive(false);
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Sprite GetWhiteSprite()
    {
        if (whiteSprite != null) return whiteSprite;

        Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        Color32 white = new Color32(255, 255, 255, 255);
        Color32[] pixels = { white, white, white, white, white, white, white, white,
            white, white, white, white, white, white, white, white };
        tex.SetPixels32(pixels);
        tex.Apply();
        whiteSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        return whiteSprite;
    }
}
