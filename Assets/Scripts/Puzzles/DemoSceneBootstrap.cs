using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class DemoSceneBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneHandler()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameInput.ConfigureUiInput();
        InitializeCurrentScene();
    }

    public static void InitializeCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        CoinRoomController.EnsureSceneCoin();
        QuestCoinCounter.EnsureCreated();

        if (sceneName == "GameScene2")
        {
            EnsureFinalDoor();
        }

        if (sceneName == "GameScene4")
        {
            SetupMemoryScene();
        }
    }

    public static void EnsureFinalDoor()
    {
        if (!DemoQuest.IsFinalPathOpen) return;

        foreach (DoorTransition transition in Object.FindObjectsByType<DoorTransition>(FindObjectsInactive.Include))
        {
            if (transition.targetScene != "GameScene4") continue;

            transition.requireKey = false;
            transition.requireQuestStarted = true;
            transition.requireFinalPathOpen = true;
            transition.gameObject.SetActive(true);
        }
    }

    private static void SetupMemoryScene()
    {
        DisableIfExists("BoxForPuzzle");
        DisableIfExists("GameObject");
        DisableIfExists("DoorBack");

        ExtendPlayerBounds();
        EnsureMemoryFragment();
    }

    private static void EnsureMemoryFragment()
    {
        if (GameObject.Find("MemoryFragment") != null) return;

        GameObject fragment = new GameObject("MemoryFragment");
        fragment.transform.position = new Vector3(4.8f, -2.35f, 0f);
        fragment.transform.localScale = Vector3.one * 0.75f;

        SpriteRenderer renderer = fragment.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateDiamondSprite();
        renderer.sortingOrder = 6;

        CircleCollider2D collider = fragment.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.8f;

        fragment.AddComponent<MemoryFragmentPickup>();
    }

    private static void DisableIfExists(string objectName)
    {
        GameObject target = GameObject.Find(objectName);
        if (target != null)
        {
            target.SetActive(false);
        }
    }

    private static void ExtendPlayerBounds()
    {
        PlayerController player = Object.FindAnyObjectByType<PlayerController>();
        if (player == null) return;

        player.minX = -11.5f;
        player.maxX = 11.5f;
        player.minY = -3.4f;
        player.maxY = -1.2f;
    }

    private static void CreateWorldLabel(string objectName, string text, Vector3 position)
    {
        if (GameObject.Find(objectName) != null) return;

        GameObject labelObject = new GameObject(objectName);
        labelObject.transform.position = position;

        TextMeshPro label = labelObject.AddComponent<TextMeshPro>();
        label.text = text;
        label.fontSize = 4f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.sortingOrder = 10;
    }

    private static Sprite CreateRectangleSprite(Color fill, Color edge)
    {
        Texture2D texture = new Texture2D(32, 48);

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                bool isEdge = x < 3 || y < 3 || x > texture.width - 4 || y > texture.height - 4;
                texture.SetPixel(x, y, isEdge ? edge : fill);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 32f);
    }

    private static Sprite CreateDiamondSprite()
    {
        Texture2D texture = new Texture2D(40, 40);
        Color clear = new Color(0, 0, 0, 0);
        Color fill = new Color(0.45f, 0.85f, 1f, 1f);
        Color edge = new Color(1f, 1f, 1f, 1f);
        Vector2 center = new Vector2(19.5f, 19.5f);

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float distance = Mathf.Abs(x - center.x) + Mathf.Abs(y - center.y);
                if (distance > 18f)
                {
                    texture.SetPixel(x, y, clear);
                }
                else
                {
                    texture.SetPixel(x, y, distance > 15f ? edge : fill);
                }
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 32f);
    }
}
