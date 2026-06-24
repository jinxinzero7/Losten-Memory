using System.Collections.Generic;
using UnityEngine;

public static class MemoryArchive
{
    private class MemoryEntry
    {
        public string Title;
        public string Description;
        public string CutsceneText;
        public Sprite Photo;
    }

    private static readonly Dictionary<string, MemoryEntry> ByTitle = new Dictionary<string, MemoryEntry>();
    private static readonly Dictionary<string, MemoryEntry> ByKey = new Dictionary<string, MemoryEntry>();

    public static void Register(string key, string title, string description, string cutsceneText, Sprite photo)
    {
        if (string.IsNullOrWhiteSpace(title)) return;

        MemoryEntry entry = new MemoryEntry
        {
            Title = title,
            Description = string.IsNullOrWhiteSpace(description) ? "Описание воспоминания пока не задано." : description,
            CutsceneText = string.IsNullOrWhiteSpace(cutsceneText) ? "Текст воспоминания пока не задан." : cutsceneText,
            Photo = photo
        };

        ByTitle[title] = entry;
        if (!string.IsNullOrWhiteSpace(key))
        {
            ByKey[key] = entry;
        }
    }

    public static void ShowByTitle(string title)
    {
        if (!string.IsNullOrWhiteSpace(title) && ByTitle.TryGetValue(title, out MemoryEntry entry))
        {
            MemoryPresentation.Show(entry.Title, entry.Description, entry.CutsceneText, entry.Photo);
            return;
        }

        MemoryPresentation.Show(title, "Описание воспоминания пока не задано.", "Текст воспоминания пока не задан.", null);
    }

    public static void ShowByKey(string key)
    {
        if (!string.IsNullOrWhiteSpace(key) && ByKey.TryGetValue(key, out MemoryEntry entry))
        {
            MemoryPresentation.Show(entry.Title, entry.Description, entry.CutsceneText, entry.Photo);
        }
    }
}
