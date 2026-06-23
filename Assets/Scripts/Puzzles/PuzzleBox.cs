using TMPro;
using UnityEngine;

public class PuzzleBox : MonoBehaviour
{
    public PuzzleAuto puzzleController;
    public GameObject hintText;
    public float interactionRadius = 2.5f;
    public bool playerNear;
    public string rewardCoinId = "coin_scene_3";
    public string memoryKey = "box_memory";
    public string memoryTitle = "Воспоминание из коробки";
    [TextArea] public string memoryDescription = "Пример описания фотокарточки из коробки.";
    [TextArea] public string memoryCutsceneText = "Пример внутреннего монолога после найденного воспоминания.";

    private Transform player;
    private ThoughtPrompt hintPrompt;

    void Start()
    {
        if (puzzleController == null)
        {
            puzzleController = FindAnyObjectByType<PuzzleAuto>();
        }

        CachePlayer();
        ConfigureHint();
        SetHintVisible(false);
    }

    void Update()
    {
        bool canInteract = playerNear || IsPlayerInRange();
        bool canTakeReward = CanTakeReward();
        bool shouldShowHint = canInteract && puzzleController != null && !puzzleController.IsOpen()
            && (!puzzleController.IsSolved() || canTakeReward);
        SetHintVisible(shouldShowHint);

        if (canInteract && puzzleController != null && GameInput.InteractPressed && !puzzleController.IsOpen())
        {
            if (canTakeReward)
            {
                TakeReward();
            }
            else if (!puzzleController.IsSolved())
            {
                puzzleController.OpenPuzzle();
            }

            SetHintVisible(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerNear = true;
        player = other.transform;
        SetHintVisible(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerNear = false;
        SetHintVisible(false);
    }

    void ConfigureHint()
    {
        hintText = ThoughtPrompt.EnsurePrompt(hintText, "PuzzleBoxPrompt", "E - открыть", transform, new Vector3(0f, 1.35f, 0f));
        hintPrompt = ThoughtPrompt.Ensure(hintText);
    }

    void SetHintVisible(bool visible)
    {
        if (hintPrompt != null)
        {
            if (visible)
            {
                UpdateHintText();
                hintPrompt.Show();
            }
            else
            {
                hintPrompt.Hide();
            }
        }
        else if (hintText != null)
        {
            hintText.SetActive(visible);
        }
    }

    void UpdateHintText()
    {
        if (hintText == null || puzzleController == null) return;

        string text = puzzleController.IsSolved() && CanTakeReward()
            ? "E - забрать"
            : "E - открыть";
        ThoughtPrompt.ConfigureLabel(hintText, text, 320f);
    }

    bool CanTakeReward()
    {
        return puzzleController != null
            && puzzleController.IsSolved()
            && (!DemoQuest.IsCoinCollected(rewardCoinId) || !DemoQuest.IsMemoryUnlocked(memoryKey));
    }

    void TakeReward()
    {
        if (!DemoQuest.IsCoinCollected(rewardCoinId))
        {
            Inventory.AddCoins(1);
            DemoQuest.MarkCoinCollected(rewardCoinId);
        }

        if (!DemoQuest.IsMemoryUnlocked(memoryKey))
        {
            DemoQuest.UnlockMemory(memoryKey, memoryTitle);
            Inventory.AddMemory(memoryTitle);
            MemoryPresentation.Show(memoryTitle, memoryDescription, memoryCutsceneText, null);
        }
    }

    bool IsPlayerInRange()
    {
        if (player == null && !CachePlayer()) return false;

        return Vector2.Distance(transform.position, player.position) <= interactionRadius;
    }

    bool CachePlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null) return false;

        player = playerObject.transform;
        return true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
