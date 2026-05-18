using HighScore;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinScreenUI : MonoBehaviour
{
    private static TMP_FontAsset cachedDefaultFont;

    [Header("UI References")]
    public TextMeshProUGUI scoreSummaryText;
    public TMP_InputField playerNameInput;
    public Button submitScoreButton;
    public TextMeshProUGUI submitStatusText;

    private bool scoreSubmitted;
    private bool uiBuilt;

    private void Awake()
    {
        GetDefaultFont();
        FixLegacyMainMenuButton();
    }

    private void Start()
    {
        if (GameManager.Instance != null && !GameManager.Instance.RunFinalized)
            GameManager.Instance.FinalizeRun();

        BuildUI();
        RefreshScoreDisplay();
    }

    public void RefreshScoreDisplay()
    {
        if (scoreSummaryText == null) return;

        if (GameManager.Instance == null)
        {
            scoreSummaryText.text =
                "Run Complete!\n\n" +
                "No run data found.\n" +
                "Start from the Title Screen and finish Level 3 to record a score.";
            return;
        }

        GameManager gm = GameManager.Instance;
        scoreSummaryText.text =
            "Run Complete!\n\n" +
            $"Final Score: {gm.FinalScore}\n" +
            $"Enemies Defeated: {gm.EnemiesKilled} (+{gm.KillScore} pts)\n" +
            $"Time: {gm.GetFormattedRunTime()} (+{gm.TimeBonus} pts)";
    }

    public void SubmitScore()
    {
        if (scoreSubmitted || GameManager.Instance == null) return;

        string playerName = playerNameInput != null
            ? playerNameInput.text.Trim()
            : "Player";

        if (string.IsNullOrEmpty(playerName))
            playerName = "Player";

        HS.SubmitHighScore(this, playerName, GameManager.Instance.FinalScore);
        scoreSubmitted = true;

        if (submitStatusText != null)
            submitStatusText.text = "Score submitted!";
        if (submitScoreButton != null)
            submitScoreButton.interactable = false;
        if (playerNameInput != null)
            playerNameInput.interactable = false;
    }

    private void BuildUI()
    {
        if (uiBuilt) return;
        uiBuilt = true;

        Canvas canvas = GetComponent<Canvas>() ?? FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        if (scoreSummaryText == null)
            scoreSummaryText = canvas.GetComponentInChildren<TextMeshProUGUI>(true);

        RectTransform panel = ResolveScorePanel(canvas.transform);
        if (scoreSummaryText == null)
        {
            scoreSummaryText = CreateText(panel, "ScoreSummary", Vector2.zero, Vector2.zero, 26);
            scoreSummaryText.alignment = TextAlignmentOptions.Center;
        }
        else
        {
            ApplyFont(scoreSummaryText);
            scoreSummaryText.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        }

        if (playerNameInput == null)
            playerNameInput = CreateNameInput(panel, Vector2.zero);

        if (submitScoreButton == null)
            submitScoreButton = CreateButton(panel, "Submit Score", Vector2.zero, SubmitScore);

        if (submitStatusText == null)
        {
            submitStatusText = CreateText(panel, "SubmitStatus", Vector2.zero, Vector2.zero, 18);
            submitStatusText.alignment = TextAlignmentOptions.Left;
        }

        ApplyLayout(panel);
        panel.SetAsFirstSibling();
    }

    private static void ApplyLayout(RectTransform panel)
    {
        Transform summary = panel.Find("ScoreSummary");
        if (summary is RectTransform summaryRect)
        {
            summaryRect.anchoredPosition = new Vector2(0, 115);
            summaryRect.sizeDelta = new Vector2(760, 185);
        }

        Transform nameInput = panel.Find("PlayerNameInput");
        if (nameInput is RectTransform nameRect)
        {
            nameRect.anchoredPosition = new Vector2(0, -55);
            nameRect.sizeDelta = new Vector2(340, 44);
        }

        Transform submit = panel.Find("Submit ScoreButton");
        if (submit is RectTransform submitRect)
        {
            submitRect.anchoredPosition = new Vector2(-110, -130);
            submitRect.sizeDelta = new Vector2(210, 46);
        }

        Transform status = panel.Find("SubmitStatus");
        if (status is RectTransform statusRect)
        {
            statusRect.anchoredPosition = new Vector2(115, -130);
            statusRect.sizeDelta = new Vector2(260, 40);
        }
    }

    private static RectTransform ResolveScorePanel(Transform canvasTransform)
    {
        Transform existing = canvasTransform.Find("ScorePanel");
        if (existing != null)
            return existing as RectTransform;

        return CreatePanel(canvasTransform);
    }

    private static void FixLegacyMainMenuButton()
    {
        if (SceneManager.GetActiveScene().name != "WinScreen") return;

        foreach (Button button in FindObjectsByType<Button>(FindObjectsSortMode.None))
        {
            if (button.gameObject.name != "Button") continue;

            RectTransform rect = button.GetComponent<RectTransform>();
            if (rect == null) continue;

            rect.sizeDelta = new Vector2(240, 52);
            rect.anchoredPosition = new Vector2(0, -300);

            Image image = button.GetComponent<Image>();
            if (image != null)
                image.color = new Color(0.92f, 0.92f, 0.92f, 1f);

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = "Main Menu";
                ApplyFont(label);
            }
        }
    }

    private static RectTransform CreatePanel(Transform canvasTransform)
    {
        GameObject panelGo = new GameObject("ScorePanel", typeof(RectTransform), typeof(Image));
        panelGo.layer = 5;
        RectTransform panel = panelGo.GetComponent<RectTransform>();
        panel.SetParent(canvasTransform, false);
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(820, 460);
        panel.anchoredPosition = new Vector2(0, 40);
        panel.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.14f, 0.92f);
        return panel;
    }

    private static TextMeshProUGUI CreateText(RectTransform parent, string name, Vector2 position, Vector2 size, float fontSize)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.layer = 5;
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        ApplyFont(text);
        return text;
    }

    private static TMP_FontAsset GetDefaultFont()
    {
        if (cachedDefaultFont != null)
            return cachedDefaultFont;

        cachedDefaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (cachedDefaultFont == null)
            cachedDefaultFont = TMP_Settings.defaultFontAsset;

        return cachedDefaultFont;
    }

    private static void ApplyFont(TextMeshProUGUI text)
    {
        if (text == null) return;

        TMP_FontAsset font = GetDefaultFont();
        if (font != null)
            text.font = font;
    }

    private static TMP_InputField CreateNameInput(RectTransform parent, Vector2 position)
    {
        GameObject root = new GameObject("PlayerNameInput", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        root.layer = 5;
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(340, 44);
        rect.anchoredPosition = position;
        root.GetComponent<Image>().color = new Color(0.2f, 0.22f, 0.28f, 1f);

        GameObject textArea = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        textArea.layer = 5;
        RectTransform textAreaRect = textArea.GetComponent<RectTransform>();
        textAreaRect.SetParent(rect, false);
        StretchFull(textAreaRect);

        GameObject placeholderGo = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
        placeholderGo.layer = 5;
        placeholderGo.transform.SetParent(textAreaRect, false);
        TextMeshProUGUI placeholder = placeholderGo.GetComponent<TextMeshProUGUI>();
        placeholder.text = "Player name";
        ApplyFont(placeholder);
        placeholder.fontSize = 22;
        placeholder.color = new Color(1f, 1f, 1f, 0.45f);
        StretchFull(placeholderGo.GetComponent<RectTransform>());

        GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.layer = 5;
        textGo.transform.SetParent(textAreaRect, false);
        TextMeshProUGUI text = textGo.GetComponent<TextMeshProUGUI>();
        ApplyFont(text);
        text.fontSize = 22;
        text.color = Color.white;
        StretchFull(textGo.GetComponent<RectTransform>());

        TMP_InputField input = root.GetComponent<TMP_InputField>();
        input.textViewport = textAreaRect;
        input.textComponent = text;
        input.placeholder = placeholder;
        return input;
    }

    private static Button CreateButton(RectTransform parent, string label, Vector2 position, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.layer = 5;
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(210, 46);
        rect.anchoredPosition = position;
        go.GetComponent<Image>().color = new Color(0.22f, 0.48f, 0.28f, 1f);

        TextMeshProUGUI text = CreateText(rect, "Label", Vector2.zero, rect.sizeDelta, 20);
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        Button button = go.GetComponent<Button>();
        button.onClick.AddListener(onClick);
        return button;
    }

    private static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(12, 8);
        rect.offsetMax = new Vector2(-12, -8);
    }
}
