using UnityEngine;

public class PuzzleBox : MonoBehaviour, IWorldInteractable
{
    public PuzzleAuto puzzleController;
    public GameObject hintText;
    public string rewardCoinId = "coin_scene_3";
    public string memoryKey = "box_memory";
    public string memoryTitle = "Воспоминание из коробки";
    [TextArea] public string memoryDescription = "Пример описания фотокарточки из коробки.";
    [TextArea] public string memoryCutsceneText = "Пример внутреннего монолога после найденного воспоминания.";

    private ThoughtPrompt hintPrompt;
    private PlayerInteractionController interactionController;
    private Collider2D interactionCollider;

    public Transform InteractionTransform => transform;
    public int InteractionPriority => 80;
    public bool CanInteract => puzzleController != null
        && !puzzleController.IsOpen()
        && !puzzleController.IsSolved();

    void Start()
    {
        interactionCollider = GetComponent<Collider2D>();
        if (puzzleController == null)
        {
            puzzleController = FindAnyObjectByType<PuzzleAuto>();
        }

        ConfigureHint();
        SetInteractionHighlighted(false);
        EnsureRewardsSpawned();
    }

    void Update()
    {
        EnsureRewardsSpawned();
    }

    public void Interact()
    {
        if (!CanInteract) return;

        puzzleController.OpenPuzzle();
        SetInteractionHighlighted(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        interactionController = other.GetComponent<PlayerInteractionController>();
        interactionController?.Register(this);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        interactionController?.Unregister(this);
        interactionController = null;
    }

    private void OnDisable()
    {
        interactionController?.Unregister(this);
        SetInteractionHighlighted(false);
    }

    public Vector2 GetInteractionPoint(Vector2 playerPosition)
    {
        return interactionCollider != null ? interactionCollider.ClosestPoint(playerPosition) : (Vector2)transform.position;
    }

    public void SetInteractionHighlighted(bool highlighted)
    {
        if (hintPrompt != null)
        {
            if (highlighted) hintPrompt.Show();
            else hintPrompt.Hide();
        }
        else if (hintText != null)
        {
            hintText.SetActive(highlighted);
        }
    }

    void ConfigureHint()
    {
        hintText = ThoughtPrompt.EnsurePrompt(hintText, "PuzzleBoxPrompt", "E - открыть", transform, new Vector3(0f, 1.35f, 0f));
        hintPrompt = ThoughtPrompt.Ensure(hintText);
    }

    void EnsureRewardsSpawned()
    {
        if (puzzleController == null || !puzzleController.IsSolved()) return;

        Vector3 basePosition = transform.position;
        if (!DemoQuest.IsCoinCollected(rewardCoinId))
        {
            CoinRoomController.EnsureCoin(rewardCoinId, basePosition + new Vector3(0.95f, -0.45f, 0f), 0.45f);
        }

        if (!DemoQuest.IsMemoryUnlocked(memoryKey))
        {
            GameObject fragment = GameObject.Find("BoxMemoryFragment");
            if (fragment == null)
            {
                fragment = new GameObject("BoxMemoryFragment");
                fragment.transform.position = basePosition + new Vector3(-0.95f, -0.45f, 0f);
                fragment.transform.localScale = Vector3.one * 0.42f;
            }

            SpriteRenderer renderer = fragment.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = fragment.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = RuntimeSpriteLoader.LoadProjectSprite("Assets/Art/Sprites/places/newSprites/3/photo.PNG", 100f);
            renderer.sortingOrder = 6;

            Collider2D existingCollider = fragment.GetComponent<Collider2D>();
            if (existingCollider == null)
            {
                CircleCollider2D circle = fragment.AddComponent<CircleCollider2D>();
                circle.isTrigger = true;
                circle.radius = 0.8f;
            }
            else
            {
                existingCollider.isTrigger = true;
            }

            MemoryFragmentPickup pickup = fragment.GetComponent<MemoryFragmentPickup>();
            if (pickup == null)
            {
                pickup = fragment.AddComponent<MemoryFragmentPickup>();
            }

            pickup.fallbackMemoryKey = memoryKey;
            pickup.fallbackTitle = memoryTitle;
            pickup.fallbackDescription = memoryDescription;
            pickup.fallbackCutsceneText = memoryCutsceneText;
        }
    }
}
