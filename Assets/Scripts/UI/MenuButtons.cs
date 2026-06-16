using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    public void PlayGame()
    {
        Debug.Log("===== ÊÍÎÏÊÀ ÑĞÀÁÎÒÀËÀ! =====");
        SceneManager.LoadScene("Game");
    }

    public void ExitGame()
    {
        Debug.Log("===== ÂÛÕÎÄ =====");
        Application.Quit();
    }
}