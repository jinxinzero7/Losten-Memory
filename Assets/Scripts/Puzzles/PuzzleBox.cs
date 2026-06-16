using UnityEngine;

public class PuzzleBox : MonoBehaviour
{
    public PuzzleAuto puzzleController;
    public GameObject hintText;
    public bool playerNear;

    void Start()
    {
        if (puzzleController == null)
        {
            puzzleController = FindAnyObjectByType<PuzzleAuto>();
        }

        SetHintVisible(false);
    }

    void Update()
    {
        if (!playerNear || puzzleController == null) return;

        if (Input.GetKeyDown(KeyCode.E) && !puzzleController.IsOpen())
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
}
