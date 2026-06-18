using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MemoryPresentation : MonoBehaviour
{
    private PlayerController player;

    public static void Show(string title, string description, string cutsceneText, Sprite photo)
    {
        MemoryPresentation existing = FindAnyObjectByType<MemoryPresentation>();
        if (existing != null)
        {
            Destroy(existing.gameObject);
        }

        GameObject canvasObject = new GameObject("MemoryPresentationCanvas");
        MemoryPresentation presentation = canvasObject.AddComponent<MemoryPresentation>();
        presentation.Build(title, description, cutsceneText, photo);
    }

    private void Update()
    {
        if (GameInput.CancelPressed)
        {
            Close();
        }
    }

    private void Build(string title, string description, string cutsceneText, Sprite photo)
    {
        player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            player.SetMovementBlocked(true);
        }

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 160;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        gameObject.AddComponent<GraphicRaycaster>();

        Image backdrop = CreateImage(transform, "Backdrop", new Color(0.025f, 0.03f, 0.04f, 0.94f));
        Stretch(backdrop.rectTransform);

        GameObject content = new GameObject("Content");
        content.transform.SetParent(transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(1040f, 700f);

        Image polaroid = CreateImage(content.transform, "Polaroid", new Color(0.94f, 0.93f, 0.88f, 1f));
        RectTransform polaroidRect = polaroid.rectTransform;
        polaroidRect.anchorMin = new Vector2(0f, 0.5f);
        polaroidRect.anchorMax = new Vector2(0f, 0.5f);
        polaroidRect.pivot = new Vector2(0f, 0.5f);
        polaroidRect.anchoredPosition = new Vector2(10f, 0f);
        polaroidRect.sizeDelta = new Vector2(440f, 560f);
        polaroidRect.localRotation = Quaternion.Euler(0f, 0f, -2f);

        Image photoImage = CreateImage(polaroid.transform, "Photo", Color.white);
        RectTransform photoRect = photoImage.rectTransform;
        photoRect.anchorMin = new Vector2(0.5f, 1f);
        photoRect.anchorMax = new Vector2(0.5f, 1f);
        photoRect.pivot = new Vector2(0.5f, 1f);
        photoRect.anchoredPosition = new Vector2(0f, -32f);
        photoRect.sizeDelta = new Vector2(376f, 376f);
        photoImage.sprite = photo != null ? photo : CreatePlaceholderPhoto();
        photoImage.preserveAspect = true;

        TMP_Text caption = CreateText(polaroid.transform, "Caption", title, 30f, new Color(0.12f, 0.12f, 0.14f, 1f));
        RectTransform captionRect = caption.rectTransform;
        captionRect.anchorMin = new Vector2(0.5f, 0f);
        captionRect.anchorMax = new Vector2(0.5f, 0f);
        captionRect.pivot = new Vector2(0.5f, 0f);
        captionRect.anchoredPosition = new Vector2(0f, 38f);
        captionRect.sizeDelta = new Vector2(370f, 84f);
        caption.alignment = TextAlignmentOptions.Center;

        TMP_Text heading = CreateText(content.transform, "Heading", "Воспоминание найдено", 38f, Color.white);
        SetTextRect(heading.rectTransform, new Vector2(490f, 520f), new Vector2(520f, 64f));
        heading.fontStyle = FontStyles.Bold;

        TMP_Text descriptionText = CreateText(content.transform, "Description", description, 24f, new Color(0.82f, 0.85f, 0.88f, 1f));
        SetTextRect(descriptionText.rectTransform, new Vector2(490f, 340f), new Vector2(520f, 150f));

        TMP_Text monologue = CreateText(content.transform, "Monologue", cutsceneText, 25f, Color.white);
        SetTextRect(monologue.rectTransform, new Vector2(490f, 120f), new Vector2(520f, 190f));
        monologue.fontStyle = FontStyles.Italic;

        Button continueButton = CreateButton(content.transform, "Продолжить", Close);
        RectTransform buttonRect = continueButton.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1f, 0f);
        buttonRect.anchorMax = new Vector2(1f, 0f);
        buttonRect.pivot = new Vector2(1f, 0f);
        buttonRect.anchoredPosition = new Vector2(-30f, 20f);
        buttonRect.sizeDelta = new Vector2(220f, 56f);
    }

    private void Close()
    {
        if (player != null)
        {
            player.SetMovementBlocked(false);
        }

        Destroy(gameObject);
    }

    private static Image CreateImage(Transform parent, string objectName, Color color)
    {
        GameObject imageObject = new GameObject(objectName);
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private static TMP_Text CreateText(Transform parent, string objectName, string text, float fontSize, Color color)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.color = color;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.overflowMode = TextOverflowModes.Ellipsis;
        return label;
    }

    private static Button CreateButton(Transform parent, string text, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject("ContinueButton");
        buttonObject.transform.SetParent(parent, false);
        Image background = buttonObject.AddComponent<Image>();
        background.color = new Color(0.18f, 0.48f, 0.58f, 1f);
        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(onClick);

        TMP_Text label = CreateText(buttonObject.transform, "Text", text, 23f, Color.white);
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        Stretch(label.rectTransform);
        return button;
    }

    private static void SetTextRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Sprite CreatePlaceholderPhoto()
    {
        const int width = 96;
        const int height = 96;
        Texture2D texture = new Texture2D(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color color;
                if (y > 54)
                {
                    color = new Color(0.22f, 0.38f, 0.48f, 1f);
                }
                else if (x > 28 && x < 68 && y > 20 && y < 58)
                {
                    color = new Color(0.72f, 0.7f, 0.58f, 1f);
                }
                else
                {
                    color = new Color(0.16f, 0.2f, 0.22f, 1f);
                }

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 96f);
    }
}
