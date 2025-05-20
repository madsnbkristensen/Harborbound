using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// Ensures that there's always exactly one EventSystem in the game,
/// preventing issues with UI interactions across scene transitions.
/// </summary>
[DefaultExecutionOrder(-30)] // Execute after GameManager and before other managers
public class EventSystemManager : MonoBehaviour
{
    public static EventSystemManager Instance { get; private set; }

    private EventSystem managedEventSystem;

    private void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[EventSystemManager] Duplicate EventSystemManager detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Get reference to our EventSystem
        managedEventSystem = GetComponent<EventSystem>();
        if (managedEventSystem == null)
        {
            managedEventSystem = gameObject.AddComponent<EventSystem>();
            gameObject.AddComponent<StandaloneInputModule>();
            Debug.Log("[EventSystemManager] Created new EventSystem components");
        }

        // Register for scene loading events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find and disable/destroy any other EventSystems in the new scene
        EventSystem[] otherEventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);

        foreach (EventSystem eventSystem in otherEventSystems)
        {
            if (eventSystem != managedEventSystem)
            {
                Debug.Log($"[EventSystemManager] Disabling duplicate EventSystem in scene {scene.name}");
                Destroy(eventSystem.gameObject);
            }
        }

        // Ensure our managed EventSystem is active
        if (managedEventSystem != null && !managedEventSystem.gameObject.activeSelf)
        {
            managedEventSystem.gameObject.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from scene loading events
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
