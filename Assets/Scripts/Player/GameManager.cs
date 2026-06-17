using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Vector3 lastSpawnPoint;
    public string lastScene;
    public bool isFirstLaunch = true;

    private bool hasSpawned = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            isFirstLaunch = PlayerPrefs.GetInt("FirstLaunch", 1) == 1;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        hasSpawned = false;

        // Телепортируем ДО того, как игрок появится на экране
        TeleportPlayerImmediately();
    }

    void TeleportPlayerImmediately()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player не найден, пробуем ещё раз...");
            Invoke("TeleportPlayerImmediately", 0.01f);
            return;
        }

        if (!hasSpawned)
        {
            Vector3 spawnPosition = GetSpawnPosition();
            player.transform.position = spawnPosition;
            hasSpawned = true;
        }
    }

    Vector3 GetSpawnPosition()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Используем твои реальные имена сцен
        if (isFirstLaunch && currentScene == "Game")  // ← Game вместо Location1
        {
            GameObject startPoint = GameObject.Find("StartPoint");
            if (startPoint != null)
            {
                isFirstLaunch = false;
                PlayerPrefs.SetInt("FirstLaunch", 0);
                PlayerPrefs.Save();

                return startPoint.transform.position;
            }
            else
            {
                Debug.LogWarning("StartPoint не найден на сцене " + currentScene);
            }
        }

        GameObject spawnPoint = GameObject.Find("SpawnPoint");
        if (spawnPoint != null)
        {
            return spawnPoint.transform.position;
        }

        return Vector3.zero;
    }

    public void SetSpawnPoint(Vector3 position)
    {
        lastSpawnPoint = position;
    }

    public static void ResetFirstLaunch()
    {
        PlayerPrefs.SetInt("FirstLaunch", 1);
        PlayerPrefs.Save();

        if (Instance != null)
        {
            Instance.isFirstLaunch = true;
            Instance.hasSpawned = false;
        }
    }
}
