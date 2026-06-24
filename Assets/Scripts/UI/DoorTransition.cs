using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorTransition : MonoBehaviour, IWorldInteractable
{
    public string targetScene;
    public bool requireKey = true;
    public bool requireQuestStarted;
    public bool requireFinalPathOpen;
    public GameObject interactionText;
    private bool playerNear = false;
    private ThoughtPrompt interactionPrompt;
    private PlayerInteractionController interactionController;
    private Collider2D interactionCollider;

    public Transform InteractionTransform => transform;
    public int InteractionPriority => 10;
    public bool CanInteract => playerNear && !ScreenTransition.IsTransitioning;

    void Start()
    {
        interactionCollider = GetComponent<Collider2D>();
        interactionText = ThoughtPrompt.EnsurePrompt(interactionText, "DoorPrompt", "E - войти", transform, new Vector3(0f, 1.45f, 0f));
        interactionPrompt = ThoughtPrompt.Ensure(interactionText);
    }

    public void Interact()
    {
        if (!CanInteract) return;

        if (CanOpen())
        {
            ScreenTransition.LoadSceneWithFade(targetScene, () =>
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.PrepareSceneTransition(SceneManager.GetActiveScene().name, targetScene, transform.position);
                }
            });
        }
        else
        {
            HandleBlockedDoor();
        }
    }

    bool CanOpen()
    {
        if (requireKey && !Inventory.HasItem("Key")) return false;
        if (requireQuestStarted && !DemoQuest.IsQuestStarted) return false;
        if (requireFinalPathOpen && !DemoQuest.IsFinalPathOpen) return false;

        return true;
    }

    void HandleBlockedDoor()
    {
        if (requireQuestStarted && !DemoQuest.IsQuestStarted)
        {
            DemoQuest.MarkLockedDoorTried();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;
            interactionController = other.GetComponent<PlayerInteractionController>();
            if (interactionController != null)
            {
                interactionController.Register(this);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            interactionController?.Unregister(this);
            interactionController = null;
        }
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
