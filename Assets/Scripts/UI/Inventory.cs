using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

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
    public Sprite inventoryCellSprite;
    public Sprite dogFoodIcon;

    private Transform inventoryPanel;
    private Transform memoriesPanel;
    private const float SlotSize = 120f;
    private const float IconSize = 58f;

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
            Instance.CopyConfigurationFrom(this);
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
        if (inventoryPanel == null)
        {
            inventoryPanel = CreateRuntimePanel(inventoryPanelName, new Vector2(24f, -24f));
        }

        if (memoriesPanel == null)
        {
            memoriesPanel = CreateRuntimePanel(memoriesPanelName, new Vector2(24f, -164f));
        }

        ConfigurePanelLayout(inventoryPanel);
        ConfigurePanelLayout(memoriesPanel);

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

    Transform CreateRuntimePanel(string panelName, Vector2 anchoredPosition)
    {
        Canvas canvas = null;
        foreach (Canvas candidate in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude))
        {
            if (candidate.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                canvas = candidate;
                break;
            }
        }

        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("RuntimeInventoryCanvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        GameObject panelObject = new GameObject(string.IsNullOrWhiteSpace(panelName) ? "InventoryPanel" : panelName);
        panelObject.transform.SetParent(canvas.transform, false);

        RectTransform rect = panelObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(SlotSize * 3f + 24f, SlotSize);

        GridLayoutGroup grid = panelObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(SlotSize, SlotSize);
        grid.spacing = new Vector2(12f, 12f);
        grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        grid.constraintCount = 1;

        Image image = panelObject.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);
        return panelObject.transform;
    }

    // ========== ПРЕДМЕТЫ ==========
    public static void AddItem(string itemName)
    {
        EnsureInstance();
        if (Instance == null)
        {
            Debug.LogError("Inventory.Instance = NULL!");
            return;
        }

        if (!Instance.items.Contains(itemName))
        {
            Instance.items.Add(itemName);
            Instance.UpdateInventoryUI();
            SaveGameService.RequestAutosave();
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
        SaveGameService.RequestAutosave();
    }

    // ========== МОНЕТКИ ==========
    public static void AddCoins(int amount)
    {
        EnsureInstance();
        if (Instance == null) return;
        Instance.coins += amount;
        Instance.UpdateInventoryUI();
        SaveGameService.RequestAutosave();
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
        SaveGameService.RequestAutosave();
        return true;
    }

    // ========== ВОСПОМИНАНИЯ ==========
    public static void AddMemory(string memoryName)
    {
        EnsureInstance();
        if (Instance == null) return;

        if (!Instance.memories.Contains(memoryName))
        {
            Instance.memories.Add(memoryName);
            Instance.UpdateInventoryUI();
            SaveGameService.RequestAutosave();
        }
    }

    public static bool HasMemory(string memoryName)
    {
        if (Instance == null) return false;
        return Instance.memories.Contains(memoryName);
    }

    public static void ResetAll()
    {
        if (Instance == null) return;

        Instance.items.Clear();
        Instance.memories.Clear();
        Instance.coins = 0;
        Instance.UpdateInventoryUI();
    }

    public static List<string> GetItems()
    {
        return Instance == null ? new List<string>() : new List<string>(Instance.items);
    }

    public static List<string> GetMemories()
    {
        return Instance == null ? new List<string>() : new List<string>(Instance.memories);
    }

    public static void Restore(IEnumerable<string> restoredItems, IEnumerable<string> restoredMemories, int restoredCoins)
    {
        EnsureInstance();

        Instance.items.Clear();
        if (restoredItems != null) Instance.items.AddRange(restoredItems);

        Instance.memories.Clear();
        if (restoredMemories != null) Instance.memories.AddRange(restoredMemories);

        Instance.coins = Mathf.Max(0, restoredCoins);
        Instance.FindUIPanels();
        Instance.UpdateInventoryUI();
    }

    public static void EnsureInstance()
    {
        if (Instance != null) return;

        GameObject inventoryObject = new GameObject("RuntimeInventory");
        inventoryObject.AddComponent<Inventory>();
    }

    private void CopyConfigurationFrom(Inventory source)
    {
        if (source == null) return;

        inventoryPanelName = source.inventoryPanelName;
        memoriesPanelName = source.memoriesPanelName;
        itemSlotPrefab = source.itemSlotPrefab;
        warnWhenPanelsMissing = source.warnWhenPanelsMissing;
        keyIcon = source.keyIcon;
        coinIcon = source.coinIcon;
        memoryIcon = source.memoryIcon;
        inventoryCellSprite = source.inventoryCellSprite;
        dogFoodIcon = source.dogFoodIcon;
        FindUIPanels();
        UpdateInventoryUI();
    }

    // ========== UI ==========
    void UpdateInventoryUI()
    {
        if (inventoryPanel != null)
        {
            foreach (Transform child in inventoryPanel)
                Destroy(child.gameObject);

            foreach (string item in items)
            {
                Sprite iconSprite = GetItemIcon(item);
                CreateSlot(inventoryPanel, iconSprite, string.Empty);
            }

            if (coins > 0)
            {
                CreateSlot(inventoryPanel, GetCoinIcon(), coins.ToString());
            }
        }

        if (memoriesPanel != null)
        {
            foreach (Transform child in memoriesPanel)
                Destroy(child.gameObject);

            foreach (string memory in memories)
            {
                string memoryTitle = memory;
                Sprite memorySprite = MemoryArchive.GetPhotoByTitle(memoryTitle) ?? GetMemoryIcon();
                CreateSlot(memoriesPanel, memorySprite, string.Empty, () => MemoryArchive.ShowByTitle(memoryTitle));
            }
        }
    }

    GameObject CreateSlot(Transform parent, Sprite iconSprite, string labelText)
    {
        return CreateSlot(parent, iconSprite, labelText, null);
    }

    GameObject CreateSlot(Transform parent, Sprite iconSprite, string labelText, UnityEngine.Events.UnityAction onClick)
    {
        GameObject slot = itemSlotPrefab != null
            ? Instantiate(itemSlotPrefab, parent)
            : CreateFallbackSlot(parent);

        ApplySlotBackground(slot);

        Image icon = GetOrCreateIcon(slot);
        if (icon != null)
        {
            icon.sprite = iconSprite;
            icon.color = iconSprite == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
            icon.preserveAspect = true;
        }

        ConfigureStackLabel(slot, labelText);
        ConfigureClick(slot, onClick);
        return slot;
    }

    GameObject CreateFallbackSlot(Transform parent)
    {
        GameObject slot = new GameObject("ItemSlot");
        slot.transform.SetParent(parent, false);
        RectTransform rect = slot.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(SlotSize, SlotSize);
        slot.AddComponent<Image>();
        return slot;
    }

    void ApplySlotBackground(GameObject slot)
    {
        Image background = slot.GetComponent<Image>();
        if (background == null)
        {
            background = slot.AddComponent<Image>();
        }

        background.sprite = GetInventoryCellSprite();
        background.color = Color.white;
        background.type = background.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
    }

    Image GetOrCreateIcon(GameObject slot)
    {
        Transform iconTransform = slot.transform.Find("Image");
        if (iconTransform == null)
        {
            GameObject iconObject = new GameObject("Image");
            iconObject.transform.SetParent(slot.transform, false);
            iconTransform = iconObject.transform;

            RectTransform rect = iconObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(IconSize, IconSize);
            iconObject.AddComponent<Image>();
        }
        else
        {
            RectTransform rect = iconTransform.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(IconSize, IconSize);
            }
        }

        return iconTransform.GetComponent<Image>();
    }

    void ConfigureStackLabel(GameObject slot, string labelText)
    {
        TMP_Text label = slot.GetComponentInChildren<TMP_Text>(true);
        if (label == null)
        {
            GameObject labelObject = new GameObject("StackText");
            labelObject.transform.SetParent(slot.transform, false);
            label = labelObject.AddComponent<TextMeshProUGUI>();

            RectTransform rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-12f, 10f);
            rect.sizeDelta = new Vector2(82f, 34f);
        }

        label.text = labelText;
        label.fontSize = string.IsNullOrWhiteSpace(labelText) || labelText.Length <= 2 ? 28f : 18f;
        label.color = Color.black;
        label.alignment = TextAlignmentOptions.BottomRight;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.gameObject.SetActive(!string.IsNullOrWhiteSpace(labelText));
    }

    void ConfigureClick(GameObject slot, UnityEngine.Events.UnityAction onClick)
    {
        Button button = slot.GetComponent<Button>();
        if (onClick == null)
        {
            if (button != null)
            {
                Destroy(button);
            }
            return;
        }

        if (button == null)
        {
            button = slot.AddComponent<Button>();
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);
    }

    void ConfigurePanelLayout(Transform panel)
    {
        if (panel == null) return;

        GridLayoutGroup grid = panel.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            grid.cellSize = new Vector2(SlotSize, SlotSize);
            grid.spacing = new Vector2(12f, 12f);
        }

        RectTransform rect = panel.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(Mathf.Max(rect.sizeDelta.x, SlotSize * 3f + 24f), Mathf.Max(rect.sizeDelta.y, SlotSize));
        }
    }

    Sprite GetInventoryCellSprite()
    {
        if (inventoryCellSprite != null) return inventoryCellSprite;

        inventoryCellSprite = RuntimeSpriteLoader.LoadProjectSprite(
            "Assets/Art/Sprites/interface/inventoryCell.PNG",
            new Rect(18f, 746f, 148f, 166f),
            100f);
        return inventoryCellSprite;
    }

    Sprite GetKeyIcon()
    {
        if (keyIcon != null) return keyIcon;

        keyIcon = RuntimeSpriteLoader.LoadProjectSprite(
            "Assets/Art/Sprites/interface/Key.PNG",
            new Rect(880f, 355f, 98f, 60f),
            100f);
        return keyIcon;
    }

    Sprite GetMemoryIcon()
    {
        if (memoryIcon != null) return memoryIcon;

        memoryIcon = RuntimeSpriteLoader.LoadProjectSprite("Assets/Art/Sprites/interface/photo.PNG", 100f)
            ?? RuntimeSpriteLoader.LoadProjectSprite("Assets/Art/Sprites/places/newSprites/3/photo.PNG", 100f);
        return memoryIcon;
    }

    Sprite GetItemIcon(string item)
    {
        if (item == "Key") return GetKeyIcon();
        if (item == "DogFood") return GetDogFoodIcon();

        return null;
    }

    Sprite GetDogFoodIcon()
    {
        if (dogFoodIcon != null) return dogFoodIcon;

        dogFoodIcon = RuntimeSpriteLoader.LoadProjectSprite("Assets/Art/Sprites/places/newSprites/3/feed.png", 100f)
            ?? RuntimeSpriteLoader.LoadProjectSprite("Assets/Art/Sprites/Maze/item_dog_food_bag.png", 100f);
        return dogFoodIcon;
    }

    Sprite GetCoinIcon()
    {
        if (coinIcon != null) return coinIcon;

        coinIcon = RuntimeSpriteLoader.LoadProjectSprite(
            "Assets/Art/Sprites/coin_norm.png",
            new Rect(13f, 17f, 47f, 35f),
            48f);
        return coinIcon;
    }
}
