using UnityEngine;

public class MemoryFragmentPickup : MonoBehaviour
{
    public MemoryPiece memory;
    public string fallbackMemoryKey = "memory_01";
    public string fallbackTitle = "Воспоминание 1";
    [TextArea] public string fallbackDescription = "Пример описания найденной фотокарточки.";
    [TextArea] public string fallbackCutsceneText = "Пример внутреннего монолога. Героиня начинает узнавать место на фотографии.";

    private bool playerNear;
    private bool collected;

    private string MemoryKey => memory != null && !string.IsNullOrWhiteSpace(memory.memoryKey)
        ? memory.memoryKey
        : fallbackMemoryKey;

    private void Start()
    {
        if (DemoQuest.IsMemoryUnlocked(MemoryKey))
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!collected && DemoQuest.IsMemoryUnlocked(MemoryKey))
        {
            Destroy(gameObject);
            return;
        }

        if (!playerNear || collected || !Input.GetKeyDown(KeyCode.E)) return;

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
        DemoQuest.UnlockMemory(MemoryKey, title);
        Inventory.AddMemory(title);
        MemoryPresentation.Show(title, description, cutsceneText, memory != null ? memory.memoryImage : null);
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
        }
    }
}
