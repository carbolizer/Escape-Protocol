using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Persistent screen-space label that reflects the player's remaining rocks.
/// Self-creates on first gameplay scene load.
/// </summary>
public class RockCountHUD : MonoBehaviour
{
    public static RockCountHUD Instance { get; private set; }
    private static readonly string[] GameplayScenes = { "Level1", "Level2", "Level3", "BeginningDungeon" };

    private Canvas hudCanvas;
    private TextMeshProUGUI label;

    public static void EnsureExists()
    {
        if (Instance != null) return;

        GameObject go = new GameObject("RockCountHUD");
        DontDestroyOnLoad(go);
        go.AddComponent<RockCountHUD>();
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
        int rocks = player != null ? player.RocksRemaining : 0;
        label.text = $"Rocks  {rocks}/{(player != null ? player.rocksPerLevel : 0)}   <size=70%>(Q to throw)";
    }

    private void BuildUI()
    {
        GameObject canvasGo = new GameObject("RockCountCanvas");
        canvasGo.transform.SetParent(transform, false);
        hudCanvas = canvasGo.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = 300;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject textGo = new GameObject("RockLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.layer = 5;
        RectTransform rect = textGo.GetComponent<RectTransform>();
        rect.SetParent(canvasGo.transform, false);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(400, 60);
        rect.anchoredPosition = new Vector2(40f, -160f);

        label = textGo.GetComponent<TextMeshProUGUI>();
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (font != null)
            label.font = font;
        else if (TMP_Settings.defaultFontAsset != null)
            label.font = TMP_Settings.defaultFontAsset;

        label.fontSize = 30;
        label.fontStyle = FontStyles.Bold;
        label.color = new Color(1f, 0.9f, 0.6f, 1f);
        label.text = "Rocks  0/0";
        label.outlineWidth = 0.25f;
        label.outlineColor = new Color(0.1f, 0.06f, 0.02f, 1f);
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
