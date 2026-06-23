using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorTransition : MonoBehaviour
{
    public string targetScene;
    public bool requireKey = true;
    public bool requireQuestStarted;
    public bool requireFinalPathOpen;
    public GameObject interactionText;
    private bool playerNear = false;
    private ThoughtPrompt interactionPrompt;

    void Start()
    {
        interactionText = ThoughtPrompt.EnsurePrompt(interactionText, "DoorPrompt", "E - войти", transform, new Vector3(0f, 1.45f, 0f));
        interactionPrompt = ThoughtPrompt.Ensure(interactionText);
    }

    void Update()
    {
        if (playerNear && GameInput.InteractPressed && !ScreenTransition.IsTransitioning)
        {
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
            if (interactionPrompt != null)
                interactionPrompt.Show();
            else if (interactionText != null)
                interactionText.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            if (interactionPrompt != null)
                interactionPrompt.Hide();
            else if (interactionText != null)
                interactionText.SetActive(false);
        }
    }
}
