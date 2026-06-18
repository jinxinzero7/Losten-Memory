using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceDialogue : MonoBehaviour
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
    private bool playerNear;
    private bool isDialogueActive;
    private PlayerController playerController;

    void Start()
    {
        ConfigureInteractionText();
        SetDialogueVisible(false);
        SetInteractionVisible(false);
        DemoSceneBootstrap.InitializeCurrentScene();
    }

    void Update()
    {
        if (playerNear && !isDialogueActive && GameInput.InteractPressed)
        {
            StartDialogue();
        }

        if (isDialogueActive && GameInput.CancelPressed)
        {
            EndDialogue();
        }
    }

    void StartDialogue()
    {
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

        currentNodeIndex = 0;
        isDialogueActive = true;
        DemoQuest.StartQuest();
        playerController = FindAnyObjectByType<PlayerController>();
        if (playerController != null)
        {
            playerController.SetMovementBlocked(true);
        }

        SetInteractionVisible(false);
        SetDialogueVisible(true);
        ShowNode(currentNodeIndex);
    }

    bool ShouldUseQuestProgressDialogue()
    {
        return DemoQuest.IsPuzzleSolved;
    }

    void StartQuestDialogue()
    {
        isDialogueActive = true;
        playerController = FindAnyObjectByType<PlayerController>();
        if (playerController != null)
        {
            playerController.SetMovementBlocked(true);
        }

        SetInteractionVisible(false);
        SetDialogueVisible(true);
        ShowQuestNode();
    }

    void ShowQuestNode()
    {
        ClearChoices();

        if (nameText != null)
        {
            nameText.text = "Незнакомец";
        }

        if (choicesPanel == null || choiceButtonPrefab == null)
        {
            Debug.LogWarning("ChoiceDialogue UI is not fully assigned.");
            return;
        }

        if (!DemoQuest.IsQuestStarted)
        {
            SetNpcText("Ты ищешь путь дальше? В соседней комнате есть головоломка. Реши ее, и откроется проход к тайнику.");
            AddQuestChoice("Я решу головоломку", () =>
            {
                DemoQuest.StartQuest();
                EndDialogue();
            });
            return;
        }

        if (!DemoQuest.IsPuzzleSolved)
        {
            SetNpcText("Проход к комнате с головоломкой открыт. Вернись ко мне, когда найдешь, что она скрывает.");
            AddQuestChoice("Пойду туда", EndDialogue);
            return;
        }

        int questCoinCount = Mathf.Max(Inventory.GetCoins(), DemoQuest.CollectedCoinCount);
        if (questCoinCount < 3 && !DemoQuest.AreCoinsHandedIn)
        {
            SetNpcText("Монеты разбросаны по комнатам, через которые ты уже прошла. Найди все три и принеси их мне.");
            AddQuestChoice("Соберу монеты", EndDialogue);
            return;
        }

        if (!DemoQuest.AreCoinsHandedIn)
        {
            SetNpcText("Ты принесла три монеты. Я открою тебе путь вперед.");
            AddQuestChoice("Отдать 3 монеты", () =>
            {
                if (Inventory.SpendCoins(3) || DemoQuest.CollectedCoinCount >= 3)
                {
                    DemoQuest.HandInCoins();
                    DemoSceneBootstrap.EnsureFinalDoor();
                }

                ShowQuestNode();
            });
            AddQuestChoice("Пока не отдавать", EndDialogue);
            return;
        }

        SetNpcText("Путь вперед открыт. Иди дальше, пока воспоминание еще держит дверь.");
        AddQuestChoice("Спасибо", EndDialogue);
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
        GameObject buttonObject = Instantiate(choiceButtonPrefab, choicesPanel.transform);
        TMP_Text buttonText = buttonObject.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = text;
        }

        Button button = buttonObject.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(onClick);
        }
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
        if (nameText != null)
        {
            nameText.text = npcName;
        }

        if (npcText != null)
        {
            npcText.text = node.npcText;
        }

        if (choicesPanel == null || choiceButtonPrefab == null)
        {
            Debug.LogWarning("ChoiceDialogue UI is not fully assigned.");
            return;
        }

        foreach (DialogueChoice choice in node.choices)
        {
            GameObject buttonObject = Instantiate(choiceButtonPrefab, choicesPanel.transform);
            TMP_Text buttonText = buttonObject.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = choice.choiceText;
            }

            Button button = buttonObject.GetComponent<Button>();
            if (button != null)
            {
                DialogueChoice capturedChoice = choice;
                button.onClick.AddListener(() => SelectChoice(capturedChoice));
            }
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

    void EndDialogue()
    {
        isDialogueActive = false;
        ClearChoices();
        SetDialogueVisible(false);

        if (playerController != null)
        {
            playerController.SetMovementBlocked(false);
        }

        if (playerNear)
        {
            SetInteractionVisible(true);
        }
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
        if (interactionText != null)
        {
            interactionText.SetActive(visible);
        }
    }

    void ConfigureInteractionText()
    {
        if (interactionText == null) return;

        TMP_Text label = interactionText.GetComponent<TMP_Text>();
        if (label == null)
        {
            label = interactionText.GetComponentInChildren<TMP_Text>(true);
        }

        if (label != null)
        {
            label.text = "E - говорить";
            label.fontSize = 24f;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.alignment = TextAlignmentOptions.Center;
        }

        RectTransform rect = interactionText.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(280f, 60f);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerNear = true;
        if (!isDialogueActive)
        {
            SetInteractionVisible(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerNear = false;
        SetInteractionVisible(false);

        if (isDialogueActive)
        {
            EndDialogue();
        }
    }
}
