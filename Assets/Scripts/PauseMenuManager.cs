using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    private static readonly string[] GameplayScenes = { "Level1", "Level2", "Level3", "BeginningDungeon" };

    private Canvas pauseCanvas;
    private GameObject pauseRoot;
    private TextMeshProUGUI scoreText;
    private TMP_FontAsset defaultFont;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (!IsGameplayScene()) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current.pKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
            TogglePause();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsGameplayScene(scene.name))
            SetPaused(false);
    }

    public void TogglePause()
    {
        SetPaused(!GameManager.IsGamePaused);
    }

    public void Resume()
    {
        SetPaused(false);
    }

    public void ReturnToMainMenu()
    {
        SetPaused(false);

        if (GameManager.Instance != null)
            GameManager.Instance.ResetRun();

        SceneManager.LoadScene("TitleScreen");
    }

    private void SetPaused(bool paused)
    {
        if (!IsGameplayScene()) return;

        GameManager.SetGamePaused(paused);

        EnsureUI();
        if (pauseRoot != null)
            pauseRoot.SetActive(paused);

        if (paused)
            RefreshPauseDisplay();
    }

    private void RefreshPauseDisplay()
    {
        if (scoreText == null) return;

        if (GameManager.Instance == null)
        {
            scoreText.text = "Current Score: N/A";
            return;
        }

        GameManager gm = GameManager.Instance;
        scoreText.text =
            $"Current Score: {gm.GetCurrentScore()}\n" +
            $"Kills: {gm.EnemiesKilled} (Stealth: {gm.StealthKills}, +{gm.KillScore} pts)\n" +
            $"Collectibles: {gm.CollectiblesGathered} (+{gm.CollectibleScore} pts)\n" +
            $"Time: {gm.GetFormattedRunTime()} (+{gm.GetProjectedTimeBonus()} pts)";
    }

    private bool IsGameplayScene()
    {
        return IsGameplayScene(SceneManager.GetActiveScene().name);
    }

    private static bool IsGameplayScene(string sceneName)
    {
        for (int i = 0; i < GameplayScenes.Length; i++)
        {
            if (GameplayScenes[i] == sceneName)
                return true;
        }
        return false;
    }

    private void EnsureUI()
    {
        if (pauseCanvas != null) return;

        defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (defaultFont == null)
            defaultFont = TMP_Settings.defaultFontAsset;

        GameObject canvasGo = new GameObject("PauseMenuCanvas");
        canvasGo.transform.SetParent(transform);
        pauseCanvas = canvasGo.AddComponent<Canvas>();
        pauseCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        pauseCanvas.sortingOrder = 200;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        pauseRoot = new GameObject("PauseMenu", typeof(RectTransform));
        pauseRoot.layer = 5;
        RectTransform rootRect = pauseRoot.GetComponent<RectTransform>();
        rootRect.SetParent(pauseCanvas.transform, false);
        StretchFull(rootRect);

        Image backdrop = pauseRoot.AddComponent<Image>();
        backdrop.color = new Color(0f, 0f, 0f, 0.72f);
        backdrop.raycastTarget = true;

        RectTransform panel = CreatePanel(pauseRoot.transform);

        CreateText(panel, "Title", new Vector2(0, 300), new Vector2(700, 60), 40, "PAUSED", TextAlignmentOptions.Center);

        scoreText = CreateText(panel, "ScoreText", new Vector2(0, 175), new Vector2(700, 110), 24, "", TextAlignmentOptions.Center);

        CreateText(panel, "ControlsText", new Vector2(0, -20), new Vector2(700, 320), 22,
            "Controls\n\n" +
            "W / Up Arrow - Move Up\n" +
            "S / Down Arrow - Move Down\n" +
            "A / Left Arrow - Move Left\n" +
            "D / Right Arrow - Move Right\n" +
            "Space - Dash (i-frames, phases enemies, refunds stealth)\n" +
            "Q - Throw distraction rock (2 per level)\n" +
            "F (hold) - Stealth camouflage\n" +
            "E - Stealth kill / interact (best score)\n" +
            "P or Esc - Pause / Resume",
            TextAlignmentOptions.Left);

        CreateButton(panel, "ResumeButton", new Vector2(-130, -250), "Resume", Resume);
        CreateButton(panel, "MainMenuButton", new Vector2(130, -250), "Main Menu", ReturnToMainMenu);

        pauseRoot.SetActive(false);
    }

    private RectTransform CreatePanel(Transform parent)
    {
        GameObject panelGo = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panelGo.layer = 5;
        RectTransform panel = panelGo.GetComponent<RectTransform>();
        panel.SetParent(parent, false);
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(820, 720);
        panel.anchoredPosition = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.1f, 0.12f, 0.16f, 0.96f);
        return panel;
    }

    private TextMeshProUGUI CreateText(RectTransform parent, string name, Vector2 position, Vector2 size, float fontSize,
        string content, TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.layer = 5;
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        if (defaultFont != null)
            text.font = defaultFont;
        text.fontSize = fontSize;
        text.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        text.alignment = alignment;
        text.text = content;
        return text;
    }

    private void CreateButton(RectTransform parent, string name, Vector2 position, string label, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.layer = 5;
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(220, 50);
        rect.anchoredPosition = position;
        go.GetComponent<Image>().color = new Color(0.25f, 0.45f, 0.28f, 1f);

        TextMeshProUGUI text = CreateText(rect, "Label", Vector2.zero, rect.sizeDelta, 22, label, TextAlignmentOptions.Center);
        text.raycastTarget = false;

        go.GetComponent<Button>().onClick.AddListener(onClick);
    }

    private static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
