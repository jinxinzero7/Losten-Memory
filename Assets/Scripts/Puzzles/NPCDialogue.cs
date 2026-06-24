using UnityEngine;
using TMPro;

public class NPCDialogue : MonoBehaviour, IWorldInteractable
{
    [Header("Настройки диалога")]
    public string npcName = "Незнакомец";
    [TextArea] public string[] dialogueLines;

    [Header("UI")]
    public GameObject dialoguePanel;
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public GameObject interactionText;

    private int currentLine = 0;
    private bool isDialogueActive = false;
    private PlayerInteractionController interactionController;
    private Collider2D interactionCollider;

    public Transform InteractionTransform => transform;
    public int InteractionPriority => 30;
    public bool CanInteract => !isDialogueActive;

    void Start()
    {
        interactionCollider = GetComponent<Collider2D>();
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        if (interactionText != null)
            interactionText.SetActive(false);
    }

    void Update()
    {
        // Пролистать диалог по ЛЕВОЙ КНОПКЕ МЫШИ
        if (isDialogueActive && GameInput.PrimaryClickPressed)
        {
            NextLine();
        }
    }

    public void Interact()
    {
        if (!CanInteract) return;

        StartDialogue();
    }

    void StartDialogue()
    {
        isDialogueActive = true;
        currentLine = 0;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (interactionText != null)
            interactionText.SetActive(false);

        ShowLine();
    }

    void ShowLine()
    {
        if (nameText != null)
            nameText.text = npcName;

        if (dialogueText != null && currentLine < dialogueLines.Length)
            dialogueText.text = dialogueLines[currentLine];
    }

    void NextLine()
    {
        currentLine++;

        if (currentLine < dialogueLines.Length)
        {
            ShowLine();
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        isDialogueActive = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (interactionText != null)
            interactionText.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            interactionController = other.GetComponent<PlayerInteractionController>();
            interactionController?.Register(this);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            interactionController?.Unregister(this);
            interactionController = null;
            if (interactionText != null)
                interactionText.SetActive(false);

            if (isDialogueActive)
                EndDialogue();
        }
    }

    void OnDisable()
    {
        interactionController?.Unregister(this);
        if (interactionText != null)
            interactionText.SetActive(false);
    }

    public Vector2 GetInteractionPoint(Vector2 playerPosition)
    {
        return interactionCollider != null ? interactionCollider.ClosestPoint(playerPosition) : (Vector2)transform.position;
    }

    public void SetInteractionHighlighted(bool highlighted)
    {
        if (interactionText != null)
            interactionText.SetActive(highlighted);
    }
}
