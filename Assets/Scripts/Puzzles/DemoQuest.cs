using System.Collections.Generic;

public static class DemoQuest
{
    private static bool isQuestStarted;
    private static bool isPuzzleSolved;
    private static bool areCoinsHandedIn;
    private static bool isFinalPathOpen;
    private static readonly HashSet<string> collectedCoins = new HashSet<string>();
    private static readonly HashSet<string> unlockedMemories = new HashSet<string>();

    public static bool IsQuestStarted => isQuestStarted;
    public static bool IsPuzzleSolved => isPuzzleSolved;
    public static bool AreCoinsHandedIn => areCoinsHandedIn;
    public static bool IsFinalPathOpen => isFinalPathOpen;
    public static int CollectedCoinCount => collectedCoins.Count;

    public static void StartQuest()
    {
        isQuestStarted = true;
    }

    public static void MarkPuzzleSolved()
    {
        StartQuest();
        isPuzzleSolved = true;
    }

    public static void HandInCoins()
    {
        areCoinsHandedIn = true;
        isFinalPathOpen = true;
    }

    public static void ResetAll()
    {
        isQuestStarted = false;
        isPuzzleSolved = false;
        areCoinsHandedIn = false;
        isFinalPathOpen = false;
        collectedCoins.Clear();
        unlockedMemories.Clear();
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
        }
    }

    public static bool IsMemoryUnlocked(string memoryKey)
    {
        return !string.IsNullOrWhiteSpace(memoryKey) && unlockedMemories.Contains(memoryKey);
    }

    public static void UnlockMemory(string memoryKey)
    {
        if (!string.IsNullOrWhiteSpace(memoryKey))
        {
            unlockedMemories.Add(memoryKey);
        }
    }
}
