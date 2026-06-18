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

    void Update()
    {
        if (playerNear && GameInput.InteractPressed)
        {
            if (CanOpen())
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetSpawnPoint(transform.position);
                }

                SceneManager.LoadScene(targetScene);
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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;
            if (interactionText != null)
                interactionText.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            if (interactionText != null)
                interactionText.SetActive(false);
        }
    }
}
