using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleAuto : MonoBehaviour
{
    public GameObject tilePrefab;
    public int gridSize = 3;
    public float tileSize = 100f;
    public float spacing = 10f;

    private GameObject canvasObj;
    private readonly List<GameObject> tiles = new List<GameObject>();
    private int[,] board;
    private int emptyX;
    private int emptyY;
    private bool isOpen;
    private bool isWin;

    void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.E))
        {
            ClosePuzzle();
        }
    }

    public void OpenPuzzle()
    {
        if (isOpen || isWin) return;

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
        Debug.Log("Puzzle solved");
        ClosePuzzle();
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
}
