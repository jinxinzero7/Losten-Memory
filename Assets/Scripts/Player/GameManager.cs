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
    private string transitionSourceScene;

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
                    ApplyScenePlayerSettings(player, sceneName);
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

        if (currentScene == "GameScene4")
        {
            return new Vector3(-1.25f, -5.75f, 0.05f);
        }

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
            if (currentScene == "GameScene3" && transitionSourceScene == "GameScene2")
            {
                Vector3 puzzleRoomEntry = GetPositionAboveDoor("DoorBack");
                if (puzzleRoomEntry != Vector3.zero)
                {
                    return puzzleRoomEntry;
                }
            }

            return spawnPoint.transform.position;
        }

        return Vector3.zero;
    }

    private void ApplyScenePlayerSettings(GameObject player, string sceneName)
    {
        if (player == null) return;

        if (sceneName == "GameScene4")
        {
            player.transform.localScale = new Vector3(0.147609f, 0.105523f, 1f);
        }
    }

    public void SetSpawnPoint(Vector3 position)
    {
        lastSpawnPoint = position;
    }

    public void PrepareSceneTransition(string sourceScene, string targetScene, Vector3 sourceDoorPosition)
    {
        lastScene = sourceScene;
        transitionSourceScene = sourceScene;
        lastSpawnPoint = sourceDoorPosition;
    }

    private Vector3 GetPositionAboveDoor(string doorName)
    {
        GameObject door = GameObject.Find(doorName);
        if (door == null) return Vector3.zero;

        Collider2D collider = door.GetComponent<Collider2D>();
        if (collider != null)
        {
            return new Vector3(door.transform.position.x, collider.bounds.max.y + 0.8f, 0f);
        }

        Renderer renderer = door.GetComponent<Renderer>();
        if (renderer != null)
        {
            return new Vector3(door.transform.position.x, renderer.bounds.max.y + 0.8f, 0f);
        }

        return door.transform.position + Vector3.up * 1.4f;
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
