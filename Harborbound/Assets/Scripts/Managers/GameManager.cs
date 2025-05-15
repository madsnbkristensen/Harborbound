using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // Properties from UML
    public int money = 0;
    public Player player;
    public Boat currentPlayerBoat;
    public List<Boat> unlockedBoats = new List<Boat>();
    public enum GameState { ROAMING, DIALOGUE, FISHING, DRIVING, MENU, INVENTORY }
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
        Debug.Log($"Game state changed from {oldState} to {state}");

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

        // Notify UI or other systems about money change
        // You might want to add an event for this
    }

    public bool SpendMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            Debug.Log($"Spent {amount} money. Remaining balance: {money}");
            return true;
        }

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
