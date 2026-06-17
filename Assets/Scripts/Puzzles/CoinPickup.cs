using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public int coinValue = 1;
    public string coinID;
    public GameObject interactionText;

    private bool playerNear;

    void Start()
    {
        if (DemoQuest.IsCoinCollected(coinID))
        {
            Destroy(gameObject);
            return;
        }

        if (interactionText != null)
        {
            interactionText.SetActive(false);
        }
    }

    void Update()
    {
        if (playerNear && Input.GetKeyDown(KeyCode.E))
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
        if (interactionText != null)
        {
            interactionText.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerNear = false;
        if (interactionText != null)
        {
            interactionText.SetActive(false);
        }
    }
}
