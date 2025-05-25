using UnityEngine;

public class Follow_Player : MonoBehaviour
{
    public Transform player;
    private Player playerComponent;
    private GameManager gameManager;

    [SerializeField]
    private Vector3 offsetFromTarget = new Vector3(0, 1, -5);

    [SerializeField]
    private bool debugMode = false;

    // Track which transform to follow
    private Transform currentTargetToFollow;

    private void Start()
    {
        // Find player if not assigned
        if (player == null)
        {
            playerComponent = FindFirstObjectByType<Player>();
            if (playerComponent != null)
            {
                player = playerComponent.transform;
            }
        }
        else
        {
            playerComponent = player.GetComponent<Player>();
        }

        // Get GameManager instance
        gameManager = GameManager.Instance;

        if (gameManager != null)
        {
            // Subscribe to state change events
            gameManager.OnGameStateChanged += OnGameStateChanged;

            // Initialize with current state
            OnGameStateChanged(gameManager.state);
        }
        else
        {
            Debug.LogError("GameManager instance not found!");
        }
    }

    private void OnDestroy()
    {
        // Clean up event subscription
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged -= OnGameStateChanged;
        }
    }

    // Called when game state changes
    private void OnGameStateChanged(GameManager.GameState newState)
    {
        if (debugMode)
            Debug.Log($"Follow_player received state change: {newState}");

        // Update the target to follow based on new state
        if (newState == GameManager.GameState.DRIVING)
        {
            currentTargetToFollow = playerComponent.playerBoat.transform;
            if (debugMode)
                Debug.Log("Now following boat due to state change");
        }
        else
        {
            currentTargetToFollow = player;
            if (debugMode)
                Debug.Log("Now following player due to state change");
        }
    }

    // Use LateUpdate for smoother camera follow
    void LateUpdate()
    {
        // Safety check
        if (currentTargetToFollow == null)
        {
            // Fallback to player if current target is null
            if (player != null)
            {
                currentTargetToFollow = player;
            }
            else
            {
                return;
            }
        }

        // Actually move the camera - this was missing
        transform.position = currentTargetToFollow.position + offsetFromTarget;
    }
}
