using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 1. GameManager defines and owns the game state
    public enum GameState { ROAMING, DIALOGUE, FISHING, DRIVING, MENU, INVENTORY }

    // player reference
    public Player player;

    // 2. Event system for state changes
    public delegate void GameStateChangedHandler(GameState newState);
    public event GameStateChangedHandler OnGameStateChanged;

    // 3. Property with notification
    [Header("Game State")]
    [SerializeField]
    private GameState _state = GameState.ROAMING;
    public GameState state
    {
        get { return _state; }
        set
        {
            if (_state != value)
            {
                _state = value;
                // Notify listeners
                OnGameStateChanged?.Invoke(_state);
            }
        }
    }


    void Start()
    {
        if (player == null)
            player = FindFirstObjectByType<Player>();
    }

    // Method to change state
    public void ChangeState(GameState newState)
    {
        // Store old state for comparison or reference
        GameState oldState = state;
        state = newState;

        // Log the state change for debugging
        Debug.Log($"Game state changed from {oldState} to {state}");

        switch (state)
        {
            case GameState.ROAMING:
                // Handle roaming state initialization
                break;

            case GameState.DIALOGUE:
                // Handle dialogue state initialization
                // This is where you would initialize UI, freeze non-player elements, etc.
                break;

            case GameState.FISHING:
                // Handle fishing state initialization
                break;

            case GameState.DRIVING:
                // Handle driving state initialization
                break;

            case GameState.MENU:
                // Handle menu state initialization
                break;
            case GameState.INVENTORY:
                // Handle menu/inventory states
                break;
        }
    }
}

