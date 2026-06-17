using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PuzzleAuto : MonoBehaviour
{
    public GameObject tilePrefab;
    public int gridSize = 3;
    public float tileSize = 100f;
    public float spacing = 10f;
    public string memoryReward = "Фрагмент памяти";
    [TextArea] public string completionText = "Фрагмент памяти найден. Демо-версия завершена.";

    private GameObject canvasObj;
    private GameObject completionCanvasObj;
    private readonly List<GameObject> tiles = new List<GameObject>();
    private int[,] board;
    private int emptyX;
    private int emptyY;
    private bool isOpen;
    private bool isWin;
    private int openedFrame = -1;

    void Start()
    {
        if (DemoQuest.IsPuzzleSolved)
        {
            isWin = true;
            CoinRoomController.EnsureRoomUnlocked();
        }
    }

    void Update()
    {
        if (isOpen && Time.frameCount > openedFrame && Input.GetKeyDown(KeyCode.E))
        {
            ClosePuzzle();
        }
    }

    public void OpenPuzzle()
    {
        if (isOpen || isWin) return;

        CloseCompletionPanel();

        canvasObj = new GameObject("PuzzleCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(canvasObj.transform);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.6f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(canvasObj.transform);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        float totalSize = gridSize * (tileSize + spacing) - spacing;
        panelRect.sizeDelta = new Vector2(totalSize, totalSize);

        GridLayoutGroup grid = panel.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(tileSize, tileSize);
        grid.spacing = new Vector2(spacing, spacing);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = gridSize;

        board = new int[gridSize, gridSize];
        int val = 1;
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                board[i, j] = val++;
            }
        }

        board[gridSize - 1, gridSize - 1] = 0;
        emptyX = gridSize - 1;
        emptyY = gridSize - 1;

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                GameObject tile = Instantiate(tilePrefab, panel.transform);
                tile.name = $"tile_{i}_{j}";
                TextMeshProUGUI txt = tile.GetComponentInChildren<TextMeshProUGUI>();
                int value = board[i, j];
                txt.text = value == 0 ? "" : value.ToString();

                if (value == 0)
                {
                    Image img = tile.GetComponent<Image>();
                    if (img != null) img.color = Color.clear;
                }

                int ci = i;
                int cj = j;
                tile.GetComponent<Button>().onClick.AddListener(() => OnTileClick(ci, cj));
                tiles.Add(tile);
            }
        }

        Shuffle();
        isOpen = true;
        openedFrame = Time.frameCount;
        BlockPlayer(true);
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    void OnTileClick(int x, int y)
    {
        if (isWin) return;
        if (Mathf.Abs(x - emptyX) + Mathf.Abs(y - emptyY) != 1) return;

        int temp = board[x, y];
        board[x, y] = board[emptyX, emptyY];
        board[emptyX, emptyY] = temp;
        emptyX = x;
        emptyY = y;
        UpdateUI();
        CheckWin();
    }

    void UpdateUI()
    {
        int idx = 0;
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                GameObject tile = tiles[idx];
                TextMeshProUGUI txt = tile.GetComponentInChildren<TextMeshProUGUI>();
                int value = board[i, j];
                txt.text = value == 0 ? "" : value.ToString();

                Image img = tile.GetComponent<Image>();
                if (img != null)
                {
                    img.color = value == 0 ? Color.clear : Color.white;
                }

                idx++;
            }
        }
    }

    void Shuffle()
    {
        System.Random rand = new System.Random();
        for (int s = 0; s < 200; s++)
        {
            List<(int, int)> neighbors = new List<(int, int)>();
            if (emptyX > 0) neighbors.Add((emptyX - 1, emptyY));
            if (emptyX < gridSize - 1) neighbors.Add((emptyX + 1, emptyY));
            if (emptyY > 0) neighbors.Add((emptyX, emptyY - 1));
            if (emptyY < gridSize - 1) neighbors.Add((emptyX, emptyY + 1));

            (int, int) chosen = neighbors[rand.Next(neighbors.Count)];
            int temp = board[chosen.Item1, chosen.Item2];
            board[chosen.Item1, chosen.Item2] = board[emptyX, emptyY];
            board[emptyX, emptyY] = temp;
            emptyX = chosen.Item1;
            emptyY = chosen.Item2;
        }

        UpdateUI();
    }

    void CheckWin()
    {
        int expected = 1;
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                if (i == gridSize - 1 && j == gridSize - 1) break;
                if (board[i, j] != expected) return;
                expected++;
            }
        }

        isWin = true;
        DemoQuest.MarkPuzzleSolved();
        CoinRoomController.EnsureRoomUnlocked();

        if (!string.IsNullOrWhiteSpace(memoryReward))
        {
            Inventory.AddMemory(memoryReward);
        }

        ClosePuzzle();
        ShowCompletionPanel();
    }

    public void ClosePuzzle()
    {
        if (canvasObj != null)
        {
            Destroy(canvasObj);
        }

        tiles.Clear();
        isOpen = false;
        BlockPlayer(false);
    }

    void BlockPlayer(bool block)
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            player.SetMovementBlocked(block);
        }
    }

    void ShowCompletionPanel()
    {
        completionCanvasObj = new GameObject("PuzzleCompletionCanvas");
        Canvas canvas = completionCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 120;
        completionCanvasObj.AddComponent<CanvasScaler>();
        completionCanvasObj.AddComponent<GraphicRaycaster>();

        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(completionCanvasObj.transform, false);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.75f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(completionCanvasObj.transform, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.08f, 0.08f, 0.1f, 0.95f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(640f, 320f);

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(40, 40, 34, 34);
        layout.spacing = 18f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        CreateLabel(panel.transform, "Проход открыт", 34f, FontStyles.Bold, 56f);
        CreateLabel(panel.transform, "Фрагмент памяти найден. Справа открылся тайник с тремя монетами. Собери их и вернись к незнакомцу.", 22f, FontStyles.Normal, 120f);

        GameObject buttons = new GameObject("Buttons");
        buttons.transform.SetParent(panel.transform, false);
        HorizontalLayoutGroup buttonLayout = buttons.AddComponent<HorizontalLayoutGroup>();
        buttonLayout.spacing = 16f;
        buttonLayout.childAlignment = TextAnchor.MiddleCenter;
        buttonLayout.childControlHeight = false;
        buttonLayout.childControlWidth = false;
        buttonLayout.childForceExpandHeight = false;
        buttonLayout.childForceExpandWidth = false;
        LayoutElement buttonsLayout = buttons.AddComponent<LayoutElement>();
        buttonsLayout.preferredHeight = 58f;

        CreateButton(buttons.transform, "Продолжить", CloseCompletionPanel);
        CreateButton(buttons.transform, "В меню", () =>
        {
            CloseCompletionPanel();
            SceneManager.LoadScene("MainMenu");
        });
    }

    void CloseCompletionPanel()
    {
        if (completionCanvasObj != null)
        {
            Destroy(completionCanvasObj);
            completionCanvasObj = null;
        }
    }

    void CreateLabel(Transform parent, string text, float fontSize, FontStyles fontStyle, float height)
    {
        GameObject label = new GameObject("Label");
        label.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = label.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = fontStyle;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        LayoutElement layout = label.AddComponent<LayoutElement>();
        layout.preferredHeight = height;
    }

    void CreateButton(Transform parent, string text, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(text);
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.22f, 0.22f, 0.26f, 1f);
        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(onClick);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(180f, 52f);

        GameObject label = new GameObject("Text");
        label.transform.SetParent(buttonObject.transform, false);
        TextMeshProUGUI tmp = label.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 22f;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;

        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
    }
}
