using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBoat : Boat
{
    public enum EnemyBoatState
    {
        PATROLLING,
        ATTACKING,
        CHASE,
        SEARCHING,
        FROZEN,
    }

    [SerializeField]
    private EnemyBoatState _state = EnemyBoatState.PATROLLING;

    [Header("Detection")]
    [SerializeField]
    private float viewRange = 20f;

    [SerializeField]
    private float attackRange = 10f;

    [SerializeField]
    private float minAttackDistance = 5f;

    [Header("Movement")]
    [SerializeField]
    private float patrolSpeed = 2f;

    [SerializeField]
    private float chaseSpeed = 4f;

    [SerializeField]
    private List<Vector2> patrolPoints = new List<Vector2>();
    private int currentPatrolIndex = 0;

    [Header("Crew")]
    [SerializeField]
    private List<Enemy> enemies = new List<Enemy>();

    [SerializeField]
    private Transform[] enemyPositions = new Transform[3]; // Max 3 enemies

    private Player targetPlayer;
    private float distanceToPlayer;
    private Vector2 targetPosition;

    // Public accessor for enemy boat state with notification
    public EnemyBoatState state
    {
        get { return _state; }
        set
        {
            if (_state != value)
            {
                _state = value;
                OnStateChanged?.Invoke(_state);

                // Update enemy states based on boat state
                UpdateEnemyStates();
            }
        }
    }

    // Event to notify when the state changes
    public event Action<EnemyBoatState> OnStateChanged;

    private bool isFacingLeft = false;
    private Transform enemyPositionsContainer;

    private void Start()
    {
        // Find player if not assigned
        if (targetPlayer == null)
            targetPlayer = FindFirstObjectByType<Player>();

        // Set up enemy positions container
        SetupEnemyPositionsContainer();

        // Initialize enemies
        for (int i = 0; i < enemies.Count && i < enemyPositions.Length; i++)
        {
            if (enemyPositions[i] != null && enemies[i] != null)
            {
                enemies[i].transform.position = enemyPositions[i].position;
                enemies[i].transform.parent = transform;
                enemies[i].parentBoat = this;
            }
        }

        // If no patrol points, create a simple pattern around start position
        if (patrolPoints.Count == 0)
        {
            CreateDefaultPatrolPattern();
        }

        // Set initial speed
        speed = patrolSpeed;
    }

    private void Update()
    {
        if (targetPlayer == null)
            return;

        // Calculate distance to player
        distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.transform.position);

        // State machine
        switch (state)
        {
            case EnemyBoatState.PATROLLING:
                handlePatrolState();
                break;
            case EnemyBoatState.CHASE:
                handleChaseState();
                break;
            case EnemyBoatState.ATTACKING:
                handleAttackState();
                break;
            case EnemyBoatState.SEARCHING:
                handleSearchState();
                break;
        }

        // Check state transitions
        CheckStateTransitions();

        // Update enemy positions every frame
        UpdateEnemyPositions();
    }

    protected void handlePatrolState()
    {
        if (patrolPoints.Count == 0)
            return;

        // Move toward current patrol point
        Vector2 direction = (
            patrolPoints[currentPatrolIndex] - (Vector2)transform.position
        ).normalized;
        Move(direction);
        FaceDirection(direction);

        // Check if reached waypoint
        if (Vector2.Distance(transform.position, patrolPoints[currentPatrolIndex]) < 0.5f)
        {
            // Move to next patrol point
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
        }
    }

    protected void handleChaseState()
    {
        if (targetPlayer == null)
            return;

        // Calculate direction to player
        Vector2 directionToPlayer = (
            (Vector2)targetPlayer.transform.position - (Vector2)transform.position
        ).normalized;

        // Move toward player
        if (distanceToPlayer < minAttackDistance)
        {
            // Move away slightly
            Move(-directionToPlayer * 0.5f);
        }
        else
        {
            // Chase player
            Move(directionToPlayer);
        }

        FaceDirection(directionToPlayer);
    }

    protected void handleAttackState()
    {
        if (targetPlayer == null)
            return;

        // When attacking, try to position optimally
        Vector2 directionToPlayer = (
            (Vector2)targetPlayer.transform.position - (Vector2)transform.position
        ).normalized;

        // Maintain ideal attack distance
        if (distanceToPlayer < minAttackDistance)
        {
            Move(-directionToPlayer * 0.5f); // Back away slightly
        }
        else if (distanceToPlayer > attackRange)
        {
            Move(directionToPlayer * 0.5f); // Move closer slowly
        }
        else
        {
            // At good attack range, just rotate to face player
            FaceDirection(directionToPlayer);
        }
    }

    protected void handleSearchState()
    {
        // Last known position logic
        if (Vector2.Distance(transform.position, targetPosition) > 0.5f)
        {
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            Move(direction * 0.7f); // Move slower when searching

            FaceDirection(direction);
        }
        else
        {
            // At target search position, look around - use a different approach
            // Instead of rotating, just periodically flip the sprite
            if (Time.frameCount % 120 < 60) // Every ~2 seconds at 60fps
            {
                SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = !spriteRenderer.flipX;
                }
            }
        }
    }

    private void CheckStateTransitions()
    {
        // Check for state transitions based on player distance
        switch (state)
        {
            case EnemyBoatState.PATROLLING:
                if (distanceToPlayer <= viewRange)
                {
                    speed = chaseSpeed;
                    state = EnemyBoatState.CHASE;
                }
                break;

            case EnemyBoatState.CHASE:
                if (distanceToPlayer <= attackRange)
                {
                    state = EnemyBoatState.ATTACKING;
                }
                else if (distanceToPlayer > viewRange * 1.5f)
                {
                    targetPosition = targetPlayer.transform.position;
                    state = EnemyBoatState.SEARCHING;
                }
                break;

            case EnemyBoatState.ATTACKING:
                if (distanceToPlayer > attackRange * 1.2f && distanceToPlayer <= viewRange)
                {
                    state = EnemyBoatState.CHASE;
                }
                else if (distanceToPlayer > viewRange)
                {
                    targetPosition = targetPlayer.transform.position;
                    state = EnemyBoatState.SEARCHING;
                }
                break;

            case EnemyBoatState.SEARCHING:
                if (distanceToPlayer <= viewRange)
                {
                    speed = chaseSpeed;
                    state = EnemyBoatState.CHASE;
                }
                else if (Time.time % 20 < 0.1f) // After ~20 seconds of searching
                {
                    speed = patrolSpeed;
                    state = EnemyBoatState.PATROLLING;
                }
                break;
        }
    }

    private void SetupEnemyPositionsContainer()
    {
        // Find enemy positions container
        enemyPositionsContainer = transform.Find("EnemyPositions");

        if (enemyPositionsContainer == null)
        {
            Debug.LogWarning("EnemyPositions container not found on boat!");
            return;
        }

        // Ensure container has correct initial scale
        enemyPositionsContainer.localScale = new Vector3(1, 1, 1);

        // Store initial facing direction based on sprite
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            isFacingLeft = spriteRenderer.flipX;
        }

        // Find positions and store references
        for (int i = 0; i < enemyPositions.Length; i++)
        {
            Transform positionTransform = enemyPositionsContainer.Find($"EnemyPosition{i + 1}");
            if (positionTransform != null)
            {
                enemyPositions[i] = positionTransform;
            }
        }
    }

    // Modify the UpdateEnemyStates method to correctly handle both enemy states and positions
    private void UpdateEnemyStates()
    {
        if (enemies.Count == 0)
        {
            return; // Skip if no enemies
        }

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null)
                continue;

            // Update enemy state based on boat state
            switch (state)
            {
                case EnemyBoatState.PATROLLING:
                    enemy.SetState(Enemy.state.PATROLLING);
                    break;

                case EnemyBoatState.CHASE:
                    enemy.SetState(Enemy.state.CHASING);
                    break;

                case EnemyBoatState.ATTACKING:
                    enemy.SetState(Enemy.state.ATTACKING);
                    break;

                case EnemyBoatState.SEARCHING:
                    enemy.SetState(Enemy.state.SEARCHING);
                    break;
            }

            // Make sure enemy is at the correct position based on its index
            int index = enemies.IndexOf(enemy);
            if (index >= 0 && index < enemyPositions.Length && enemyPositions[index] != null)
            {
                Vector3 targetPos = enemyPositions[index].position;

                // Smoothly move to position
                enemy.transform.position = Vector3.Lerp(
                    enemy.transform.position,
                    targetPos,
                    Time.deltaTime * 5f
                );
            }
        }
    }

    // Add this method to your EnemyBoat class
    public void EnemyKilled(Enemy enemy)
    {
        // Remove the enemy from our list
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);

            // Log for debugging
            Debug.Log($"Enemy removed from boat. {enemies.Count} enemies remaining.");

            // Check if all enemies are dead
            if (enemies.Count == 0)
            {
                // All enemies are dead, perform boat destruction logic
                Debug.Log("All enemies on boat are dead!");

                // Optional: Drop loot, play sinking animation, etc.
                // Destroy(gameObject);  // Uncomment if you want to destroy the boat

                // Or set to a "defeated" state
                state = EnemyBoatState.FROZEN;
            }
        }
    }

    // Add this method to update enemy positions separately from state updates
    // Call this from Update() to make positioning smoother
    private void UpdateEnemyPositions()
    {
        if (enemies.Count == 0)
            return;

        // Update each enemy's position
        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy enemy = enemies[i];
            if (enemy == null)
                continue;

            // Skip enemies with indices beyond available positions
            if (i >= enemyPositions.Length || enemyPositions[i] == null)
                continue;

            // Get the target position
            Vector3 targetPos = enemyPositions[i].position;

            // Make sure enemy is parented to the boat (not to the positions container)
            if (enemy.transform.parent != transform)
            {
                enemy.transform.SetParent(transform);
            }

            // Set position directly - ensures enemies stay at their positions
            enemy.transform.position = targetPos;

            // Update enemy's facing direction based on boat's direction
            SpriteRenderer enemySprite = enemy.GetComponentInChildren<SpriteRenderer>();
            if (enemySprite != null)
            {
                enemySprite.flipX = isFacingLeft;
            }
        }
    }

    private void CreateDefaultPatrolPattern()
    {
        // Create a simple square patrol pattern around current position
        Vector2 pos = transform.position;
        float patrolRadius = 10f;

        patrolPoints.Add(new Vector2(pos.x + patrolRadius, pos.y));
        patrolPoints.Add(new Vector2(pos.x, pos.y + patrolRadius));
        patrolPoints.Add(new Vector2(pos.x - patrolRadius, pos.y));
        patrolPoints.Add(new Vector2(pos.x, pos.y - patrolRadius));
    }

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        // Show facing direction
        Gizmos.color = Color.white;
        Vector3 direction = isFacingLeft ? Vector3.left : Vector3.right;
        Gizmos.DrawRay(transform.position, direction * 2);

        // Draw the container if it exists
        if (enemyPositionsContainer != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(
                enemyPositionsContainer.position,
                new Vector3(2 * Mathf.Abs(enemyPositionsContainer.localScale.x), 1, 0)
            );
        }

        // Draw view range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRange);

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw min attack distance
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, minAttackDistance);

        // Draw patrol path
        if (patrolPoints.Count > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolPoints.Count; i++)
            {
                Gizmos.DrawSphere(patrolPoints[i], 0.3f);

                if (i < patrolPoints.Count - 1)
                    Gizmos.DrawLine(patrolPoints[i], patrolPoints[i + 1]);
                else
                    Gizmos.DrawLine(patrolPoints[i], patrolPoints[0]);
            }
        }
    }

    public void AddEnemy(Enemy enemy)
    {
        if (enemy != null && !enemies.Contains(enemy))
        {
            enemies.Add(enemy);
        }
    }

    public void ClearEnemies()
    {
        // Remove existing enemies
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        enemies.Clear();
    }

    private void OnDrawGizmos()
    {
        // Draw position markers for enemy positions
        Gizmos.color = Color.green;

        // Check for the container first
        Transform positionsContainer = transform.Find("EnemyPositions");
        if (positionsContainer != null)
        {
            // Draw the container bounds
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(positionsContainer.position, new Vector3(2, 1, 0));

            // Draw each position
            Gizmos.color = Color.green;
            for (int i = 1; i <= 3; i++)
            {
                Transform pos = positionsContainer.Find($"EnemyPosition{i}");
                if (pos != null)
                {
                    Gizmos.DrawSphere(pos.position, 0.3f);
                }
            }
        }
        else
        {
            // Fall back to direct child search (for backward compatibility)
            for (int i = 1; i <= 3; i++)
            {
                Transform pos = transform.Find($"EnemyPosition{i}");
                if (pos != null)
                {
                    Gizmos.DrawSphere(pos.position, 0.3f);
                }
            }
        }
    }

    private void FaceDirection(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Check if we're changing direction
            bool shouldFaceLeft = Mathf.Abs(angle) > 90;

            // If direction changed, update facing direction
            if (shouldFaceLeft != isFacingLeft)
            {
                isFacingLeft = shouldFaceLeft;

                // Flip sprite
                spriteRenderer.flipX = shouldFaceLeft;

                // Flip the entire enemy positions container
                if (enemyPositionsContainer != null)
                {
                    enemyPositionsContainer.localScale = new Vector3(shouldFaceLeft ? -1 : 1, 1, 1);

                    // Force immediate update of positions
                    for (int i = 0; i < enemyPositions.Length; i++)
                    {
                        if (enemyPositions[i] != null)
                        {
                            // Force Transform to update its world position
                            enemyPositions[i].position = enemyPositions[i].position;
                        }
                    }
                }
            }

            // Apply gentle tilt based on vertical movement
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Clamp(direction.y * 15f, -15f, 15f));
        }
    }

}
