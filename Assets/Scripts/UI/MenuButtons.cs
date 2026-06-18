using UnityEngine;

public class MenuButtons : MonoBehaviour
{
    public void PlayGame()
    {
        SaveGameService.StartNewGame();
    }

    public void ContinueGame()
    {
        SaveGameService.ContinueLatestGame();
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
