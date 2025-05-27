using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    public string openingSceneName = "OpeningScene";
    
    // Method to be called by the New Game button
    public void StartNewGame()
    {
        // Load the opening scene
        SceneManager.LoadScene(openingSceneName);
    }
    
    // Method to be called by the Quit button
    public void QuitGame()
    {
        // Quit the application
        #if UNITY_EDITOR
            // Stop playing the scene in the editor
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // Quit the built application
            Application.Quit();
        #endif
    }
}
