using UnityEngine;

public class KeyItem : MonoBehaviour
{
    public GameObject interactionText;
    public string keyID = "MainKey";
    private bool playerNear = false;

    void Start()
    {
        // Если ключ уже был взят - удаляем
        if (KeyInventory.IsKeyCollected(keyID))
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (playerNear && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Подбираем ключ!");

            // Проверяем, что Inventory существует
            if (Inventory.Instance == null)
            {
                Debug.LogError("Inventory не найден! Создайте объект Inventory на сцене.");
                return;
            }

            Inventory.AddItem("Key");
            KeyInventory.MarkKeyCollected(keyID);
            Destroy(gameObject);
        }
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