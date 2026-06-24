using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DogBoxInteraction : MonoBehaviour, IWorldInteractable
{
    public string requiredItem = "DogFood";
    public string memoryKey = "dog_memory";
    public string memoryTitle = "Воспоминание о собаке";
    [TextArea] public string memoryDescription = "Пример описания финальной фотокарточки рядом с будкой.";
    [TextArea] public string memoryCutsceneText = "Пример внутреннего монолога после встречи с собакой.";
    public GameObject interactionText;

    private bool hasFedDog;
    private GameObject dialogueCanvas;
    private ThoughtPrompt interactionPrompt;
    private PlayerInteractionController interactionController;
    private Collider2D interactionCollider;
    private PlayerController player;

    public Transform InteractionTransform => transform;
    public int InteractionPriority => 90;
    public bool CanInteract => !hasFedDog && !DemoQuest.IsMemoryUnlocked(memoryKey) && dialogueCanvas == null;

    private void Start()
    {
        interactionCollider = GetComponent<Collider2D>();
        interactionText = ThoughtPrompt.EnsurePrompt(interactionText, "DogBoxPrompt", "E - будка", transform, new Vector3(0f, 1.45f, 0f), 340f);
        interactionPrompt = ThoughtPrompt.Ensure(interactionText);
    }

    public void Interact()
    {
        if (!CanInteract) return;

        SetInteractionHighlighted(false);
        ShowDogDialogue();
    }

    private void ShowDogDialogue()
    {
        player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            player.SetMovementBlocked(true);
        }

        dialogueCanvas = new GameObject("DogDialogueCanvas");
        Canvas canvas = dialogueCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 220;

        CanvasScaler scaler = dialogueCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        dialogueCanvas.AddComponent<GraphicRaycaster>();

        Image backdrop = CreateImage(dialogueCanvas.transform, "Backdrop", new Color(0f, 0f, 0f, 0.55f));
        Stretch(backdrop.rectTransform);

        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(dialogueCanvas.transform, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.94f, 0.92f, 0.86f, 0.96f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0f, 0f);
        panelRect.sizeDelta = new Vector2(720f, 340f);

        TMP_Text text = CreateText(panel.transform, "Text", Inventory.HasItem(requiredItem)
            ? "У будки слышно тихое дыхание. Отдать найденный корм?"
            : "Здесь кто-то есть, но без корма лучше не подходить.", 28f);
        text.color = Color.black;
        SetRect(text.rectTransform, new Vector2(40f, -34f), new Vector2(640f, 150f), new Vector2(0f, 1f), new Vector2(0f, 1f));

        Button giveButton = CreateButton(panel.transform, Inventory.HasItem(requiredItem) ? "Отдать корм" : "Корма нет", GiveFood);
        giveButton.interactable = Inventory.HasItem(requiredItem);
        SetRect(giveButton.GetComponent<RectTransform>(), new Vector2(76f, 44f), new Vector2(250f, 66f), new Vector2(0f, 0f), new Vector2(0f, 0f));

        Button closeButton = CreateButton(panel.transform, "Уйти", CloseDialogue);
        SetRect(closeButton.GetComponent<RectTransform>(), new Vector2(394f, 44f), new Vector2(250f, 66f), new Vector2(0f, 0f), new Vector2(0f, 0f));
    }

    private void GiveFood()
    {
        if (!Inventory.HasItem(requiredItem)) return;

        Inventory.RemoveItem(requiredItem);
        hasFedDog = true;
        ApplyFedPlaceholderSprite();
        SpawnMemory();
        CloseDialogue();
    }

    private void CloseDialogue()
    {
        if (player != null)
        {
            player.SetMovementBlocked(false);
            player = null;
        }

        if (dialogueCanvas != null)
        {
            Destroy(dialogueCanvas);
            dialogueCanvas = null;
        }
    }

    private void ApplyFedPlaceholderSprite()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sprite = CreateGreenPlaceholderSprite();
            renderer.color = Color.white;
            renderer.sortingOrder = Mathf.Max(renderer.sortingOrder, 7);
        }
    }

    private void SpawnMemory()
    {
        if (GameObject.Find("DogMemoryFragment") != null) return;

        GameObject fragment = new GameObject("DogMemoryFragment");
        Vector3 spawnPosition = player != null
            ? player.transform.position + new Vector3(0.75f, 0f, 0f)
            : transform.position + new Vector3(0.85f, -0.25f, 0f);
        fragment.transform.position = spawnPosition;
        fragment.transform.localScale = Vector3.one * 0.42f;

        SpriteRenderer renderer = fragment.AddComponent<SpriteRenderer>();
        renderer.sprite = RuntimeSpriteLoader.LoadProjectSprite("Assets/Art/Sprites/places/newSprites/3/photo.PNG", 100f);
        renderer.sortingOrder = 8;

        CircleCollider2D collider = fragment.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.8f;

        MemoryFragmentPickup pickup = fragment.AddComponent<MemoryFragmentPickup>();
        pickup.fallbackMemoryKey = memoryKey;
        pickup.fallbackTitle = memoryTitle;
        pickup.fallbackDescription = memoryDescription;
        pickup.fallbackCutsceneText = memoryCutsceneText;
        interactionController?.Register(pickup);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        interactionController = other.GetComponent<PlayerInteractionController>();
        interactionController?.Register(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        interactionController?.Unregister(this);
        interactionController = null;
        if (dialogueCanvas != null)
        {
            CloseDialogue();
        }
    }

    private void OnDisable()
    {
        interactionController?.Unregister(this);
        SetInteractionHighlighted(false);
        if (dialogueCanvas != null)
        {
            CloseDialogue();
        }
    }

    public Vector2 GetInteractionPoint(Vector2 playerPosition)
    {
        return interactionCollider != null ? interactionCollider.ClosestPoint(playerPosition) : (Vector2)transform.position;
    }

    public void SetInteractionHighlighted(bool highlighted)
    {
        if (interactionPrompt != null)
        {
            if (highlighted) interactionPrompt.Show();
            else interactionPrompt.Hide();
        }
        else if (interactionText != null)
        {
            interactionText.SetActive(highlighted);
        }
    }

    private static Image CreateImage(Transform parent, string objectName, Color color)
    {
        GameObject imageObject = new GameObject(objectName);
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private static TMP_Text CreateText(Transform parent, string objectName, string value, float fontSize)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.alignment = TextAlignmentOptions.Center;
        return text;
    }

    private static Button CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(label);
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.78f, 0.76f, 0.68f, 1f);
        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(onClick);

        TMP_Text text = CreateText(buttonObject.transform, "Text", label, 24f);
        text.color = Color.black;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        Stretch(text.rectTransform);
        return button;
    }

    private static void SetRect(RectTransform rect, Vector2 position, Vector2 size, Vector2 anchor, Vector2 pivot)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
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

    private static Sprite CreateGreenPlaceholderSprite()
    {
        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color fill = new Color(0.18f, 0.75f, 0.28f, 1f);
        Color edge = new Color(0.05f, 0.35f, 0.1f, 1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool border = x < 2 || y < 2 || x > size - 3 || y > size - 3;
                texture.SetPixel(x, y, border ? edge : fill);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 32f);
    }
}
