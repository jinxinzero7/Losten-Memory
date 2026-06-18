using UnityEngine;

public class KeyItem : MonoBehaviour
{
    public GameObject interactionText;
    public string keyID = "MainKey";

    private bool playerNear;

    private void Start()
    {
        if (KeyInventory.IsKeyCollected(keyID))
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!playerNear || !GameInput.InteractPressed) return;

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

        playerNear = true;
        if (interactionText != null)
        {
            interactionText.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerNear = false;
        if (interactionText != null)
        {
            interactionText.SetActive(false);
        }
    }
}
