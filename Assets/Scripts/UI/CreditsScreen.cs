using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreditsScreen : MonoBehaviour
{
    public static void Show()
    {
        if (FindAnyObjectByType<CreditsScreen>() != null) return;

        GameObject canvasObject = new GameObject("CreditsCanvas");
        CreditsScreen credits = canvasObject.AddComponent<CreditsScreen>();
        credits.Build();
    }

    private void Build()
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            player.SetMovementBlocked(true);
        }

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 240;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        gameObject.AddComponent<GraphicRaycaster>();

        Image background = CreateImage(transform, "Background", Color.black);
        Stretch(background.rectTransform);

        TMP_Text text = CreateText(transform, "CreditsText", "Конец демо\n\nLosten Memory", 52f);
        Stretch(text.rectTransform);
        text.alignment = TextAlignmentOptions.Center;
    }

    private static Image CreateImage(Transform parent, string objectName, Color color)
    {
        GameObject imageObject = new GameObject(objectName);
        imageObject.transform.SetParent(parent, false);
        Image background = imageObject.AddComponent<Image>();
        background.color = Color.black;
        return background;
    }

    private static TMP_Text CreateText(Transform parent, string objectName, string value, float fontSize)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = value;
        label.fontSize = fontSize;
        label.color = Color.white;
        label.textWrappingMode = TextWrappingModes.Normal;
        return label;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
