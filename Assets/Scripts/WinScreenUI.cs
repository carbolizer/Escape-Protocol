using HighScore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinScreenUI : MonoBehaviour
{
    [Header("UI References (optional — built at runtime if empty)")]
    public TextMeshProUGUI scoreSummaryText;
    public TMP_InputField playerNameInput;
    public Button submitScoreButton;
    public TextMeshProUGUI submitStatusText;

    private bool scoreSubmitted;

    private void Start()
    {
        EnsureUI();
        RefreshScoreDisplay();
    }

    public void RefreshScoreDisplay()
    {
        if (GameManager.Instance == null || scoreSummaryText == null) return;

        GameManager gm = GameManager.Instance;
        scoreSummaryText.text =
            "Run Complete!\n\n" +
            $"Final Score: {gm.FinalScore}\n" +
            $"Enemies Defeated: {gm.EnemiesKilled} (+{gm.KillScore} pts)\n" +
            $"Time: {gm.GetFormattedRunTime()} (+{gm.TimeBonus} pts)\n\n" +
            "Enter your name and submit to the leaderboard.";
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

    private void EnsureUI()
    {
        if (scoreSummaryText != null && playerNameInput != null && submitScoreButton != null)
            return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        RectTransform panel = new GameObject("ScorePanel", typeof(RectTransform)).GetComponent<RectTransform>();
        panel.SetParent(canvas.transform, false);
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(700, 420);
        panel.anchoredPosition = new Vector2(0, 80);

        scoreSummaryText = CreateText(panel, "ScoreSummary", new Vector2(0, 120), new Vector2(680, 200), 22);
        scoreSummaryText.alignment = TextAlignmentOptions.Center;

        playerNameInput = CreateNameInput(panel, new Vector2(0, -40));
        submitScoreButton = CreateButton(panel, "Submit Score", new Vector2(-120, -130), SubmitScore);
        submitStatusText = CreateText(panel, "SubmitStatus", new Vector2(120, -130), new Vector2(260, 40), 18);
        submitStatusText.alignment = TextAlignmentOptions.Left;

        // Shrink the fullscreen legacy button so it does not block the score UI.
        foreach (Button button in canvas.GetComponentsInChildren<Button>(true))
        {
            if (button == submitScoreButton) continue;
            RectTransform rect = button.GetComponent<RectTransform>();
            if (rect != null && rect.sizeDelta.x > 500)
            {
                rect.sizeDelta = new Vector2(220, 50);
                rect.anchoredPosition = new Vector2(0, -280);
                TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = "Main Menu";
            }
        }
    }

    private static TextMeshProUGUI CreateText(RectTransform parent, string name, Vector2 position, Vector2 size, float fontSize)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.color = Color.white;
        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;
        return text;
    }

    private static TMP_InputField CreateNameInput(RectTransform parent, Vector2 position)
    {
        GameObject root = new GameObject("PlayerNameInput", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(320, 42);
        rect.anchoredPosition = position;
        root.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        GameObject textArea = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        RectTransform textAreaRect = textArea.GetComponent<RectTransform>();
        textAreaRect.SetParent(rect, false);
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.offsetMin = new Vector2(10, 6);
        textAreaRect.offsetMax = new Vector2(-10, -6);

        GameObject placeholderGo = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
        placeholderGo.transform.SetParent(textAreaRect, false);
        TextMeshProUGUI placeholder = placeholderGo.GetComponent<TextMeshProUGUI>();
        placeholder.text = "Player name";
        if (TMP_Settings.defaultFontAsset != null)
            placeholder.font = TMP_Settings.defaultFontAsset;
        placeholder.fontSize = 20;
        placeholder.color = new Color(1f, 1f, 1f, 0.45f);
        StretchFull(placeholderGo.GetComponent<RectTransform>());

        GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(textAreaRect, false);
        TextMeshProUGUI text = textGo.GetComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = 20;
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
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(200, 44);
        rect.anchoredPosition = position;
        go.GetComponent<Image>().color = new Color(0.25f, 0.45f, 0.25f, 1f);

        TextMeshProUGUI text = CreateText(rect, "Label", Vector2.zero, rect.sizeDelta, 20);
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;

        Button button = go.GetComponent<Button>();
        button.onClick.AddListener(onClick);
        return button;
    }

    private static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
