using TMPro;
using UnityEngine;

public class PuzzleBox : MonoBehaviour
{
    public PuzzleAuto puzzleController;
    public GameObject hintText;
    public float interactionRadius = 2.5f;
    public bool playerNear;

    private Transform player;

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
        bool shouldShowHint = canInteract && puzzleController != null && !puzzleController.IsOpen();
        SetHintVisible(shouldShowHint);

        if (canInteract && puzzleController != null && GameInput.InteractPressed && !puzzleController.IsOpen())
        {
            puzzleController.OpenPuzzle();
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
        if (hintText == null) return;

        TMP_Text label = hintText.GetComponent<TMP_Text>();
        if (label != null)
        {
            label.text = "Press E";
            label.fontSize = 24f;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.alignment = TextAlignmentOptions.Center;
        }

        RectTransform rect = hintText.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(260f, 60f);
        }
    }

    void SetHintVisible(bool visible)
    {
        if (hintText != null)
        {
            hintText.SetActive(visible);
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
