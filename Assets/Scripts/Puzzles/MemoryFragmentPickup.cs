using UnityEngine;

public class MemoryFragmentPickup : MonoBehaviour, IWorldInteractable
{
    public MemoryPiece memory;
    public string fallbackMemoryKey = "memory_01";
    public string fallbackTitle = "Воспоминание 1";
    [TextArea] public string fallbackDescription = "Пример описания найденной фотокарточки.";
    [TextArea] public string fallbackCutsceneText = "Пример внутреннего монолога. Героиня начинает узнавать место на фотографии.";
    public GameObject interactionText;

    private bool collected;
    private ThoughtPrompt interactionPrompt;
    private PlayerInteractionController interactionController;
    private Collider2D interactionCollider;

    public Transform InteractionTransform => transform;
    public int InteractionPriority => 100;
    public bool CanInteract => !collected;

    private string MemoryKey => memory != null && !string.IsNullOrWhiteSpace(memory.memoryKey)
        ? memory.memoryKey
        : fallbackMemoryKey;

    private void Start()
    {
        interactionCollider = GetComponent<Collider2D>();
        if (DemoQuest.IsMemoryUnlocked(MemoryKey))
        {
            Destroy(gameObject);
            return;
        }

        interactionText = ThoughtPrompt.EnsurePrompt(interactionText, "MemoryPrompt", "E - воспоминание", transform, new Vector3(0f, 1.1f, 0f), 380f);
        interactionPrompt = ThoughtPrompt.Ensure(interactionText);
        RegisterMemoryData();
    }

    private void Update()
    {
        if (!collected && DemoQuest.IsMemoryUnlocked(MemoryKey))
        {
            Destroy(gameObject);
        }
    }

    public void Interact()
    {
        if (!CanInteract) return;

        string title = GetTitle();
        string description = GetDescription();
        string cutsceneText = GetCutsceneText();
        Sprite image = GetImage();

        collected = true;
        SetInteractionHighlighted(false);

        MemoryArchive.Register(MemoryKey, title, description, cutsceneText, image);
        DemoQuest.UnlockMemory(MemoryKey, title);
        Inventory.AddMemory(title);
        MemoryPresentation.Show(title, description, cutsceneText, image);
        gameObject.SetActive(false);
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
    }

    private void OnDisable()
    {
        interactionController?.Unregister(this);
        SetInteractionHighlighted(false);
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

    private void RegisterMemoryData()
    {
        MemoryArchive.Register(MemoryKey, GetTitle(), GetDescription(), GetCutsceneText(), GetImage());
    }

    private string GetTitle()
    {
        return memory != null && !string.IsNullOrWhiteSpace(memory.memoryTitle)
            ? memory.memoryTitle
            : fallbackTitle;
    }

    private string GetDescription()
    {
        return memory != null && !string.IsNullOrWhiteSpace(memory.memoryDescription)
            ? memory.memoryDescription
            : fallbackDescription;
    }

    private string GetCutsceneText()
    {
        return memory != null && !string.IsNullOrWhiteSpace(memory.cutsceneText)
            ? memory.cutsceneText
            : fallbackCutsceneText;
    }

    private Sprite GetImage()
    {
        if (memory != null && memory.memoryImage != null)
        {
            return memory.memoryImage;
        }

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        return spriteRenderer != null ? spriteRenderer.sprite : null;
    }
}
