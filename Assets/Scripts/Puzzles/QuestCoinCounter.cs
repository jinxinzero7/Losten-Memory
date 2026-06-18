using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuestCoinCounter : MonoBehaviour
{
    private TMP_Text counterText;
    private int displayedCount = -1;
    private bool displayedHandedIn;

    public static void EnsureCreated()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != "Game" && sceneName != "GameScene2" && sceneName != "GameScene3") return;
        if (FindAnyObjectByType<QuestCoinCounter>() != null) return;

        GameObject counterObject = new GameObject("QuestCoinCounter");
        counterObject.AddComponent<QuestCoinCounter>().BuildUi();
    }

    private void Update()
    {
        Refresh();
    }

    private void BuildUi()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 80;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        gameObject.AddComponent<GraphicRaycaster>();

        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(transform, false);
        Image background = panel.AddComponent<Image>();
        background.color = new Color(0.08f, 0.08f, 0.1f, 0.86f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.anchoredPosition = new Vector2(-24f, -24f);
        panelRect.sizeDelta = new Vector2(230f, 54f);

        GameObject label = new GameObject("Text");
        label.transform.SetParent(panel.transform, false);
        counterText = label.AddComponent<TextMeshProUGUI>();
        counterText.fontSize = 24f;
        counterText.color = Color.white;
        counterText.alignment = TextAlignmentOptions.Center;
        counterText.textWrappingMode = TextWrappingModes.NoWrap;

        RectTransform textRect = label.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12f, 4f);
        textRect.offsetMax = new Vector2(-12f, -4f);

        Refresh();
    }

    private void Refresh()
    {
        if (counterText == null) return;

        int count = Mathf.Min(DemoQuest.CollectedCoinCount, 3);
        bool handedIn = DemoQuest.AreCoinsHandedIn;
        if (count == displayedCount && handedIn == displayedHandedIn) return;

        displayedCount = count;
        displayedHandedIn = handedIn;
        counterText.text = handedIn ? "Монеты переданы" : $"Монеты: {count}/3";
    }
}
