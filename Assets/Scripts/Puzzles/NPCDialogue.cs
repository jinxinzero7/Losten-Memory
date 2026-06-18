using UnityEngine;
using TMPro;

public class NPCDialogue : MonoBehaviour
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
    private bool playerNear = false;
    private bool isDialogueActive = false;

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        if (interactionText != null)
            interactionText.SetActive(false);
    }

    void Update()
    {
        // Начать диалог по нажатию E
        if (playerNear && GameInput.InteractPressed && !isDialogueActive)
        {
            StartDialogue();
        }
        // Пролистать диалог по ЛЕВОЙ КНОПКЕ МЫШИ
        else if (isDialogueActive && GameInput.PrimaryClickPressed)
        {
            NextLine();
        }
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

        if (playerNear && interactionText != null)
            interactionText.SetActive(true);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;
            if (!isDialogueActive && interactionText != null)
                interactionText.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            if (interactionText != null)
                interactionText.SetActive(false);

            if (isDialogueActive)
                EndDialogue();
        }
    }
}
