using UnityEngine;
using TMPro;

public class PuzzleBox : MonoBehaviour
{
    public PuzzleAuto puzzleController;
    public GameObject hintText;
    public float interactionRadius = 2.5f;
    public bool playerNear;

    private Transform player;
    private TMP_Text hintLabel;

    void Start()
    {
        if (puzzleController == null)
        {
            puzzleController = FindAnyObjectByType<PuzzleAuto>();
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        if (hintText != null)
        {
            hintLabel = hintText.GetComponent<TMP_Text>();
            if (hintLabel != null)
            {
                hintLabel.text = "Нажми E, чтобы открыть головоломку";
                hintLabel.fontSize = 24f;
            }
        }

        SetHintVisible(false);
    }

    void Update()
    {
        bool canInteract = playerNear || IsPlayerInRange();
        SetHintVisible(canInteract && puzzleController != null && !puzzleController.IsOpen());

        if (canInteract && puzzleController != null && Input.GetKeyDown(KeyCode.E) && !puzzleController.IsOpen())
        {
            puzzleController.OpenPuzzle();
            SetHintVisible(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerNear = true;
        SetHintVisible(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerNear = false;
        SetHintVisible(false);
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
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null) return false;

            player = playerObject.transform;
        }

        return Vector2.Distance(transform.position, player.position) <= interactionRadius;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
