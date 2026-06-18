using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveGameService : MonoBehaviour
{
    public static SaveGameService Instance { get; private set; }
    public static bool HasSave => Instance != null && Instance.database.GetLatestSlot() != null;
    public static string DatabasePath => Instance != null ? Instance.database.DatabasePath : string.Empty;
    public static bool IsRestoring => Instance != null && Instance.isRestoring;

    private SaveDatabase database;
    private int activeSlotId;
    private double basePlayTime;
    private float sessionStartTime;
    private bool autosaveQueued;
    private SaveSlotRecord pendingRestore;
    private bool isRestoring;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;

        GameObject serviceObject = new GameObject("SaveGameService");
        serviceObject.AddComponent<SaveGameService>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        database = new SaveDatabase();
        sessionStartTime = Time.realtimeSinceStartup;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update()
    {
        if (GameInput.ManualSavePressed)
        {
            SaveNow();
        }
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        database?.Dispose();
        Instance = null;
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) SaveNow();
    }

    private void OnApplicationQuit()
    {
        SaveNow();
    }

    public static void StartNewGame()
    {
        if (Instance == null) Bootstrap();

        GameManager.ResetFirstLaunch();
        Instance.activeSlotId = Instance.database.CreateSlot("Новая игра", "Game").Id;
        Instance.basePlayTime = 0d;
        Instance.sessionStartTime = Time.realtimeSinceStartup;
        Instance.pendingRestore = null;
        Instance.isRestoring = false;
        SceneManager.LoadScene("Game");
    }

    public static void ContinueLatestGame()
    {
        if (Instance == null) Bootstrap();

        SaveSlotRecord slot = Instance.database.GetLatestSlot();
        if (slot == null) return;

        Instance.activeSlotId = slot.Id;
        Instance.basePlayTime = slot.PlayTimeSeconds;
        Instance.sessionStartTime = Time.realtimeSinceStartup;
        Instance.pendingRestore = slot;
        Instance.isRestoring = true;
        SceneManager.LoadScene(string.IsNullOrWhiteSpace(slot.CurrentScene) ? "Game" : slot.CurrentScene);
    }

    public static void RequestAutosave()
    {
        if (Instance == null || Instance.activeSlotId == 0 || Instance.autosaveQueued) return;

        Instance.autosaveQueued = true;
        Instance.StartCoroutine(Instance.DelayedAutosave());
    }

    public static void SaveNow()
    {
        if (Instance == null || Instance.activeSlotId == 0) return;

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "MainMenu") return;

        PlayerController player = FindAnyObjectByType<PlayerController>();
        Vector2 position = player != null ? player.transform.position : Vector2.zero;
        double playTime = Instance.basePlayTime + (Time.realtimeSinceStartup - Instance.sessionStartTime);
        Instance.database.SaveSnapshot(Instance.activeSlotId, sceneName, position, playTime, CaptureSnapshot());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SaveMenuPresenter.Setup(scene.name);

        if (scene.name == "MainMenu") return;

        if (pendingRestore != null)
        {
            StartCoroutine(RestoreAfterSceneLoad(pendingRestore));
            pendingRestore = null;
            return;
        }

        if (activeSlotId == 0 && scene.name == "Game")
        {
            activeSlotId = database.CreateSlot("Новая игра", scene.name).Id;
            basePlayTime = 0d;
            sessionStartTime = Time.realtimeSinceStartup;
        }

        RequestAutosave();
    }

    private IEnumerator RestoreAfterSceneLoad(SaveSlotRecord slot)
    {
        yield return null;

        GameSaveSnapshot snapshot = database.LoadSnapshot(slot.Id);
        DemoQuest.Restore(
            snapshot.IsQuestStarted,
            snapshot.IsPuzzleSolved,
            snapshot.AreCoinsHandedIn,
            snapshot.IsFinalPathOpen,
            snapshot.CollectedCoinIds,
            snapshot.MemoryKeys,
            snapshot.MemoryTitles);
        KeyInventory.Restore(snapshot.KeyIds);
        Inventory.Restore(snapshot.Items, snapshot.MemoryTitles, snapshot.Coins);

        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            player.transform.position = new Vector3(slot.PlayerX, slot.PlayerY, player.transform.position.z);
        }

        foreach (KeyItem keyItem in FindObjectsByType<KeyItem>())
        {
            if (KeyInventory.IsKeyCollected(keyItem.keyID))
            {
                Destroy(keyItem.gameObject);
            }
        }

        DemoSceneBootstrap.InitializeCurrentScene();
        isRestoring = false;
    }

    private IEnumerator DelayedAutosave()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        autosaveQueued = false;
        SaveNow();
    }

    private static GameSaveSnapshot CaptureSnapshot()
    {
        return new GameSaveSnapshot
        {
            IsQuestStarted = DemoQuest.IsQuestStarted,
            IsPuzzleSolved = DemoQuest.IsPuzzleSolved,
            AreCoinsHandedIn = DemoQuest.AreCoinsHandedIn,
            IsFinalPathOpen = DemoQuest.IsFinalPathOpen,
            Coins = Inventory.GetCoins(),
            Items = Inventory.GetItems(),
            KeyIds = KeyInventory.GetCollectedKeys(),
            CollectedCoinIds = DemoQuest.GetCollectedCoinIds(),
            MemoryKeys = DemoQuest.GetUnlockedMemoryKeys(),
            MemoryTitles = DemoQuest.GetUnlockedMemoryTitles()
        };
    }
}
