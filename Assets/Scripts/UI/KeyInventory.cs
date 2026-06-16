using UnityEngine;
using System.Collections.Generic;

public class KeyInventory : MonoBehaviour
{
    public static KeyInventory Instance;
    private static List<string> collectedKeys = new List<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static bool IsKeyCollected(string keyID)
    {
        return collectedKeys.Contains(keyID);
    }

    public static void MarkKeyCollected(string keyID)
    {
        if (!collectedKeys.Contains(keyID))
        {
            collectedKeys.Add(keyID);
        }
    }

    public static bool HasAnyKey()
    {
        return collectedKeys.Count > 0;
    }
}