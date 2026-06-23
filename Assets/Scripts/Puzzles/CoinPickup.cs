using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public int coinValue = 1;
    public string coinID;
    public GameObject interactionText;

    private bool playerNear;
    private ThoughtPrompt interactionPrompt;

    void Start()
    {
        if (DemoQuest.IsCoinCollected(coinID))
        {
            Destroy(gameObject);
            return;
        }

        interactionText = ThoughtPrompt.EnsurePrompt(interactionText, "CoinPrompt", "E - взять", transform, new Vector3(0f, 0.8f, 0f), 300f);
        interactionPrompt = ThoughtPrompt.Ensure(interactionText);
    }

    void Update()
    {
        if (playerNear && GameInput.InteractPressed)
        {
            Inventory.AddCoins(coinValue);
            DemoQuest.MarkCoinCollected(coinID);
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerNear = true;
        if (interactionPrompt != null)
        {
            interactionPrompt.Show();
        }
        else if (interactionText != null)
        {
            interactionText.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerNear = false;
        if (interactionPrompt != null)
        {
            interactionPrompt.Hide();
        }
        else if (interactionText != null)
        {
            interactionText.SetActive(false);
        }
    }
}
