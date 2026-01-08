using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Settings")]
    public string gameSceneName = "SampleScene"; // Name of your actual gameplay scene

    public void PlayGame()
    {
        // Load the game scene
        // Ensure the scene is added to Build Settings!
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game Requested");
        Application.Quit();
    }
}
