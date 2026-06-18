using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SaveDatabaseSmokeTest
{
    [MenuItem("Losten Memory/Verify Save Database")]
    public static void Run()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), "losten-memory-save-smoke-test.db");
        DeleteDatabaseFiles(databasePath);

        try
        {
            using (SaveDatabase database = new SaveDatabase(databasePath))
            {
                SaveSlotRecord slot = database.CreateSlot("Smoke Test", "GameScene3");
                GameSaveSnapshot source = new GameSaveSnapshot
                {
                    IsQuestStarted = true,
                    IsPuzzleSolved = true,
                    AreCoinsHandedIn = false,
                    IsFinalPathOpen = false,
                    Coins = 3,
                    Items = new List<string> { "Key" },
                    KeyIds = new List<string> { "key_start" },
                    CollectedCoinIds = new List<string> { "coin_game", "coin_scene_2", "coin_scene_3" },
                    MemoryKeys = new List<string> { "memory_01" },
                    MemoryTitles = new List<string> { "Воспоминание 1" }
                };

                database.SaveSnapshot(slot.Id, "GameScene3", new Vector2(2.5f, -3f), 120d, source);
                GameSaveSnapshot loaded = database.LoadSnapshot(slot.Id);
                SaveSlotRecord loadedSlot = database.GetSlot(slot.Id);
                Dictionary<string, int> counts = database.GetTableRowCounts();

                Require(loadedSlot != null && loadedSlot.CurrentScene == "GameScene3", "Save slot was not restored.");
                Require(Mathf.Approximately(loadedSlot.PlayerX, 2.5f), "Player position was not restored.");
                Require(loaded.IsPuzzleSolved && loaded.CollectedCoinIds.Count == 3, "Quest state was not restored.");
                Require(loaded.Coins == 3 && loaded.Items.Contains("Key"), "Inventory was not restored.");
                Require(loaded.MemoryKeys.Contains("memory_01"), "Memory state was not restored.");
                Require(counts["SaveSlots"] == 1, "SaveSlots row count is invalid.");
                Require(counts["QuestStates"] == 4, "QuestStates row count is invalid.");
                Require(counts["InventoryItems"] >= 6, "InventoryItems row count is invalid.");
                Require(counts["Memories"] == 1, "Memories row count is invalid.");

                Debug.Log($"SQLite smoke test passed: {string.Join(", ", counts)}");
            }

            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
            SaveMenuPresenter.Setup("MainMenu");
            Require(GameObject.Find("ContinueButton") != null, "Continue button was not created.");
            Debug.Log("Save menu smoke test passed.");

            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity", OpenSceneMode.Single);
            SaveMenuPresenter.Setup("Game");
            Require(
                UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include).Any(button => button.name == "SaveButton"),
                "Pause menu save button was not created.");
            Debug.Log("Pause menu smoke test passed.");
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void DeleteDatabaseFiles(string path)
    {
        foreach (string suffix in new[] { string.Empty, "-shm", "-wal" })
        {
            string filePath = path + suffix;
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }
}
