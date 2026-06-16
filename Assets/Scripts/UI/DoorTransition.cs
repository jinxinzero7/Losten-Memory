using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorTransition : MonoBehaviour
{
    public string targetScene;
    public bool requireKey = true;
    public GameObject interactionText;
    private bool playerNear = false;

    void Update()
    {
        if (playerNear && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Нажата E у двери. requireKey=" + requireKey);
            Debug.Log("Inventory.HasItem(Key)=" + Inventory.HasItem("Key"));

            if (!requireKey || Inventory.HasItem("Key"))
            {
                Debug.Log("Дверь открыта! Переход...");
                GameManager.Instance.SetSpawnPoint(transform.position);
                SceneManager.LoadScene(targetScene);
            }
            else
            {
                Debug.Log("Нужен ключ!");
            }
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