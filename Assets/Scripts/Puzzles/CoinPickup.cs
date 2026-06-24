using UnityEngine;

public class CoinPickup : MonoBehaviour, IWorldInteractable
{
    public int coinValue = 1;
    public string coinID;
    public GameObject interactionText;

    private ThoughtPrompt interactionPrompt;
    private PlayerInteractionController interactionController;
    private Collider2D interactionCollider;

    public Transform InteractionTransform => transform;
    public int InteractionPriority => 100;
    public bool CanInteract => true;

    void Start()
    {
        interactionCollider = GetComponent<Collider2D>();
        if (DemoQuest.IsCoinCollected(coinID))
        {
            Destroy(gameObject);
            return;
        }

        interactionText = ThoughtPrompt.EnsurePrompt(interactionText, "CoinPrompt", "E - взять", transform, new Vector3(0f, 0.8f, 0f), 300f);
        interactionPrompt = ThoughtPrompt.Ensure(interactionText);
    }

    public void Interact()
    {
        if (!CanInteract) return;

        interactionController?.Unregister(this);
        DestroyInteractionPrompt();
        Inventory.AddCoins(coinValue);
        DemoQuest.MarkCoinCollected(coinID);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        interactionController = other.GetComponent<PlayerInteractionController>();
        if (interactionController != null)
        {
            interactionController.Register(this);
        }
    }

    void OnTriggerExit2D(Collider2D other)
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

    private void DestroyInteractionPrompt()
    {
        if (interactionText != null)
        {
            Destroy(interactionText);
            interactionText = null;
            interactionPrompt = null;
        }
    }
}
