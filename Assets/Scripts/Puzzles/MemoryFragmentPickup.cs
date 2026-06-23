using UnityEngine;

public class MemoryFragmentPickup : MonoBehaviour
{
    public MemoryPiece memory;
    public string fallbackMemoryKey = "memory_01";
    public string fallbackTitle = "Воспоминание 1";
    [TextArea] public string fallbackDescription = "Пример описания найденной фотокарточки.";
    [TextArea] public string fallbackCutsceneText = "Пример внутреннего монолога. Героиня начинает узнавать место на фотографии.";
    public GameObject interactionText;

    private bool playerNear;
    private bool collected;
    private ThoughtPrompt interactionPrompt;

    private string MemoryKey => memory != null && !string.IsNullOrWhiteSpace(memory.memoryKey)
        ? memory.memoryKey
        : fallbackMemoryKey;

    private void Start()
    {
        if (DemoQuest.IsMemoryUnlocked(MemoryKey))
        {
            Destroy(gameObject);
            return;
        }

        interactionText = ThoughtPrompt.EnsurePrompt(interactionText, "MemoryPrompt", "E - воспоминание", transform, new Vector3(0f, 1.1f, 0f), 380f);
        interactionPrompt = ThoughtPrompt.Ensure(interactionText);
    }

    private void Update()
    {
        if (!collected && DemoQuest.IsMemoryUnlocked(MemoryKey))
        {
            Destroy(gameObject);
            return;
        }

        if (!playerNear || collected || !GameInput.InteractPressed) return;

        string title = memory != null && !string.IsNullOrWhiteSpace(memory.memoryTitle)
            ? memory.memoryTitle
            : fallbackTitle;
        string description = memory != null && !string.IsNullOrWhiteSpace(memory.memoryDescription)
            ? memory.memoryDescription
            : fallbackDescription;
        string cutsceneText = memory != null && !string.IsNullOrWhiteSpace(memory.cutsceneText)
            ? memory.cutsceneText
            : fallbackCutsceneText;

        collected = true;
        if (interactionPrompt != null)
        {
            interactionPrompt.Hide();
        }

        DemoQuest.UnlockMemory(MemoryKey, title);
        Inventory.AddMemory(title);
        MemoryPresentation.Show(title, description, cutsceneText, memory != null ? memory.memoryImage : null);
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerNear = true;
        if (interactionPrompt != null)
        {
            interactionPrompt.Show();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerNear = false;
        if (interactionPrompt != null)
        {
            interactionPrompt.Hide();
        }
    }
}
