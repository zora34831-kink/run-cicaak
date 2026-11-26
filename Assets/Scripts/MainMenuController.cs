using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Tooltip("Set the exact scene names in Build Settings.")]
    public string gameplayScene = "Game";
    public string menuScene = "MainMenu";

    public void Play()
    {
        if (Application.CanStreamedLevelBeLoaded(gameplayScene))
            SceneManager.LoadScene(gameplayScene);
        else
            Debug.LogError("RUN CICAAK: Scene '" + gameplayScene + "' belum dimasukkan ke Build Settings.");
    }

    public void OpenMenu()
    {
        if (Application.CanStreamedLevelBeLoaded(menuScene))
            SceneManager.LoadScene(menuScene);
    }

    public void Quit()
    {
        Application.Quit();
    }
}