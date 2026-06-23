using System.Collections.Generic;

public static class DemoQuest
{
    private static bool isQuestStarted;
    private static bool lockedDoorTried;
    private static bool isPuzzleSolved;
    private static bool areCoinsHandedIn;
    private static bool isFinalPathOpen;
    private static readonly HashSet<string> collectedCoins = new HashSet<string>();
    private static readonly HashSet<string> unlockedMemories = new HashSet<string>();
    private static readonly Dictionary<string, string> memoryTitles = new Dictionary<string, string>();

    public static bool IsQuestStarted => isQuestStarted;
    public static bool IsLockedDoorTried => lockedDoorTried;
    public static bool IsPuzzleSolved => isPuzzleSolved;
    public static bool AreCoinsHandedIn => areCoinsHandedIn;
    public static bool IsFinalPathOpen => isFinalPathOpen;
    public static int CollectedCoinCount => collectedCoins.Count;

    public static void MarkLockedDoorTried()
    {
        lockedDoorTried = true;
        SaveGameService.RequestAutosave();
    }

    public static void StartQuest()
    {
        isQuestStarted = true;
        lockedDoorTried = true;
        SaveGameService.RequestAutosave();
    }

    public static void MarkPuzzleSolved()
    {
        StartQuest();
        isPuzzleSolved = true;
        SaveGameService.RequestAutosave();
    }

    public static void HandInCoins()
    {
        areCoinsHandedIn = true;
        isFinalPathOpen = true;
        SaveGameService.RequestAutosave();
    }

    public static void ResetAll()
    {
        isQuestStarted = false;
        lockedDoorTried = false;
        isPuzzleSolved = false;
        areCoinsHandedIn = false;
        isFinalPathOpen = false;
        collectedCoins.Clear();
        unlockedMemories.Clear();
        memoryTitles.Clear();
    }

    public static bool IsCoinCollected(string coinId)
    {
        return !string.IsNullOrWhiteSpace(coinId) && collectedCoins.Contains(coinId);
    }

    public static void MarkCoinCollected(string coinId)
    {
        if (!string.IsNullOrWhiteSpace(coinId))
        {
            collectedCoins.Add(coinId);
            SaveGameService.RequestAutosave();
        }
    }

    public static bool IsMemoryUnlocked(string memoryKey)
    {
        return !string.IsNullOrWhiteSpace(memoryKey) && unlockedMemories.Contains(memoryKey);
    }

    public static void UnlockMemory(string memoryKey, string title = null)
    {
        if (!string.IsNullOrWhiteSpace(memoryKey))
        {
            unlockedMemories.Add(memoryKey);
            memoryTitles[memoryKey] = string.IsNullOrWhiteSpace(title) ? memoryKey : title;
            SaveGameService.RequestAutosave();
        }
    }

    public static List<string> GetCollectedCoinIds()
    {
        return new List<string>(collectedCoins);
    }

    public static List<string> GetUnlockedMemoryKeys()
    {
        List<string> keys = new List<string>(unlockedMemories);
        keys.Sort();
        return keys;
    }

    public static List<string> GetUnlockedMemoryTitles()
    {
        List<string> titles = new List<string>();
        foreach (string key in GetUnlockedMemoryKeys())
        {
            titles.Add(memoryTitles.TryGetValue(key, out string title) ? title : key);
        }

        return titles;
    }

    public static void Restore(
        bool questStarted,
        bool restoredLockedDoorTried,
        bool puzzleSolved,
        bool coinsHandedIn,
        bool finalPathOpen,
        IEnumerable<string> coinIds,
        IEnumerable<string> memoryKeysToRestore,
        IEnumerable<string> memoryTitlesToRestore)
    {
        isQuestStarted = questStarted;
        lockedDoorTried = restoredLockedDoorTried || questStarted;
        isPuzzleSolved = puzzleSolved;
        areCoinsHandedIn = coinsHandedIn;
        isFinalPathOpen = finalPathOpen;

        collectedCoins.Clear();
        if (coinIds != null) collectedCoins.UnionWith(coinIds);

        unlockedMemories.Clear();
        memoryTitles.Clear();

        List<string> keys = memoryKeysToRestore == null
            ? new List<string>()
            : new List<string>(memoryKeysToRestore);
        List<string> titles = memoryTitlesToRestore == null
            ? new List<string>()
            : new List<string>(memoryTitlesToRestore);

        for (int i = 0; i < keys.Count; i++)
        {
            unlockedMemories.Add(keys[i]);
            memoryTitles[keys[i]] = i < titles.Count ? titles[i] : keys[i];
        }
    }
}
