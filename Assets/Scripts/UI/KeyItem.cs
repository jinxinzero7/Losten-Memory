using UnityEngine;

public class KeyItem : MonoBehaviour, IWorldInteractable
{
    public GameObject interactionText;
    public string keyID = "MainKey";

    private ThoughtPrompt interactionPrompt;
    private PlayerInteractionController interactionController;
    private Collider2D interactionCollider;

    public Transform InteractionTransform => transform;
    public int InteractionPriority => 100;
    public bool CanInteract => true;

    private void Start()
    {
        interactionCollider = GetComponent<Collider2D>();
        if (KeyInventory.IsKeyCollected(keyID))
        {
            Destroy(gameObject);
            return;
        }

        interactionText = ThoughtPrompt.EnsurePrompt(interactionText, "KeyPrompt", "E - взять ключ", transform, new Vector3(0f, 1.1f, 0f), 340f);
        interactionPrompt = ThoughtPrompt.Ensure(interactionText);
    }

    public void Interact()
    {
        if (!CanInteract) return;

        if (Inventory.Instance == null)
        {
            Debug.LogError("Inventory не найден на сцене.");
            return;
        }

        Inventory.AddItem("Key");
        KeyInventory.MarkKeyCollected(keyID);
        Destroy(gameObject);
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
