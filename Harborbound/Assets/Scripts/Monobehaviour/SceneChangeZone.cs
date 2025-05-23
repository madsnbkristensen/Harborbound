using UnityEngine;

public class SceneChangeZone : MonoBehaviour
{
    [SerializeField]
    private string sceneName; // The name of the scene to load

    public void TravelToScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public string GetSceneName()
    {
        return sceneName;
    }
}
