using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChoiceDialogue : MonoBehaviour, IWorldInteractable
{
    [Serializable]
    public class DialogueChoice
    {
        public string choiceText;
        public int nextNodeIndex = -1;
        public string itemReward;
        public string memoryReward;
    }

    [Serializable]
    public class DialogueNode
    {
        [TextArea] public string npcText;
        public List<DialogueChoice> choices = new List<DialogueChoice>();
    }

    [Header("Dialogue")]
    public string npcName = "Незнакомец";
    public List<DialogueNode> dialogueNodes = new List<DialogueNode>();

    [Header("UI")]
    public GameObject dialoguePanel;
    public TMP_Text nameText;
    public TMP_Text npcText;
    public GameObject choicesPanel;
    public GameObject choiceButtonPrefab;
    public GameObject interactionText;
    public bool useDemoQuestDialogue = false;

    private int currentNodeIndex;
    private bool isDialogueActive;
    private PlayerController playerController;
    private ThoughtPrompt interactionPrompt;
    private PlayerInteractionController interactionController;
    private Collider2D interactionCollider;

    public Transform InteractionTransform => transform;
    public int InteractionPriority => 30;
    public bool CanInteract => !isDialogueActive;

    void Start()
    {
        interactionCollider = GetComponent<Collider2D>();
        ConfigureInteractionText();
        SetDialogueVisible(false);
        SetInteractionVisible(false);
        DemoSceneBootstrap.InitializeCurrentScene();
    }

    void Update()
    {
        if (isDialogueActive && GameInput.CancelPressed)
        {
            EndDialogue();
        }
    }

    public void Interact()
    {
        if (!CanInteract) return;

        StartDialogue();
    }

    void StartDialogue()
    {
        if (ShouldUseSilentMonsterDialogue())
        {
            StartSilentDialogue();
            return;
        }

        if (useDemoQuestDialogue || ShouldUseQuestProgressDialogue())
        {
            StartQuestDialogue();
            return;
        }

        if (dialogueNodes.Count == 0)
        {
            Debug.LogWarning("ChoiceDialogue has no dialogue nodes.");
            return;
        }

        BeginDialogue();
        ShowNode(0);
    }

    void BeginDialogue()
    {
        currentNodeIndex = 0;
        isDialogueActive = true;
        playerController = FindAnyObjectByType<PlayerController>();
        if (playerController != null)
        {
            playerController.SetMovementBlocked(true);
        }

        SetInteractionVisible(false);
        SetDialogueVisible(true);
    }

    bool ShouldUseSilentMonsterDialogue()
    {
        return SceneManager.GetActiveScene().name == "GameScene2"
            && !DemoQuest.IsLockedDoorTried
            && !DemoQuest.IsQuestStarted
            && !DemoQuest.IsPuzzleSolved;
    }

    bool ShouldUseQuestProgressDialogue()
    {
        return DemoQuest.IsLockedDoorTried || DemoQuest.IsQuestStarted || DemoQuest.IsPuzzleSolved;
    }

    void StartSilentDialogue()
    {
        BeginDialogue();
        ClearChoices();
        SetSpeakerName("Незнакомец");
        SetNpcText("...");
        AddQuestChoice("Уйти", EndDialogue);
    }

    void StartQuestDialogue()
    {
        BeginDialogue();
        ShowQuestNode();
    }

    void ShowQuestNode()
    {
        ClearChoices();
        SetSpeakerName("Незнакомец");

        if (choicesPanel == null || choiceButtonPrefab == null)
        {
            Debug.LogWarning("ChoiceDialogue UI is not fully assigned.");
            return;
        }

        if (!DemoQuest.IsLockedDoorTried)
        {
            SetNpcText("...");
            AddQuestChoice("Уйти", EndDialogue);
            return;
        }

        if (!DemoQuest.IsQuestStarted)
        {
            SetNpcText("Дверь тебя не пустила? Тогда слушай: принеси мне три монеты, и я открою путь дальше. Одна монета рядом, остальные ищи в комнатах, через которые уже проходила.");
            AddQuestChoice("Я найду монеты", () =>
            {
                DemoQuest.StartQuest();
                EndDialogue();
            });
            return;
        }

        if (!DemoQuest.IsPuzzleSolved)
        {
            SetNpcText("Теперь иди в комнату с коробкой. Реши головоломку, потом проверь коробку ещё раз.");
            AddQuestChoice("Пойду туда", EndDialogue);
            return;
        }

        int questCoinCount = Mathf.Max(Inventory.GetCoins(), DemoQuest.CollectedCoinCount);
        if (questCoinCount < 3 && !DemoQuest.AreCoinsHandedIn)
        {
            SetNpcText("Монеты разбросаны по комнатам. Найди все три и возвращайся ко мне.");
            AddQuestChoice("Соберу монеты", EndDialogue);
            return;
        }

        if (!DemoQuest.AreCoinsHandedIn)
        {
            SetNpcText("Ты принесла три монеты. Сделка есть сделка: отдавай их, и проход откроется.");
            AddQuestChoice("Отдать 3 монеты", () =>
            {
                bool hasEnoughCoins = Inventory.GetCoins() >= 3 || DemoQuest.CollectedCoinCount >= 3;
                if (hasEnoughCoins)
                {
                    Inventory.ClearCoins();
                    DemoQuest.HandInCoins();
                    DemoSceneBootstrap.EnsureFinalDoor();
                }

                ShowQuestNode();
            });
            AddQuestChoice("Пока не отдавать", EndDialogue);
            return;
        }

        SetNpcText("Путь открыт. Иди дальше, пока воспоминание ещё держит дверь.");
        AddQuestChoice("Спасибо", EndDialogue);
    }

    void ShowNode(int nodeIndex)
    {
        if (nodeIndex < 0 || nodeIndex >= dialogueNodes.Count)
        {
            EndDialogue();
            return;
        }

        ClearChoices();
        DialogueNode node = dialogueNodes[nodeIndex];
        SetSpeakerName(npcName);
        SetNpcText(node.npcText);

        if (choicesPanel == null || choiceButtonPrefab == null)
        {
            Debug.LogWarning("ChoiceDialogue UI is not fully assigned.");
            return;
        }

        foreach (DialogueChoice choice in node.choices)
        {
            AddChoiceButton(choice.choiceText, () => SelectChoice(choice));
        }
    }

    void SelectChoice(DialogueChoice choice)
    {
        GiveReward(choice);

        if (choice.nextNodeIndex >= 0)
        {
            currentNodeIndex = choice.nextNodeIndex;
            ShowNode(currentNodeIndex);
        }
        else
        {
            EndDialogue();
        }
    }

    void GiveReward(DialogueChoice choice)
    {
        if (!string.IsNullOrWhiteSpace(choice.itemReward))
        {
            Inventory.AddItem(choice.itemReward);
        }

        if (!string.IsNullOrWhiteSpace(choice.memoryReward))
        {
            Inventory.AddMemory(choice.memoryReward);
        }
    }

    void SetSpeakerName(string speakerName)
    {
        if (nameText != null)
        {
            nameText.text = speakerName;
        }
    }

    void SetNpcText(string text)
    {
        if (npcText != null)
        {
            npcText.text = text;
        }
    }

    void AddQuestChoice(string text, UnityEngine.Events.UnityAction onClick)
    {
        AddChoiceButton(text, onClick);
    }

    void AddChoiceButton(string text, UnityEngine.Events.UnityAction onClick)
    {
        if (choicesPanel == null || choiceButtonPrefab == null) return;

        GameObject buttonObject = Instantiate(choiceButtonPrefab, choicesPanel.transform);
        TMP_Text buttonText = buttonObject.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = text;
            buttonText.textWrappingMode = TextWrappingModes.NoWrap;
        }

        Button button = buttonObject.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(onClick);
        }
    }

    void EndDialogue()
    {
        isDialogueActive = false;
        ClearChoices();
        SetDialogueVisible(false);

        if (playerController != null)
        {
            playerController.SetMovementBlocked(false);
        }

        SetInteractionVisible(false);
    }

    void ClearChoices()
    {
        if (choicesPanel == null) return;

        foreach (Transform child in choicesPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    void SetDialogueVisible(bool visible)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(visible);
        }

        if (choicesPanel != null)
        {
            choicesPanel.SetActive(visible);
        }
    }

    void SetInteractionVisible(bool visible)
    {
        if (interactionPrompt != null)
        {
            if (visible) interactionPrompt.Show();
            else interactionPrompt.Hide();
            return;
        }

        if (interactionText != null)
        {
            interactionText.SetActive(visible);
        }
    }

    void ConfigureInteractionText()
    {
        interactionText = ThoughtPrompt.EnsurePrompt(interactionText, "NpcPrompt", "E - говорить", transform, new Vector3(0f, 1.65f, 0f));
        interactionPrompt = ThoughtPrompt.Ensure(interactionText);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        interactionController = other.GetComponent<PlayerInteractionController>();
        if (interactionController != null)
        {
            interactionController.Register(this);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        interactionController?.Unregister(this);
        interactionController = null;
        SetInteractionVisible(false);

        if (isDialogueActive)
        {
            EndDialogue();
        }
    }

    void OnDisable()
    {
        interactionController?.Unregister(this);
        SetInteractionVisible(false);
    }

    public Vector2 GetInteractionPoint(Vector2 playerPosition)
    {
        return interactionCollider != null ? interactionCollider.ClosestPoint(playerPosition) : (Vector2)transform.position;
    }

    public void SetInteractionHighlighted(bool highlighted)
    {
        SetInteractionVisible(highlighted);
    }
}
