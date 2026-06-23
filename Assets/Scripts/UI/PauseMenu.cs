using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool isPaused = false;
    public GameObject pauseMenuUI;

    private bool styleApplied;

    void Start()
    {
        ApplyPauseMenuStyle();
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
    }

    void Update()
    {
        if (!GameInput.CancelPressed) return;

        if (isPaused)
            Resume();
        else
            Pause();
    }

    public void Resume()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        Time.timeScale = 1f;
        isPaused = false;
    }

    void Pause()
    {
        ApplyPauseMenuStyle();
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        ScreenTransition.LoadSceneWithFade("MainMenu");
    }

    public void SaveGame()
    {
        SaveGameService.SaveNow();
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#endif
    }

    private void ApplyPauseMenuStyle()
    {
        if (styleApplied || pauseMenuUI == null) return;

        Sprite panelSprite = RuntimeSpriteLoader.LoadProjectSprite(
            "Assets/Art/Sprites/interface/pauseMenu.PNG",
            new Rect(499f, 96f, 733f, 803f),
            100f);
        Sprite buttonSprite = RuntimeSpriteLoader.LoadProjectSprite(
            "Assets/Art/Sprites/interface/button.PNG",
            new Rect(672f, 538f, 424f, 117f),
            100f);

        GameObject panelObject = ResolvePausePanelObject();
        Image panelImage = panelObject != null ? panelObject.GetComponent<Image>() : null;
        if (panelImage == null && panelObject != null)
        {
            panelImage = panelObject.AddComponent<Image>();
        }

        if (panelImage != null && panelSprite != null)
        {
            panelImage.sprite = panelSprite;
            panelImage.color = Color.white;
            panelImage.type = Image.Type.Sliced;
        }

        foreach (Button button in pauseMenuUI.GetComponentsInChildren<Button>(true))
        {
            Image image = button.GetComponent<Image>();
            if (image != null && buttonSprite != null)
            {
                image.sprite = buttonSprite;
                image.color = Color.white;
                image.type = Image.Type.Sliced;
            }
        }

        styleApplied = true;
    }

    private GameObject ResolvePausePanelObject()
    {
        Transform menuPanel = pauseMenuUI.transform.Find("MenuPanel");
        if (menuPanel != null) return menuPanel.gameObject;

        Transform parent = pauseMenuUI.transform.parent;
        if (parent != null)
        {
            Transform siblingMenuPanel = parent.Find("MenuPanel");
            if (siblingMenuPanel != null) return siblingMenuPanel.gameObject;
        }

        GameObject sceneMenuPanel = GameObject.Find("MenuPanel");
        if (sceneMenuPanel != null) return sceneMenuPanel;

        return pauseMenuUI;
    }
}
