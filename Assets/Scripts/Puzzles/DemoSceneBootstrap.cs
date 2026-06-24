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
            ConfigureNpcAnimation();
        }

        if (sceneName == "GameScene4")
        {
            SetupMemoryScene();
        }

        if (sceneName == "GameScene5")
        {
            SetupDogScene();
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

    private static void ConfigureNpcAnimation()
    {
        ChoiceDialogue dialogue = Object.FindAnyObjectByType<ChoiceDialogue>();
        if (dialogue == null) return;

        SpriteRenderer renderer = dialogue.GetComponent<SpriteRenderer>();
        if (renderer == null) return;

        SimpleSpriteAnimation animation = dialogue.GetComponent<SimpleSpriteAnimation>();
        if (animation == null)
        {
            animation = dialogue.gameObject.AddComponent<SimpleSpriteAnimation>();
        }

        animation.frameDuration = 0.32f;
        animation.spritePaths = new[]
        {
            "Assets/Art/Sprites/places/newSprites/2/npc/IMG_2821.PNG",
            "Assets/Art/Sprites/places/newSprites/2/npc/IMG_2822.PNG",
            "Assets/Art/Sprites/places/newSprites/2/npc/IMG_2823.PNG",
            "Assets/Art/Sprites/places/newSprites/2/npc/IMG_2824.PNG",
            "Assets/Art/Sprites/places/newSprites/2/npc/IMG_2825.PNG",
            "Assets/Art/Sprites/places/newSprites/2/npc/IMG_2826.PNG",
            "Assets/Art/Sprites/places/newSprites/2/npc/IMG_2827.PNG"
        };
    }

    private static void SetupMemoryScene()
    {
        DisableIfExists("BoxForPuzzle");
        DisableIfExists("GameObject");
        DisableIfExists("DoorBack");

        ExtendPlayerBounds();
        ConfigureExistingMemoryFragment();
        ConfigureExistingFeedPickup();
        EnsureMazeVignette();
    }

    private static void SetupDogScene()
    {
        DisableIfExists("DialoguePanel");
        DisableIfExists("ChoicesPanel");
        DisableIfExists("InteractionText");

        GameObject dogBox = GameObject.Find("dogBox") ?? GameObject.Find("dogbox");
        if (dogBox != null)
        {
            SpriteRenderer renderer = dogBox.GetComponent<SpriteRenderer>();
            if (renderer != null && renderer.sprite == null)
            {
                renderer.sprite = RuntimeSpriteLoader.LoadProjectSprite("Assets/Art/Sprites/places/newSprites/3/box.PNG", 100f);
            }

            Collider2D collider = dogBox.GetComponent<Collider2D>();
            if (collider == null)
            {
                BoxCollider2D box = dogBox.AddComponent<BoxCollider2D>();
                box.size = new Vector2(1.4f, 1.2f);
                collider = box;
            }
            collider.isTrigger = true;

            if (dogBox.GetComponent<DogBoxInteraction>() == null)
            {
                dogBox.AddComponent<DogBoxInteraction>();
            }
        }

        GameObject portal = GameObject.Find("portal");
        if (portal != null)
        {
            SpriteRenderer renderer = portal.GetComponent<SpriteRenderer>();
            if (renderer != null && renderer.sprite == null)
            {
                renderer.sprite = CreatePortalSprite();
            }

            Collider2D collider = portal.GetComponent<Collider2D>();
            if (collider == null)
            {
                BoxCollider2D box = portal.AddComponent<BoxCollider2D>();
                box.size = new Vector2(1.2f, 1.8f);
                collider = box;
            }
            collider.isTrigger = true;

            if (portal.GetComponent<EndingPortal>() == null)
            {
                portal.AddComponent<EndingPortal>();
            }
        }

        DogSceneController controller = Object.FindAnyObjectByType<DogSceneController>();
        if (controller == null)
        {
            GameObject controllerObject = new GameObject("DogSceneController");
            controller = controllerObject.AddComponent<DogSceneController>();
        }

        controller.portal = portal;
        if (portal != null)
        {
            portal.SetActive(DemoQuest.IsMemoryUnlocked("dog_memory"));
        }
    }

    private static Sprite CreatePortalSprite()
    {
        const int width = 80;
        const int height = 120;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2((width - 1) * 0.5f, (height - 1) * 0.5f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = (x - center.x) / (width * 0.48f);
                float dy = (y - center.y) / (height * 0.48f);
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                if (distance > 1f)
                {
                    texture.SetPixel(x, y, new Color(0f, 0f, 0f, 0f));
                    continue;
                }

                float edge = Mathf.SmoothStep(0.72f, 1f, distance);
                Color color = Color.Lerp(new Color(0.18f, 0.45f, 0.95f, 0.65f), new Color(0.78f, 0.92f, 1f, 1f), edge);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 64f);
    }

    private static void ConfigureExistingMemoryFragment()
    {
        GameObject fragment = GameObject.Find("photo_0");
        if (fragment == null) return;

        SpriteRenderer renderer = fragment.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = fragment.AddComponent<SpriteRenderer>();
        }
        if (renderer.sprite == null)
        {
            renderer.sprite = RuntimeSpriteLoader.LoadProjectSprite("Assets/Art/Sprites/places/newSprites/3/photo.PNG", 100f);
        }
        renderer.sortingOrder = Mathf.Max(renderer.sortingOrder, 6);

        Collider2D collider = fragment.GetComponent<Collider2D>();
        if (collider == null)
        {
            CircleCollider2D circle = fragment.AddComponent<CircleCollider2D>();
            circle.radius = 0.8f;
            collider = circle;
        }
        collider.isTrigger = true;

        MemoryFragmentPickup pickup = fragment.GetComponent<MemoryFragmentPickup>();
        if (pickup == null)
        {
            pickup = fragment.AddComponent<MemoryFragmentPickup>();
        }

        pickup.fallbackMemoryKey = "maze_memory_01";
        pickup.fallbackTitle = "Воспоминание из лабиринта";
        pickup.fallbackDescription = "Пример описания фотокарточки, найденной в лабиринте.";
        pickup.fallbackCutsceneText = "Пример внутреннего монолога после лабиринта.";
    }

    private static void ConfigureExistingFeedPickup()
    {
        GameObject feed = GameObject.Find("feed_0");
        if (feed == null) return;

        Collider2D collider = feed.GetComponent<Collider2D>();
        if (collider == null)
        {
            CircleCollider2D circle = feed.AddComponent<CircleCollider2D>();
            circle.radius = 0.8f;
            collider = circle;
        }
        collider.isTrigger = true;

        InventoryItemPickup pickup = feed.GetComponent<InventoryItemPickup>();
        if (pickup == null)
        {
            pickup = feed.AddComponent<InventoryItemPickup>();
        }

        pickup.itemId = "DogFood";
        pickup.promptText = "E - взять корм";
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

        player.minX = -12f;
        player.maxX = 12f;
        player.minY = -6.8f;
        player.maxY = 5.7f;
    }

    private static void EnsureMazeVignette()
    {
        if (GameObject.Find("MazeVignetteCanvas") != null) return;

        GameObject canvasObject = new GameObject("MazeVignetteCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 80;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject overlayObject = new GameObject("Vignette");
        overlayObject.transform.SetParent(canvasObject.transform, false);
        Image overlay = overlayObject.AddComponent<Image>();
        overlay.sprite = CreateVignetteSprite();
        overlay.color = Color.white;
        overlay.raycastTarget = false;
        Stretch(overlay.rectTransform);
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Sprite CreateVignetteSprite()
    {
        const int size = 256;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center) / (size * 0.5f);
                float alpha = Mathf.SmoothStep(0f, 0.78f, Mathf.InverseLerp(0.38f, 0.78f, distance));
                texture.SetPixel(x, y, new Color(0f, 0f, 0f, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
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
