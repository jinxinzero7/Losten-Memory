using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SQLite;
using UnityEngine;

public sealed class SaveDatabase : IDisposable
{
    public const string DatabaseFileName = "losten_memory_saves.db";

    private readonly SQLiteConnection connection;

    public string DatabasePath { get; }

    public SaveDatabase(string databasePath = null)
    {
        DatabasePath = string.IsNullOrWhiteSpace(databasePath)
            ? Path.Combine(Application.persistentDataPath, DatabaseFileName)
            : databasePath;

        string directory = Path.GetDirectoryName(DatabasePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        connection = new SQLiteConnection(DatabasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        InitializeSchema();
    }

    public SaveSlotRecord CreateSlot(string slotName, string sceneName)
    {
        string now = DateTime.UtcNow.ToString("O");
        SaveSlotRecord slot = new SaveSlotRecord
        {
            SlotName = string.IsNullOrWhiteSpace(slotName) ? "Сохранение" : slotName,
            CreatedAt = now,
            UpdatedAt = now,
            CurrentScene = string.IsNullOrWhiteSpace(sceneName) ? "Game" : sceneName,
            PlayerX = 0f,
            PlayerY = 0f,
            PlayTimeSeconds = 0d
        };

        connection.Insert(slot);
        return slot;
    }

    public SaveSlotRecord GetLatestSlot()
    {
        return connection.Table<SaveSlotRecord>()
            .OrderByDescending(slot => slot.UpdatedAt)
            .FirstOrDefault();
    }

    public SaveSlotRecord GetSlot(int slotId)
    {
        return connection.Find<SaveSlotRecord>(slotId);
    }

    public void SaveSnapshot(int slotId, string sceneName, Vector2 playerPosition, double playTimeSeconds, GameSaveSnapshot snapshot)
    {
        SaveSlotRecord slot = GetSlot(slotId);
        if (slot == null) throw new InvalidOperationException($"Save slot {slotId} does not exist.");

        string now = DateTime.UtcNow.ToString("O");
        connection.RunInTransaction(() =>
        {
            slot.UpdatedAt = now;
            slot.CurrentScene = sceneName;
            slot.PlayerX = playerPosition.x;
            slot.PlayerY = playerPosition.y;
            slot.PlayTimeSeconds = playTimeSeconds;
            connection.Update(slot);

            connection.Execute("DELETE FROM QuestStates WHERE save_slot_id = ?", slotId);
            connection.Execute("DELETE FROM InventoryItems WHERE save_slot_id = ?", slotId);
            connection.Execute("DELETE FROM Memories WHERE save_slot_id = ?", slotId);

            InsertQuestState(slotId, "quest_started", snapshot.IsQuestStarted, now);
            InsertQuestState(slotId, "locked_door_tried", snapshot.IsLockedDoorTried, now);
            InsertQuestState(slotId, "puzzle_solved", snapshot.IsPuzzleSolved, now);
            InsertQuestState(slotId, "coins_handed_in", snapshot.AreCoinsHandedIn, now);
            InsertQuestState(slotId, "final_path_open", snapshot.IsFinalPathOpen, now);

            InsertInventoryItem(slotId, "coins", "currency", snapshot.Coins, snapshot.Coins > 0);
            foreach (string item in snapshot.Items.Distinct())
            {
                InsertInventoryItem(slotId, item, "inventory", 1, true);
            }

            foreach (string keyId in snapshot.KeyIds.Distinct())
            {
                InsertInventoryItem(slotId, keyId, "key", 1, true);
            }

            foreach (string coinId in snapshot.CollectedCoinIds.Distinct())
            {
                InsertInventoryItem(slotId, coinId, "quest_coin", 1, true);
            }

            List<string> memoryKeys = snapshot.MemoryKeys.Distinct().ToList();
            for (int i = 0; i < memoryKeys.Count; i++)
            {
                string memoryKey = memoryKeys[i];
                connection.Insert(new MemoryRecord
                {
                    SaveSlotId = slotId,
                    MemoryKey = memoryKey,
                    Title = i < snapshot.MemoryTitles.Count ? snapshot.MemoryTitles[i] : memoryKey,
                    Description = string.Empty,
                    PhotoAssetKey = string.Empty,
                    IsUnlocked = 1
                });
            }
        });
    }

    public GameSaveSnapshot LoadSnapshot(int slotId)
    {
        GameSaveSnapshot snapshot = new GameSaveSnapshot();
        Dictionary<string, int> questStates = connection.Table<QuestStateRecord>()
            .Where(row => row.SaveSlotId == slotId)
            .ToDictionary(row => row.QuestKey, row => row.State);

        snapshot.IsQuestStarted = IsEnabled(questStates, "quest_started");
        snapshot.IsLockedDoorTried = IsEnabled(questStates, "locked_door_tried");
        snapshot.IsPuzzleSolved = IsEnabled(questStates, "puzzle_solved");
        snapshot.AreCoinsHandedIn = IsEnabled(questStates, "coins_handed_in");
        snapshot.IsFinalPathOpen = IsEnabled(questStates, "final_path_open");

        foreach (InventoryItemRecord item in connection.Table<InventoryItemRecord>().Where(row => row.SaveSlotId == slotId))
        {
            switch (item.ItemType)
            {
                case "currency":
                    snapshot.Coins = item.Amount;
                    break;
                case "inventory":
                    snapshot.Items.Add(item.ItemKey);
                    break;
                case "key":
                    snapshot.KeyIds.Add(item.ItemKey);
                    break;
                case "quest_coin":
                    snapshot.CollectedCoinIds.Add(item.ItemKey);
                    break;
            }
        }

        foreach (MemoryRecord memory in connection.Table<MemoryRecord>().Where(row => row.SaveSlotId == slotId && row.IsUnlocked == 1))
        {
            snapshot.MemoryKeys.Add(memory.MemoryKey);
            snapshot.MemoryTitles.Add(memory.Title);
        }

        return snapshot;
    }

    public Dictionary<string, int> GetTableRowCounts()
    {
        return new Dictionary<string, int>
        {
            ["SaveSlots"] = connection.Table<SaveSlotRecord>().Count(),
            ["QuestStates"] = connection.Table<QuestStateRecord>().Count(),
            ["InventoryItems"] = connection.Table<InventoryItemRecord>().Count(),
            ["Memories"] = connection.Table<MemoryRecord>().Count()
        };
    }

    public void Dispose()
    {
        connection?.Close();
    }

    private void InitializeSchema()
    {
        connection.CreateTable<SaveSlotRecord>();
        connection.CreateTable<QuestStateRecord>();
        connection.CreateTable<InventoryItemRecord>();
        connection.CreateTable<MemoryRecord>();
        connection.Execute("PRAGMA foreign_keys = ON");
        connection.Execute("CREATE INDEX IF NOT EXISTS idx_quest_slot ON QuestStates(save_slot_id)");
        connection.Execute("CREATE INDEX IF NOT EXISTS idx_inventory_slot ON InventoryItems(save_slot_id)");
        connection.Execute("CREATE INDEX IF NOT EXISTS idx_memory_slot ON Memories(save_slot_id)");
    }

    private void InsertQuestState(int slotId, string key, bool state, string updatedAt)
    {
        connection.Insert(new QuestStateRecord
        {
            SaveSlotId = slotId,
            QuestKey = key,
            State = state ? 1 : 0,
            UpdatedAt = updatedAt
        });
    }

    private void InsertInventoryItem(int slotId, string key, string type, int amount, bool collected)
    {
        connection.Insert(new InventoryItemRecord
        {
            SaveSlotId = slotId,
            ItemKey = key,
            ItemType = type,
            Amount = amount,
            IsCollected = collected ? 1 : 0
        });
    }

    private static bool IsEnabled(Dictionary<string, int> states, string key)
    {
        return states.TryGetValue(key, out int value) && value != 0;
    }
}
