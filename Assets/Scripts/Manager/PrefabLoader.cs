using UnityEngine;

public class PrefabLoader : MonoBehaviour
{
    public GameObject gameManagerPrefab;
    public GameObject inventoryPrefab;

    void Awake()
    {
        // Создаём GameManager, если его нет
        if (GameManager.Instance == null && gameManagerPrefab != null)
        {
            Instantiate(gameManagerPrefab);
        }

        // Создаём Inventory, если его нет
        if (Inventory.Instance == null && inventoryPrefab != null)
        {
            Instantiate(inventoryPrefab);
        }
    }
}