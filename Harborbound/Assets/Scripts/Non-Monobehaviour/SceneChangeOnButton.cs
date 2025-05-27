using UnityEngine;

public class SceneChangeOnButton : MonoBehaviour
{
    public string sceneName; // The name of the scene to load
    public string secondSceneName; // The name of the second scene to load
    public KeyCode keyToPress = KeyCode.C;

    // This method is called when the button is clicked
    public void Update()
    {
        // Check if the key is pressed
        if (Input.GetKeyDown(keyToPress))
        {
            // if scene to change to already the scene, switch back to second scene
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == sceneName)
            {
                // Load the second scene
                UnityEngine.SceneManagement.SceneManager.LoadScene(secondSceneName);
                return;
            }
            // Load the specified scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
}
