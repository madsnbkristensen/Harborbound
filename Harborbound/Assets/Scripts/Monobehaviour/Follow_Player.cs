using UnityEngine;
using UnityEngine.SceneManagement;

public class Follow_player : MonoBehaviour
{
    public Transform player;
    public bool shouldFollow = true;

    // Create a list of scene-specific zoom settings
    [System.Serializable]
    public class SceneZoomSettings
    {
        public string sceneName;
        public float normalZoom = 5.0f;
        public float zoomedOutDistance = 10.0f;
    }

    public SceneZoomSettings[] sceneSettings;

    // Default values if no scene-specific settings are found
    public float defaultNormalZoom = 5.0f;     // Default orthographic size
    public float defaultZoomedOutDistance = 10.0f;  // Zoomed out orthographic size
    public float zoomSpeed = 2.0f;      // How fast to zoom

    private Camera mainCamera;
    private SpriteRenderer playerSpriteRenderer;
    private float currentZoom;
    private float normalZoom;
    private float zoomedOutDistance;

    // find player by object and add
    private void Start()
    {
        if (player == null)
            player = FindFirstObjectByType<Player>().transform;

        if (player != null)
            playerSpriteRenderer = player.GetComponent<SpriteRenderer>();

        mainCamera = GetComponent<Camera>();

        // Get the current scene name
        string currentScene = SceneManager.GetActiveScene().name;

        // Look for scene-specific settings
        bool foundSceneSettings = false;
        foreach (var setting in sceneSettings)
        {
            if (setting.sceneName == currentScene)
            {
                normalZoom = setting.normalZoom;
                zoomedOutDistance = setting.zoomedOutDistance;
                foundSceneSettings = true;
                break;
            }
        }

        // Use defaults if no scene-specific settings
        if (!foundSceneSettings)
        {
            normalZoom = defaultNormalZoom;
            zoomedOutDistance = defaultZoomedOutDistance;
        }

        currentZoom = normalZoom;
        mainCamera.orthographicSize = currentZoom;
    }

    void LateUpdate()
    {
        if (player != null)
        {
            // Check if sprite renderer is enabled or not
            float targetZoom = normalZoom;
            if (playerSpriteRenderer != null)
            {
                targetZoom = playerSpriteRenderer.enabled ? normalZoom : zoomedOutDistance;
            }

            // Smoothly interpolate between current and target orthographic size
            currentZoom = Mathf.Lerp(
                currentZoom,
                targetZoom,
                Time.deltaTime * zoomSpeed
            );

            // Apply the zoom to the camera
            mainCamera.orthographicSize = currentZoom;

            // Update camera position (keeping a fixed Z distance)
            Vector3 targetPosition = new Vector3(
                player.transform.position.x,
                player.transform.position.y + 1,
                transform.position.z  // Keep the same Z position
            );

            transform.position = targetPosition;
        }
        else
        {
            Debug.LogWarning("Player not found! Please assign the player object in the inspector or ensure it exists in the scene.");
        }
    }
}
