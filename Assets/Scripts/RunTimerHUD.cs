using TMPro;
using UnityEngine;

[RequireComponent(typeof(GameManager))]
public class RunTimerHUD : MonoBehaviour
{
    private TextMeshProUGUI timerText;
    private Canvas hudCanvas;

    private void Start()
    {
        CreateHudIfNeeded();
    }

    private void Update()
    {
        if (timerText == null || GameManager.Instance == null) return;

        bool show = GameManager.Instance.IsRunActive && !GameManager.Instance.RunFinalized;
        if (hudCanvas != null)
            hudCanvas.enabled = show;
        if (!show) return;

        timerText.text = "Time: " + GameManager.Instance.GetFormattedRunTime();
    }

    private void CreateHudIfNeeded()
    {
        if (timerText != null) return;

        GameObject canvasGo = new GameObject("RunTimerCanvas");
        canvasGo.transform.SetParent(transform);
        hudCanvas = canvasGo.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = 50;
        canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        GameObject textGo = new GameObject("RunTimerText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(canvasGo.transform, false);

        RectTransform rect = textGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-20, -20);
        rect.sizeDelta = new Vector2(220, 40);

        timerText = textGo.GetComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null)
            timerText.font = TMP_Settings.defaultFontAsset;
        timerText.fontSize = 24;
        timerText.color = Color.white;
        timerText.alignment = TextAlignmentOptions.Right;
    }
}
