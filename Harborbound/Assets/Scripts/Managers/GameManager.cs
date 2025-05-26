using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Properties from UML
    public int money = 0;
    public Player player;
    public Boat currentPlayerBoat;
    public List<Boat> unlockedBoats = new List<Boat>();

    public enum GameState
    {
        ROAMING,
        DIALOGUE,
        FISHING,
        DRIVING,
        MENU,
        INVENTORY,
        SHOPPING,
    }

    [SerializeField]
    private GameState _state = GameState.ROAMING;
    private bool isPaused = false;

    // Public accessor for game state with notification
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

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Subscribe to scene loading event
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Refind scene-specific references
        RefindSceneReferences();
    }

    private void RefindSceneReferences()
    {
        // Find player if not assigned or if current reference is null
        if (player == null)
            player = FindFirstObjectByType<Player>();

        // Find current player boat if not assigned or if current reference is null
        if (currentPlayerBoat == null)
            currentPlayerBoat = FindFirstObjectByType<Boat>();

        // Log what was found for debugging
        Debug.Log(
            $"GameManager refound references - Player: {(player != null ? "Found" : "Not found")}, Boat: {(currentPlayerBoat != null ? "Found" : "Not found")}"
        );
    }

    // Event system for state changes
    public delegate void GameStateChangedHandler(GameState newState);
    public event GameStateChangedHandler OnGameStateChanged;

    void Start()
    {
        if (player == null)
            player = FindFirstObjectByType<Player>();

        if (currentPlayerBoat == null)
            currentPlayerBoat = FindFirstObjectByType<Boat>();
    }

    // Methods from UML
    public void ChangeState(GameState newState)
    {
        // Store old state for comparison or reference
        GameState oldState = state;
        state = newState;

        // Log the state change for debugging
        //Debug.Log($"Game state changed from {oldState} to {state}");

        switch (state)
        {
            case GameState.ROAMING:
                // Handle roaming state initialization
                break;

            case GameState.DIALOGUE:
                // Handle dialogue state initialization
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
                // Handle inventory state initialization
                break;
        }
    }

    public void TogglePause(bool paused)
    {
        isPaused = paused;
        Time.timeScale = isPaused ? 0 : 1;
        Debug.Log($"Game paused: {isPaused}");
    }

    public void AddMoney(int amount)
    {
        money += amount;
        Debug.Log($"Added {amount} money. New balance: {money}");

        UIManager.Instance.UpdateMoneyDisplay(money);
    }

    public bool SpendMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            Debug.Log($"Spent {amount} money. Remaining balance: {money}");
            return true;
        }

        UIManager.Instance.UpdateMoneyDisplay(money);
        Debug.Log($"Not enough money to spend {amount}. Current balance: {money}");
        return false;
    }

    public void UnlockBoat(PlayerBoat boat)
    {
        if (boat != null && !unlockedBoats.Contains(boat))
        {
            unlockedBoats.Add(boat);
            Debug.Log($"Unlocked boat: {boat.name}");
        }
    }

    public void ChangePlayerBoat(PlayerBoat newBoat)
    {
        if (newBoat != null && unlockedBoats.Contains(newBoat))
        {
            // Store old boat reference
            Boat oldBoat = currentPlayerBoat;

            // Update current boat
            currentPlayerBoat = newBoat;

            Debug.Log($"Changed player boat from {oldBoat?.name ?? "None"} to {newBoat.name}");

            // Additional logic for changing boat (repositioning player, etc.)
        }
        else
        {
            Debug.LogWarning("Attempted to change to an unavailable boat");
        }
    }

    public void OnPlayerDeath()
    {
        Debug.Log("Player has died!");
        AudioManager.Instance.Play(AudioManager.SoundType.Death);
        // Handle player death (game over screen, reset, etc.)
    }

    public void RespawnPlayer()
    {
        if (player != null)
        {
            // Reset player health
            player.currentHealth = player.maxHealth;

            // Position player at spawn point
            // This would need customization based on your game's spawn system

            Debug.Log("Player respawned");
        }
    }

    public void SaveGame()
    {
        Debug.Log("Saving game...");
        // Implement save game logic
    }

    public void LoadGame()
    {
        Debug.Log("Loading game...");
        // Implement load game logic
    }
}
