using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class SaveMenuPresenter
{
    public static void Setup(string sceneName)
    {
        if (sceneName != "MainMenu")
        {
            SetupPauseMenu();
            return;
        }

        Button startButton = GameObject.Find("StartButton")?.GetComponent<Button>();
        Button quitButton = GameObject.Find("QuitButton")?.GetComponent<Button>();
        if (startButton == null || GameObject.Find("ContinueButton") != null) return;

        SetButtonText(startButton, "Новая игра");

        GameObject continueObject = Object.Instantiate(startButton.gameObject, startButton.transform.parent);
        continueObject.name = "ContinueButton";
        Button continueButton = continueObject.GetComponent<Button>();
        continueButton.onClick = new Button.ButtonClickedEvent();
        continueButton.onClick.AddListener(SaveGameService.ContinueLatestGame);
        continueButton.interactable = SaveGameService.HasSave;
        SetButtonText(continueButton, "Продолжить");

        RectTransform startRect = startButton.GetComponent<RectTransform>();
        RectTransform continueRect = continueButton.GetComponent<RectTransform>();
        continueRect.anchoredPosition = startRect.anchoredPosition + new Vector2(0f, -105f);

        if (quitButton != null)
        {
            RectTransform quitRect = quitButton.GetComponent<RectTransform>();
            quitRect.anchoredPosition += new Vector2(0f, -105f);
        }
    }

    private static void SetupPauseMenu()
    {
        Button resumeButton = FindButton("ResumeButton");
        Button menuButton = FindButton("MenuButton");
        Button quitButton = FindButton("QuitButton");
        PauseMenu pauseMenu = Object.FindAnyObjectByType<PauseMenu>(FindObjectsInactive.Include);
        if (resumeButton == null || menuButton == null || quitButton == null || pauseMenu == null) return;
        if (FindButton("SaveButton") != null) return;

        GameObject saveObject = Object.Instantiate(resumeButton.gameObject, resumeButton.transform.parent);
        saveObject.name = "SaveButton";
        Button saveButton = saveObject.GetComponent<Button>();
        saveButton.onClick = new Button.ButtonClickedEvent();
        saveButton.onClick.AddListener(pauseMenu.SaveGame);
        SetButtonText(saveButton, "Сохранить");

        SetButtonY(resumeButton, 75f);
        SetButtonY(saveButton, 25f);
        SetButtonY(menuButton, -25f);
        SetButtonY(quitButton, -75f);
    }

    private static void SetButtonY(Button button, float y)
    {
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y);
    }

    private static Button FindButton(string objectName)
    {
        foreach (Button button in Object.FindObjectsByType<Button>(FindObjectsInactive.Include))
        {
            if (button.name == objectName) return button;
        }

        return null;
    }

    private static void SetButtonText(Button button, string text)
    {
        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.text = text;
        }
    }
}
