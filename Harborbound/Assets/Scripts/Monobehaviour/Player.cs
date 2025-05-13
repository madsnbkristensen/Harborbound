using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class Player : Humanoid
{
    public GameManager gameManager;
    public Boat playerBoat;
    private Friend currentInteractableFriend;


    [Header("Interaction Settings")]
    public float interactionRange = 2f;
    public KeyCode interactionKey = KeyCode.E;
    public Transform boatWheelPosition;
    private Vector3 lastPositionBeforeDriving;


    protected override void Start()
    {

        base.Start();

        // Find GameManager if needed
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        // Subscribe to game state changes to react
        if (gameManager != null)
            gameManager.OnGameStateChanged += HandleGameStateChanged;

        if (playerBoat == null)
            playerBoat = FindFirstObjectByType<Boat>();

        if (boatWheelPosition == null && playerBoat != null)
            boatWheelPosition = playerBoat.wheelPosition;
    }

    // React to game state changes
    private void HandleGameStateChanged(GameManager.GameState newState)
    {
        // Get components if needed
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Collider2D col = GetComponent<Collider2D>();

        switch (newState)
        {
            case GameManager.GameState.ROAMING:
                // Enable movement components
                if (col != null) col.enabled = true;
                // Unfreeze rigidbody if you're using physics-based movement
                if (rb != null) rb.constraints = RigidbodyConstraints2D.None;
                break;

            case GameManager.GameState.DIALOGUE:
                // Freeze player during dialogue
                if (rb != null)
                {
                    // Stop any current movement
                    rb.linearVelocity = Vector2.zero;
                    // Freeze position but allow rotation if needed
                    rb.constraints = RigidbodyConstraints2D.FreezePosition;
                }
                break;

            case GameManager.GameState.DRIVING:
                // Disable player's collider when driving
                if (col != null) col.enabled = false;
                break;

                // Handle other states as needed...
        }
    }

    void Update()
    {
        if (gameManager == null) return;

        Vector2 inputDirection = GetInputDirection();

        switch (gameManager.state)
        {
            case GameManager.GameState.ROAMING:
                // Move the player
                if (inputDirection.magnitude > 0.1f)
                    Move(inputDirection);

                // Check for interaction key press
                if (UnityEngine.Input.GetKeyDown(interactionKey))
                {
                    TryInteract();
                }
                break;

            case GameManager.GameState.DRIVING:
                // Control the boat
                if (inputDirection.magnitude > 0.1f && playerBoat != null)
                    playerBoat.Move(inputDirection);

                // Exit the boat
                if (UnityEngine.Input.GetKeyDown(interactionKey))
                    StopDriving();
                break;

            // Other states...
            case GameManager.GameState.DIALOGUE:
                // Player is frozen during dialogue
                // Only check for input to advance or exit dialogue
                if (UnityEngine.Input.GetKeyDown(KeyCode.Space) ||
                    UnityEngine.Input.GetKeyDown(KeyCode.Return))
                {
                    // Signal to advance dialogue
                    Debug.Log("Advancing dialogue...");
                }
                else if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
                {
                    // Exit dialogue
                    ExitDialogue();
                }
                break;

        }
    }

    public void TryInteract()
    {
        // Create a list of potential interactions
        var interactions = new List<(string type, Action action, float priority)>
    {
        // Boat interaction
        ("Boat", () => {
            Debug.Log("Player is near the boat wheel. Starting to drive.");
            StartDriving();
        }, 2.0f), // Higher priority number
        
        // Friend interaction
        ("Friend", () => {
            Debug.Log($"Player is near {currentInteractableFriend.humanoidName}. Starting dialogue.");
            currentInteractableFriend.StartDialogue();
            if (gameManager != null)
                gameManager.ChangeState(GameManager.GameState.DIALOGUE);
        }, 1.0f),
        
        // You can add more interactions here as needed
        // ("FishingSpot", () => { /* fishing logic */ }, 1.0f),
    };

        // Filter out interactions that aren't available
        var availableInteractions = interactions.Where(i =>
            (i.type == "Boat" && IsNearBoatWheel()) ||
            (i.type == "Friend" && IsNearFriend())
        // Add more conditions for other interaction types
        ).ToList();

        if (availableInteractions.Count > 0)
        {
            // Sort by priority (highest first) and execute the top action
            availableInteractions.OrderByDescending(i => i.priority).First().action();
        }
        else
        {
            Debug.Log("Nothing to interact with nearby.");
            // No interactions available
        }
    }

    private bool IsNearFriend()
    {
        // Find all Friend objects in the scene
        Friend[] nearbyFriends = FindObjectsByType<Friend>(FindObjectsSortMode.None);

        foreach (Friend friend in nearbyFriends)
        {
            // Calculate distance between player and this friend
            float distance = Vector2.Distance(transform.position, friend.transform.position);

            // Check if within interaction range
            if (distance <= interactionRange)
            {
                // Store reference to the closest friend
                currentInteractableFriend = friend;
                return true;
            }
        }

        // No friends within range
        currentInteractableFriend = null;
        return false;
    }

    private bool IsNearBoatWheel()
    {
        if (playerBoat == null || boatWheelPosition == null)
            return false;

        float distance = Vector2.Distance(transform.position, boatWheelPosition.position);
        return distance <= interactionRange;
    }

    public void StartDriving()
    {
        if (playerBoat == null)
        {
            Debug.LogWarning("Cannot start driving: no player boat assigned");
            return;
        }

        Debug.Log("Starting to drive boat");

        // Store current position before getting on boat
        lastPositionBeforeDriving = transform.position;

        // Position the player at the wheel
        transform.position = boatWheelPosition.position;

        // Parent the player to the boat
        transform.SetParent(playerBoat.transform);

        // Change game state
        if (gameManager != null)
            gameManager.ChangeState(GameManager.GameState.DRIVING);
    }

    public void StopDriving()
    {
        Debug.Log("Stopping driving boat");

        // Unparent from the boat
        transform.SetParent(null);

        // Move the player slightly away from the wheel to prevent immediate re-entry
        if (boatWheelPosition != null)
        {
            Vector2 awayDirection = (transform.position - boatWheelPosition.position).normalized;
            transform.position += (Vector3)awayDirection * 1.0f;
        }

        if (boatWheelPosition != null)
        {
            // Calculate a position slightly away from the wheel
            Vector2 awayDirection = (transform.position - boatWheelPosition.position).normalized;
            transform.position = boatWheelPosition.position + (Vector3)awayDirection * 1.5f;
        }

        // Change game state
        if (gameManager != null)
            gameManager.ChangeState(GameManager.GameState.ROAMING);
    }

    private void ExitDialogue()
    {
        if (currentInteractableFriend != null)
        {
            // Let the friend know dialogue has ended (we'll add this method to Friend)
            // currentInteractableFriend.EndDialogue();
        }

        // Return to roaming state
        if (gameManager != null)
            gameManager.ChangeState(GameManager.GameState.ROAMING);
    }

    private Vector2 GetInputDirection()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (UnityEngine.Input.GetKey(KeyCode.W) || UnityEngine.Input.GetKey(KeyCode.UpArrow))
            vertical = 1f;
        if (UnityEngine.Input.GetKey(KeyCode.S) || UnityEngine.Input.GetKey(KeyCode.DownArrow))
            vertical = -1f;
        if (UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow))
            horizontal = -1f;
        if (UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow))
            horizontal = 1f;

        return new Vector2(horizontal, vertical).normalized;
    }

    // Visual indicator of interaction range (for debugging)
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        if (boatWheelPosition != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(boatWheelPosition.position, 0.3f);
        }
    }

    void OnDestroy()
    {
        if (gameManager != null)
            gameManager.OnGameStateChanged -= HandleGameStateChanged;
    }
}
