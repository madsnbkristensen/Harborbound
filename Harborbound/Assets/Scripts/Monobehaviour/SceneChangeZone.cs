using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneChangeZone : MonoBehaviour
{
    [SerializeField]
    private string oceanSceneName = "karlo2";
    [SerializeField]
    private string islandSceneName = "Main island";

    public void TravelToScene()
    {
        Debug.Log($"[SceneChange] TravelToScene called");
        Scene currentScene = SceneManager.GetActiveScene();
        Debug.Log($"[SceneChange] Current scene: {currentScene.name}");

        if (currentScene.name == oceanSceneName)
        {
            Debug.Log($"[SceneChange] Going from ocean to island");
            AudioManager.Instance.ChangeMusic(AudioManager.SoundType.Music_Island);
            SceneManager.LoadScene(islandSceneName);
        }
        else if (currentScene.name == islandSceneName)
        {
            Debug.Log($"[SceneChange] Going from island to ocean");
            AudioManager.Instance.ChangeMusic(AudioManager.SoundType.Music_Ocean);
            SceneManager.LoadScene(oceanSceneName);
        }
    }

}