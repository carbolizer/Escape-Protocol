using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Persistent screen-space label that reflects the player's remaining freeze potions.
/// Self-creates on first gameplay scene load.
/// </summary>
public class FreezePotionHUD : MonoBehaviour
{
    public static FreezePotionHUD Instance { get; private set; }
    private static readonly string[] GameplayScenes = { "Level1", "Level2", "Level3", "BeginningDungeon" };

    private Canvas hudCanvas;
    private TextMeshProUGUI label;

    public static void EnsureExists()
    {
        if (Instance != null) return;

        GameObject go = new GameObject("FreezePotionHUD");
        DontDestroyOnLoad(go);
        go.AddComponent<FreezePotionHUD>();
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
        if (hudCanvas == null || label == null) return;

        bool gameplay = IsGameplay(SceneManager.GetActiveScene().name);
        hudCanvas.gameObject.SetActive(gameplay);
        if (!gameplay) return;

        PlayerController player = FindAnyObjectByType<PlayerController>();
        int potions = player != null ? player.FreezePotionsRemaining : 0;
        int maxPotions = player != null ? player.freezePotionsPerLevel : 0;
        label.text = $"Freeze Potions  {potions}/{maxPotions}   <size=70%>(Q to throw)";
    }

    private void BuildUI()
    {
        GameObject canvasGo = new GameObject("FreezePotionCanvas");
        canvasGo.transform.SetParent(transform, false);
        hudCanvas = canvasGo.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = 300;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject textGo = new GameObject("FreezePotionLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.layer = 5;
        RectTransform rect = textGo.GetComponent<RectTransform>();
        rect.SetParent(canvasGo.transform, false);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(520, 60);
        rect.anchoredPosition = new Vector2(40f, -160f);

        label = textGo.GetComponent<TextMeshProUGUI>();
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (font != null)
            label.font = font;
        else if (TMP_Settings.defaultFontAsset != null)
            label.font = TMP_Settings.defaultFontAsset;

        label.fontSize = 30;
        label.fontStyle = FontStyles.Bold;
        label.color = new Color(0.65f, 0.9f, 1f, 1f);
        label.text = "Freeze Potions  0/0";
        label.outlineWidth = 0.25f;
        label.outlineColor = new Color(0.02f, 0.06f, 0.1f, 1f);
        label.raycastTarget = false;

        hudCanvas.gameObject.SetActive(false);
    }

    private static bool IsGameplay(string sceneName)
    {
        for (int i = 0; i < GameplayScenes.Length; i++)
            if (GameplayScenes[i] == sceneName) return true;
        return false;
    }
}
