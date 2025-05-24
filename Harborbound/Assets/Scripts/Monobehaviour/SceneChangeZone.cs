using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeZone : MonoBehaviour
{
    [SerializeField]
    private string oceanSceneName = "OceanScene";
    [SerializeField]
    private string islandSceneName = "IslandScene";

    public void TravelToScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.name == oceanSceneName)
        {
            AudioManager.Instance.ChangeMusic(AudioManager.SoundType.Music_Island);
            SceneManager.LoadScene(islandSceneName);
        }
        else if (currentScene.name == islandSceneName)
        {
            AudioManager.Instance.ChangeMusic(AudioManager.SoundType.Music_Ocean);
            SceneManager.LoadScene(oceanSceneName);
        }
    }
}