using UnityEngine;
using UnityEngine.SceneManagement;

public class CoinRoomController : MonoBehaviour
{
    private bool isUnlocked;

    public static void EnsureSceneCoin()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        switch (sceneName)
        {
            case "Game":
                CreateCoin("coin_game", new Vector2(-3.5f, -3.5f));
                break;
            case "GameScene2":
                CreateCoin("coin_scene_2", new Vector2(4.5f, -3.2f));
                break;
            case "GameScene3" when DemoQuest.IsPuzzleSolved:
                EnsureRoomUnlocked();
                break;
        }
    }

    public static void EnsureRoomUnlocked()
    {
        CoinRoomController controller = FindAnyObjectByType<CoinRoomController>();
        if (controller == null)
        {
            GameObject controllerObject = new GameObject("CoinRoomController");
            controller = controllerObject.AddComponent<CoinRoomController>();
        }

        controller.UnlockRoom();
    }

    void Start()
    {
        if (DemoQuest.IsPuzzleSolved)
        {
            UnlockRoom();
        }
    }

    public void UnlockRoom()
    {
        if (isUnlocked) return;

        isUnlocked = true;
        OpenRightPassage();
        ExtendPlayerBounds();
        SpawnCoins();
    }

    private void OpenRightPassage()
    {
        GameObject rightWall = GameObject.Find("RightWall");
        if (rightWall != null)
        {
            rightWall.SetActive(false);
        }
    }

    private void ExtendPlayerBounds()
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player == null) return;

        player.maxX = Mathf.Max(player.maxX, 12f);
        player.minY = Mathf.Min(player.minY, -3.2f);
        player.maxY = Mathf.Max(player.maxY, -1.5f);
    }

    private void SpawnCoins()
    {
        CreateCoin("coin_scene_3", new Vector2(9f, -2.45f));
    }

    private static void CreateCoin(string coinId, Vector2 position)
    {
        if (DemoQuest.IsCoinCollected(coinId)) return;
        if (GameObject.Find("QuestCoin_" + coinId) != null) return;

        GameObject coin = new GameObject("QuestCoin_" + coinId);
        coin.transform.position = position;
        coin.transform.localScale = Vector3.one * 0.45f;

        SpriteRenderer renderer = coin.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateCoinSprite();
        renderer.sortingOrder = 5;

        CircleCollider2D collider = coin.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.8f;

        CoinPickup pickup = coin.AddComponent<CoinPickup>();
        pickup.coinValue = 1;
        pickup.coinID = coinId;
    }

    private static Sprite CreateCoinSprite()
    {
        Texture2D texture = new Texture2D(32, 32);
        Color clear = new Color(0, 0, 0, 0);
        Color fill = new Color(1f, 0.78f, 0.12f, 1f);
        Color edge = new Color(0.95f, 0.48f, 0.06f, 1f);
        Vector2 center = new Vector2(15.5f, 15.5f);

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance > 15f)
                {
                    texture.SetPixel(x, y, clear);
                }
                else
                {
                    texture.SetPixel(x, y, distance > 12f ? edge : fill);
                }
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 32f);
    }
}
