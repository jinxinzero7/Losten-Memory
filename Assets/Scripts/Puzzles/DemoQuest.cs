using UnityEngine;

public static class DemoQuest
{
    private const string QuestStartedKey = "DemoQuest.Started";
    private const string PuzzleSolvedKey = "DemoQuest.PuzzleSolved";
    private const string CoinsHandedInKey = "DemoQuest.CoinsHandedIn";
    private const string FinalPathOpenKey = "DemoQuest.FinalPathOpen";
    private const string CoinCollectedPrefix = "DemoQuest.Coin.";

    public static bool IsQuestStarted => PlayerPrefs.GetInt(QuestStartedKey, 0) == 1;
    public static bool IsPuzzleSolved => PlayerPrefs.GetInt(PuzzleSolvedKey, 0) == 1;
    public static bool AreCoinsHandedIn => PlayerPrefs.GetInt(CoinsHandedInKey, 0) == 1;
    public static bool IsFinalPathOpen => PlayerPrefs.GetInt(FinalPathOpenKey, 0) == 1;

    public static void StartQuest()
    {
        SetFlag(QuestStartedKey);
    }

    public static void MarkPuzzleSolved()
    {
        StartQuest();
        SetFlag(PuzzleSolvedKey);
    }

    public static void HandInCoins()
    {
        SetFlag(CoinsHandedInKey);
        SetFlag(FinalPathOpenKey);
    }

    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(QuestStartedKey);
        PlayerPrefs.DeleteKey(PuzzleSolvedKey);
        PlayerPrefs.DeleteKey(CoinsHandedInKey);
        PlayerPrefs.DeleteKey(FinalPathOpenKey);
        PlayerPrefs.DeleteKey(CoinCollectedPrefix + "coin_room_1");
        PlayerPrefs.DeleteKey(CoinCollectedPrefix + "coin_room_2");
        PlayerPrefs.DeleteKey(CoinCollectedPrefix + "coin_room_3");
        PlayerPrefs.Save();
    }

    public static bool IsCoinCollected(string coinId)
    {
        if (string.IsNullOrWhiteSpace(coinId)) return false;
        return PlayerPrefs.GetInt(CoinCollectedPrefix + coinId, 0) == 1;
    }

    public static void MarkCoinCollected(string coinId)
    {
        if (string.IsNullOrWhiteSpace(coinId)) return;
        SetFlag(CoinCollectedPrefix + coinId);
    }

    private static void SetFlag(string key)
    {
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }
}
