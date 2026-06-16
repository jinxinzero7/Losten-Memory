using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public int coinValue = 1;
    public GameObject interactionText;

    private bool playerNear;

    void Start()
    {
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
