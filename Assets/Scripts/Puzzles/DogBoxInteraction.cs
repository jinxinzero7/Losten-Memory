using UnityEngine;

public class DogBoxInteraction : MonoBehaviour, IWorldInteractable
{
    public string requiredItem = "DogFood";
    public string memoryKey = "dog_memory";
    public string memoryTitle = "Воспоминание о собаке";
    [TextArea] public string memoryDescription = "Пример описания финальной фотокарточки рядом с будкой.";
    [TextArea] public string memoryCutsceneText = "Пример внутреннего монолога после встречи с собакой.";
    public GameObject interactionText;

    private ThoughtPrompt interactionPrompt;
    private PlayerInteractionController interactionController;
    private Collider2D interactionCollider;

    public Transform InteractionTransform => transform;
    public int InteractionPriority => 90;
    public bool CanInteract => !DemoQuest.IsMemoryUnlocked(memoryKey);

    private void Start()
    {
        interactionCollider = GetComponent<Collider2D>();
        interactionText = ThoughtPrompt.EnsurePrompt(interactionText, "DogBoxPrompt", "E - покормить", transform, new Vector3(0f, 1.45f, 0f), 340f);
        interactionPrompt = ThoughtPrompt.Ensure(interactionText);
    }

    public void Interact()
    {
        if (!CanInteract) return;

        if (!Inventory.HasItem(requiredItem))
        {
            MemoryPresentation.Show("Нужен корм", "Сначала нужно найти корм для собаки.", "Я взяла корм не просто так. Надо вернуться, когда он будет у меня.", null);
            return;
        }

        Inventory.RemoveItem(requiredItem);
        SpawnMemory();
        SetInteractionHighlighted(false);
    }

    private void SpawnMemory()
    {
        if (GameObject.Find("DogMemoryFragment") != null) return;

        GameObject fragment = new GameObject("DogMemoryFragment");
        fragment.transform.position = transform.position + new Vector3(1.15f, -0.3f, 0f);
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
}
