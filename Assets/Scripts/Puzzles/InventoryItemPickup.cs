using UnityEngine;

public class InventoryItemPickup : MonoBehaviour, IWorldInteractable
{
    public string itemId = "DogFood";
    public string promptText = "E - взять";
    public GameObject interactionText;

    private bool playerNear;
    private ThoughtPrompt interactionPrompt;
    private PlayerInteractionController interactionController;
    private Collider2D interactionCollider;

    public Transform InteractionTransform => transform;
    public int InteractionPriority => 100;
    public bool CanInteract => playerNear;

    private void Start()
    {
        interactionCollider = GetComponent<Collider2D>();
        if (Inventory.HasItem(itemId))
        {
            Destroy(gameObject);
            return;
        }

        interactionText = ThoughtPrompt.EnsurePrompt(interactionText, gameObject.name + "Prompt", promptText, transform, new Vector3(0f, 1.0f, 0f), 300f);
        interactionPrompt = ThoughtPrompt.Ensure(interactionText);
    }

    public void Interact()
    {
        if (!CanInteract) return;

        Inventory.AddItem(itemId);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerNear = true;
        interactionController = other.GetComponent<PlayerInteractionController>();
        interactionController?.Register(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerNear = false;
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
