using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Vector3 lastSpawnPoint;
    public string lastScene;
    public bool isFirstLaunch = true;

    private bool hasSpawned = false;
    private Coroutine teleportRoutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            ResetRuntimeProgress();
            isFirstLaunch = true;

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

        if (teleportRoutine != null)
        {
            StopCoroutine(teleportRoutine);
            teleportRoutine = null;
        }

        if (scene.name == "MainMenu" || SaveGameService.IsRestoring) return;

        teleportRoutine = StartCoroutine(TeleportPlayerWhenReady(scene.name));
    }

    IEnumerator TeleportPlayerWhenReady(string sceneName)
    {
        const int maxWaitFrames = 60;
        for (int frame = 0; frame < maxWaitFrames; frame++)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                if (!hasSpawned)
                {
                    player.transform.position = GetSpawnPosition();
                    hasSpawned = true;
                }

                teleportRoutine = null;
                yield break;
            }

            yield return null;
        }

        Debug.LogWarning("Player не найден после загрузки сцены " + sceneName);
        teleportRoutine = null;
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
        ResetRuntimeProgress();

        if (Instance != null)
        {
            Instance.isFirstLaunch = true;
            Instance.hasSpawned = false;
        }
    }

    private static void ResetRuntimeProgress()
    {
        DemoQuest.ResetAll();
        KeyInventory.ResetAll();
        Inventory.ResetAll();
    }
}
