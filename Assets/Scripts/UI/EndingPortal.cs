using UnityEngine;

public class EndingPortal : MonoBehaviour, IWorldInteractable
{
    public GameObject interactionText;

    private ThoughtPrompt interactionPrompt;
    private PlayerInteractionController interactionController;
    private Collider2D interactionCollider;

    public Transform InteractionTransform => transform;
    public int InteractionPriority => 20;
    public bool CanInteract => DemoQuest.IsMemoryUnlocked("dog_memory");

    private void Start()
    {
        interactionCollider = GetComponent<Collider2D>();
        interactionText = ThoughtPrompt.EnsurePrompt(interactionText, "PortalPrompt", "E - войти", transform, new Vector3(0f, 1.6f, 0f), 300f);
        interactionPrompt = ThoughtPrompt.Ensure(interactionText);
    }

    public void Interact()
    {
        if (!CanInteract) return;

        CreditsScreen.Show();
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
