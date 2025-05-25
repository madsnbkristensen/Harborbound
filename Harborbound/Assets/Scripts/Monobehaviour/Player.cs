using UnityEngine;
using System.Collections.Generic;

public class Player : Humanoid
{
    public GameManager gameManager;
    public PlayerBoat playerBoat;
    private Friend currentInteractableFriend;
    private PlayerEquipment playerEquipment;
    private SpriteRenderer playerSpriteRenderer;
    private Vector2 moveDirection;
    public Item hoveredItem;
    public List<string> interactableTags;

    public bool startDriving = true;

    private SceneChangeZone sceneChangeZone;

    // Add this line to declare the missing variable
    private Transform boatWheelPosition;

    [Header("Interaction Settings")]
    public float interactionRange = 2f;
    public KeyCode interactionKey = KeyCode.E;
    public KeyCode inventoryKey = KeyCode.Tab;
    private Vector3 lastPositionBeforeDriving;

    private Animator animator;

    protected override void Start()
    {
        base.Start();
        playerSpriteRenderer = GetComponent<SpriteRenderer>();

        animator = GetComponent<Animator>();

        // Find GameManager if needed
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        // Subscribe to game state changes
        if (gameManager != null)
            gameManager.OnGameStateChanged += HandleGameStateChanged;

        // Change the FindFirstObjectByType to look for PlayerBoat
        if (playerBoat == null)
            playerBoat = FindFirstObjectByType<PlayerBoat>();

        if (boatWheelPosition == null && playerBoat != null)
            boatWheelPosition = playerBoat.wheelPosition;

        // Get the PlayerEquipment component
        playerEquipment = GetComponent<PlayerEquipment>();

        // Ensure basic collision setup
        EnsureBasicCollisionSetup();

        if (startDriving)
        {
            // Start driving if the player is already in the boat
            StartDriving();
        }
    }

    private void EnsureBasicCollisionSetup()
    {
        // Make sure player has a Collider2D
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null)
        {
            Debug.LogError("Player has no Collider2D! Adding CircleCollider2D");
            playerCollider = gameObject.AddComponent<CircleCollider2D>();
        }

        // Make sure player collider is NOT a trigger
        playerCollider.isTrigger = false;

        // Make sure player has a Rigidbody2D (needed for collisions)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Player has no Rigidbody2D! Adding one for collision");
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        // Configure Rigidbody for character movement
        rb.gravityScale = 0; // No gravity in top-down
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Don't rotate from collisions
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.linearDamping = 0.5f; // Add some drag to prevent sliding

        // IMPORTANT - Make sure we're using the right physics type for movement
        rb.bodyType = RigidbodyType2D.Dynamic;

        // Make sure we're on the correct layer for collisions
        gameObject.layer = LayerMask.NameToLayer("Player");

        Debug.Log("Player collision setup complete - Player should now collide properly");
    }

    // This is now correctly overriding the Move method from the Humanoid class
    protected override void Move(Vector2 direction)
    {
        if (direction.magnitude < 0.1f) return;

        // Use proper physics movement
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Calculate movement with proper time delta
            Vector2 movement = direction * speed * Time.fixedDeltaTime;
            Vector2 newPosition = rb.position + movement;

            // Use Rigidbody2D.MovePosition for proper physics movement
            rb.MovePosition(newPosition);

            // Debug.DrawLine(rb.position, newPosition, Color.red, 0.1f); // Visualize movement
        }
        else
        {
            // Fallback to transform movement if no rigidbody (shouldn't happen now)
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            Debug.LogWarning("Using transform movement - collisions may not work properly");
        }
    }

    // React to game state changes
    private void HandleGameStateChanged(GameManager.GameState newState)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Collider2D col = GetComponent<Collider2D>();

        switch (newState)
        {
            case GameManager.GameState.ROAMING:
                // Show player sprite
                if (playerSpriteRenderer != null)
                    playerSpriteRenderer.enabled = true;

                // Enable collider
                if (col != null)
                    col.enabled = true;

                // Unfreeze rigidbody, but keep rotation frozen
                if (rb != null)
                {
                    rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                    rb.bodyType = RigidbodyType2D.Dynamic; // Make sure physics is active
                }
                break;

            case GameManager.GameState.DRIVING:
                // Hide player sprite while driving
                if (playerSpriteRenderer != null)
                    playerSpriteRenderer.enabled = false;

                // Disable player's collider while driving
                if (col != null)
                    col.enabled = false;
                break;

            case GameManager.GameState.DIALOGUE:
            case GameManager.GameState.INVENTORY:
            case GameManager.GameState.SHOPPING:
                // Freeze player during these states
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.constraints = RigidbodyConstraints2D.FreezeAll;
                }
                break;
        }
    }

    void Update()
    {
        if (gameManager == null) return;

        // if press m 
        if (Input.GetKeyDown(KeyCode.M))
        {
            AudioManager.Instance.ChangeMusic(AudioManager.SoundType.Music_Ocean_Loop);
        }

        // check interactable objects within range
        GetInteractableObjects();
        CallHelperManager(GetInteractableObjects());

        // Get input in Update
        moveDirection = GetInputDirection();

        // Handle inventory toggle
        if (Input.GetKeyDown(inventoryKey))
        {
            ToggleInventory();
            return;
        }

        // Handle escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameManager.state != GameManager.GameState.ROAMING)
                gameManager.ChangeState(GameManager.GameState.ROAMING);
            else
                gameManager.ChangeState(GameManager.GameState.MENU);
            return;
        }

        // State-specific non-movement logic
        switch (gameManager.state)
        {
            case GameManager.GameState.ROAMING:
                // Check for interaction
                if (Input.GetKeyDown(interactionKey))
                    TryInteract();
                break;

            case GameManager.GameState.DRIVING:
                // Control the boat
                if (moveDirection.magnitude > 0.1f && playerBoat != null)
                    playerBoat.Move(moveDirection);

                // Exit the boat
                if (Input.GetKeyDown(interactionKey))
                    StopDriving();
                break;

            case GameManager.GameState.DIALOGUE:
                // Dialogue navigation
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                    Debug.Log("Advancing dialogue...");
                else if (Input.GetKeyDown(KeyCode.Escape))
                    ExitDialogue();
                break;

            case GameManager.GameState.SHOPPING:
                // Handle right-click to sell
                if (Input.GetMouseButtonDown(1))
                {
                    InventoryManager2.Instance.SellItem(hoveredItem);
                }
                break;
        }
    }

    void FixedUpdate()
    {
        // Apply movement in FixedUpdate for proper physics
        if (gameManager.state == GameManager.GameState.ROAMING && moveDirection.magnitude > 0.1f)
        {
            Move(moveDirection);
        }
    }

    // Add collision debug callback
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Player collided with: {collision.gameObject.name}", collision.gameObject);
    }

    public void TryInteract()
    {

        if (IsNearSceneChangeZone())
        {
            SceneChangeZone sceneChangeZone = FindFirstObjectByType<SceneChangeZone>();
            sceneChangeZone.TravelToScene();
            return;
        }

        // Check boat interaction first
        if (IsNearBoatWheel())
        {
            Debug.Log("Starting to drive boat");
            StartDriving();
            return;
        }

        // Then check NPC interaction
        if (IsNearFriend())
        {
            Debug.Log($"Interacting with {currentInteractableFriend.humanoidName}");

            switch (currentInteractableFriend.friendType)
            {
                case Friend.type.MERCHANT:
                    gameManager.ChangeState(GameManager.GameState.SHOPPING);
                    break;
                case Friend.type.QUEST_GIVER:
                case Friend.type.NPC:
                default:
                    gameManager.ChangeState(GameManager.GameState.DIALOGUE);
                    break;
            }

            currentInteractableFriend.StartDialogue();
            return;
        }

        Debug.Log("Nothing to interact with nearby");
    }

    private void ToggleInventory()
    {
        if (gameManager == null) return;

        Debug.Log("Toggling inventory");

        if (gameManager.state == GameManager.GameState.INVENTORY)
        {
            gameManager.ChangeState(GameManager.GameState.ROAMING);
            InventoryManager2.Instance.SetHoveringFalse();
        }
        else
            gameManager.ChangeState(GameManager.GameState.INVENTORY);

        // set all items isHovering to false
    }

    private bool IsNearFriend()
    {
        Friend[] nearbyFriends = FindObjectsByType<Friend>(FindObjectsSortMode.None);

        foreach (Friend friend in nearbyFriends)
        {
            float distance = Vector2.Distance(transform.position, friend.transform.position);

            if (distance <= interactionRange)
            {
                currentInteractableFriend = friend;
                return true;
            }
        }

        currentInteractableFriend = null;
        return false;
    }

    private bool IsNearBoatWheel()
    {
        if (playerBoat == null || playerBoat.wheelPosition == null)
            return false;

        float distance = Vector2.Distance(transform.position, playerBoat.wheelPosition.position);
        return distance <= interactionRange;
    }

    private bool IsNearSceneChangeZone()
    {
        SceneChangeZone sceneChangeZone = FindFirstObjectByType<SceneChangeZone>();
        if (sceneChangeZone == null) return false;

        BoxCollider2D collider = sceneChangeZone.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            return collider.OverlapPoint(transform.position);
        }
        return false;
    }

    public void StartDriving()
    {
        if (playerBoat == null)
        {
            Debug.LogWarning("Cannot start driving: no player boat assigned");
            return;
        }

        // unequip any currently equipped item
        if (playerEquipment != null)
        {
            playerEquipment.EquipItem(null);
        }

        // Store current position
        lastPositionBeforeDriving = transform.position;
        // Position player at wheel
        transform.position = boatWheelPosition.position;
        // Parent to boat
        transform.SetParent(playerBoat.transform);

        // Start the boat engine sound
        AudioManager.Instance.PlayPersistent(AudioManager.SoundType.Boat_Engine, true);

        // Change game state
        if (gameManager != null)
            gameManager.ChangeState(GameManager.GameState.DRIVING);
    }

    public void StopDriving()
    {
        Debug.Log("Stopping driving boat");

        // Stop the boat engine sound
        PlayerBoat boat = GetComponentInParent<PlayerBoat>();
        if (boat != null)
        {
            boat.StopEngine();
        }

        // Ensure player is visible
        if (playerSpriteRenderer != null)
            playerSpriteRenderer.enabled = true;
        // Store current wheel position before unparenting
        Vector3 exitPosition = boatWheelPosition.position;
        // Add slight upward offset to correct the position
        exitPosition.y += 0.0f; // Adjust this value as needed (0.5 units up)
                                // Unparent from boat
        transform.SetParent(null);
        // Move to the adjusted wheel position
        transform.position = exitPosition;
        // Re-enable player collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = true;
        // Change game state
        if (gameManager != null)
            gameManager.ChangeState(GameManager.GameState.ROAMING);
    }

    private void ExitDialogue()
    {
        if (currentInteractableFriend != null)
            currentInteractableFriend.EndDialogue();

        if (gameManager != null)
            gameManager.ChangeState(GameManager.GameState.ROAMING);
    }

    private Vector2 GetInputDirection()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            vertical = 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            vertical = -1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            horizontal = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            horizontal = 1f;

        // Then set animation direction based on movement
        // Handle sprite flipping based on horizontal direction
        if (horizontal != 0 && playerSpriteRenderer != null)
        {
            // Set flipX to true when moving left, false when moving right
            playerSpriteRenderer.flipX = (horizontal < 0);
        }

        if (animator != null)
        {
            if (vertical > 0)
                animator.SetInteger("direction", 0);  // Up
            else if (vertical < 0)
                animator.SetInteger("direction", 2);  // Down
            else if (horizontal != 0)
                animator.SetInteger("direction", 1);  // Use right animation for both left and right
            else
                animator.SetInteger("direction", -1); // Idle or default state

            // This triggers animations to play once
            if (moveDirection.magnitude > 0.1f)
                animator.SetBool("isMoving", true);
            else
                animator.SetBool("isMoving", false);
        }

        return new Vector2(horizontal, vertical).normalized;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        // Update to use playerBoat.wheelPosition
        if (playerBoat != null && playerBoat.wheelPosition != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(playerBoat.wheelPosition.position, 0.3f);
        }
    }

    void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged -= HandleGameStateChanged;
        }

    }

    // function to detect interActable objects within interaction range
    private List<GameObject> GetInteractableObjects()
    {
        List<GameObject> interactableObjects = new List<GameObject>();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactionRange);

        foreach (Collider2D collider in colliders)
        {
            // Check if the collider has a tag that matches any of the interactable tags
            foreach (string tag in interactableTags)
            {
                if (collider.CompareTag(tag))
                {
                    // Add the game object to the list
                    interactableObjects.Add(collider.gameObject);
                    break; // No need to check other tags for this collider
                }
            }
        }

        return interactableObjects;
    }

    // function to call helper manager based on interactable objects within range
    private void CallHelperManager(List<GameObject> interactableObjects)
    {
        foreach (GameObject obj in interactableObjects)
        {
            if (HelperManager.Instance != null)
            {
                HelperManager.Instance.HandleInteraction(obj);
            }
        }
    }

    public override void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        UIManager.Instance.UpdateHealthDisplay(currentHealth, maxHealth);

    }
}
