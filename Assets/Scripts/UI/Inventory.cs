using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    private List<string> items = new List<string>();
    private List<string> memories = new List<string>();
    private int coins = 0;

    [Header("UI Компоненты (назначаются один раз)")]
    public string inventoryPanelName = "InventoryPanel";
    public string memoriesPanelName = "MemoriesPanel";
    public GameObject itemSlotPrefab;
    public bool warnWhenPanelsMissing = false;

    [Header("Иконки")]
    public Sprite keyIcon;
    public Sprite coinIcon;
    public Sprite memoryIcon;

    private Transform inventoryPanel;
    private Transform memoriesPanel;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

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
        FindUIPanels();
        UpdateInventoryUI();
    }

    void Start()
    {
        FindUIPanels();
        UpdateInventoryUI();
    }

    void FindUIPanels()
    {
        inventoryPanel = FindPanel(inventoryPanelName);
        memoriesPanel = FindPanel(memoriesPanelName);

        if (warnWhenPanelsMissing && inventoryPanel == null && !string.IsNullOrWhiteSpace(inventoryPanelName))
        {
            Debug.LogWarning("Панель не найдена: " + inventoryPanelName);
        }

        if (warnWhenPanelsMissing && memoriesPanel == null && !string.IsNullOrWhiteSpace(memoriesPanelName))
        {
            Debug.LogWarning("Панель не найдена: " + memoriesPanelName);
        }
    }

    Transform FindPanel(string panelName)
    {
        if (string.IsNullOrWhiteSpace(panelName)) return null;

        Canvas[] canvases = FindObjectsByType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            foreach (Transform child in canvas.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == panelName)
                {
                    return child;
                }
            }
        }

        return null;
    }

    // ========== ПРЕДМЕТЫ ==========
    public static void AddItem(string itemName)
    {
        if (Instance == null)
        {
            Debug.LogError("Inventory.Instance = NULL!");
            return;
        }

        if (!Instance.items.Contains(itemName))
        {
            Instance.items.Add(itemName);
            Instance.UpdateInventoryUI();
        }
    }

    public static bool HasItem(string itemName)
    {
        if (Instance == null) return false;
        return Instance.items.Contains(itemName);
    }

    public static void RemoveItem(string itemName)
    {
        if (Instance == null) return;
        Instance.items.Remove(itemName);
        Instance.UpdateInventoryUI();
    }

    // ========== МОНЕТКИ ==========
    public static void AddCoins(int amount)
    {
        if (Instance == null) return;
        Instance.coins += amount;
        Instance.UpdateInventoryUI();
    }

    public static int GetCoins()
    {
        return Instance?.coins ?? 0;
    }

    public static bool SpendCoins(int amount)
    {
        if (Instance == null || amount <= 0) return false;
        if (Instance.coins < amount) return false;

        Instance.coins -= amount;
        Instance.UpdateInventoryUI();
        return true;
    }

    // ========== ВОСПОМИНАНИЯ ==========
    public static void AddMemory(string memoryName)
    {
        if (Instance == null) return;

        if (!Instance.memories.Contains(memoryName))
        {
            Instance.memories.Add(memoryName);
            Instance.UpdateInventoryUI();
        }
    }

    public static bool HasMemory(string memoryName)
    {
        if (Instance == null) return false;
        return Instance.memories.Contains(memoryName);
    }

    // ========== UI ==========
    void UpdateInventoryUI()
    {
        // Обновляем панель предметов
        if (inventoryPanel != null)
        {
            foreach (Transform child in inventoryPanel)
                Destroy(child.gameObject);

            foreach (string item in items)
            {
                if (itemSlotPrefab == null) continue;

                GameObject slot = Instantiate(itemSlotPrefab, inventoryPanel);
                Image icon = slot.GetComponent<Image>();

                if (item == "Key" && keyIcon != null)
                    icon.sprite = keyIcon;
            }

            // Монетки
            if (coins > 0 && itemSlotPrefab != null)
            {
                GameObject coinSlot = Instantiate(itemSlotPrefab, inventoryPanel);
                Image icon = coinSlot.GetComponent<Image>();
                if (coinIcon != null) icon.sprite = coinIcon;

                TMPro.TMP_Text text = coinSlot.GetComponentInChildren<TMPro.TMP_Text>();
                if (text != null) text.text = coins.ToString();
            }
        }

        // Обновляем панель воспоминаний
        if (memoriesPanel != null)
        {
            foreach (Transform child in memoriesPanel)
                Destroy(child.gameObject);

            foreach (string memory in memories)
            {
                if (itemSlotPrefab == null) continue;

                GameObject slot = Instantiate(itemSlotPrefab, memoriesPanel);
                Image icon = slot.GetComponent<Image>();
                if (memoryIcon != null) icon.sprite = memoryIcon;

                TMPro.TMP_Text text = slot.GetComponentInChildren<TMPro.TMP_Text>();
                if (text != null) text.text = memory;
            }
        }
    }
}
