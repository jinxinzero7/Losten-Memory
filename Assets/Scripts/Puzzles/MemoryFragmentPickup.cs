using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MemoryFragmentPickup : MonoBehaviour
{
    private bool playerNear;
    private bool collected;

    void Update()
    {
        if (!playerNear || collected || !Input.GetKeyDown(KeyCode.E)) return;

        collected = true;
        Inventory.AddMemory("Осколок прошлого");
        ShowFinalPanel();
        gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
        }
    }

    private void ShowFinalPanel()
    {
        GameObject canvasObject = new GameObject("MemoryFinalCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(canvasObject.transform, false);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.78f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(canvasObject.transform, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.08f, 0.1f, 0.13f, 0.96f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(700f, 340f);

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(42, 42, 36, 36);
        layout.spacing = 18f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandHeight = false;

        CreateText(panel.transform, "Воспоминание найдено", 34f, FontStyles.Bold, 58f);
        CreateText(panel.transform, "Ты нашла осколок прошлого. Это конец текущей демо-версии, но путь дальше теперь открыт.", 22f, FontStyles.Normal, 120f);
        CreateButton(panel.transform, "В главное меню", () => SceneManager.LoadScene("MainMenu"));
    }

    private void CreateText(Transform parent, string text, float fontSize, FontStyles style, float height)
    {
        GameObject labelObject = new GameObject("Text");
        labelObject.transform.SetParent(parent, false);
        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.Normal;

        LayoutElement layout = labelObject.AddComponent<LayoutElement>();
        layout.preferredHeight = height;
    }

    private void CreateButton(Transform parent, string text, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(text);
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.2f, 0.35f, 0.42f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(onClick);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(230f, 54f);

        LayoutElement layout = buttonObject.AddComponent<LayoutElement>();
        layout.preferredHeight = 54f;
        layout.preferredWidth = 230f;

        GameObject labelObject = new GameObject("Text");
        labelObject.transform.SetParent(buttonObject.transform, false);
        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = 22f;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
    }
}
