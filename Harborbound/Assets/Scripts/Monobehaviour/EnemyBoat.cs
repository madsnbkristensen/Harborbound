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
        FROZEN
    }

    [SerializeField]
    private EnemyBoatState _state = EnemyBoatState.PATROLLING;

    [Header("Detection")]
    [SerializeField] private float viewRange = 20f;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float minAttackDistance = 5f;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private List<Vector2> patrolPoints = new List<Vector2>();
    private int currentPatrolIndex = 0;

    [Header("Crew")]
    [SerializeField] private List<Enemy> enemies = new List<Enemy>();
    [SerializeField] private Transform[] enemyPositions = new Transform[3]; // Max 3 enemies

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

    private void Start()
    {
        // Find player if not assigned
        if (targetPlayer == null)
            targetPlayer = FindFirstObjectByType<Player>();

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
        if (targetPlayer == null) return;

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
    }

    protected void handlePatrolState()
    {
        if (patrolPoints.Count == 0) return;

        // Move toward current patrol point
        Vector2 direction = (patrolPoints[currentPatrolIndex] - (Vector2)transform.position).normalized;
        Move(direction);

        // Check if reached waypoint
        if (Vector2.Distance(transform.position, patrolPoints[currentPatrolIndex]) < 0.5f)
        {
            // Move to next patrol point
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
        }
    }

    protected void handleChaseState()
    {
        // Calculate distance maintaining direction
        Vector2 directionToPlayer = ((Vector2)targetPlayer.transform.position - (Vector2)transform.position).normalized;

        // If we're too close, try to maintain attack range
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
    }

    protected void handleAttackState()
    {
        // When attacking, try to position optimally
        Vector2 directionToPlayer = ((Vector2)targetPlayer.transform.position - (Vector2)transform.position).normalized;

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
            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    protected void handleSearchState()
    {
        // Last known position logic
        if (Vector2.Distance(transform.position, targetPosition) > 0.5f)
        {
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            Move(direction * 0.7f); // Move slower when searching
        }
        else
        {
            // At target search position, look around
            transform.Rotate(0, 0, 45 * Time.deltaTime);
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

    private void UpdateEnemyStates()
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;

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
}
