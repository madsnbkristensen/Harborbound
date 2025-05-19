using UnityEngine;

public class Enemy : Humanoid
{
    public enum state { PATROLLING, ATTACKING, CHASING, SEARCHING }
    public enum type { PIRATE, SHARK }

    [Header("Enemy Properties")]
    public int attackDamage = 10;
    public float attackRange = 2f;
    public float attackSpeed = 2f;
    public float attackCooldown = 1f;
    private float lastAttackTime;

    [SerializeField]
    public state enemyState = state.PATROLLING;
    [SerializeField]
    public type enemyType = type.PIRATE;

    [Header("References")]
    public EnemyBoat parentBoat;
    public Transform weaponMountPoint;
    public Weapon equippedWeapon;
    private Player targetPlayer;
    private float distanceToPlayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        if (humanoidName == null)
            humanoidName = "Enemy";

        // Find player in scene if not assigned
        if (targetPlayer == null)
            targetPlayer = FindFirstObjectByType<Player>();

        // Get parent boat if not assigned
        if (parentBoat == null)
            parentBoat = GetComponentInParent<EnemyBoat>();
    }

    // Update is called once per frame
    void Update()
    {
        if (targetPlayer == null) return;

        // Calculate distance to player
        distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.transform.position);

        // Execute state-specific behavior
        switch (enemyState)
        {
            case state.PATROLLING:
                handlePatrolState();
                break;
            case state.ATTACKING:
                handleAttackState();
                break;
            case state.CHASING:
                handleChaseState();
                break;
            case state.SEARCHING:
                handleSearchState();
                break;
        }
    }
    protected void handlePatrolState()
    {
        // When on a boat, the boat handles movement
        // Enemy just looks around, plays idle animations
    }
    protected void handleChaseState()
    {
        // When on a boat, the boat handles the chasing
        // Enemy prepares weapons, faces player direction
        FaceTarget(targetPlayer.transform);
    }
    protected void handleAttackState()
    {
        if (equippedWeapon == null) return;

        // Face the target
        FaceTarget(targetPlayer.transform);

        // Check if ready to attack
        if (Time.time > lastAttackTime + attackCooldown)
        {
            // Fire weapon
            if (equippedWeapon.Fire(targetPlayer.transform.position))
            {
                lastAttackTime = Time.time;
            }
        }
    }
    protected void handleSearchState()
    {
        // Look around for the player
        // Could rotate in a search pattern
    }

    // Helper method to face the target
    private void FaceTarget(Transform target)
    {
        Vector2 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void SetState(state newState)
    {
        enemyState = newState;
        // Reset any state-specific variables
        switch (newState)
        {
            case state.ATTACKING:
                lastAttackTime = Time.time; // Ready to attack immediately
                break;
        }
    }

    // Public method that the boat can call to check if player is in attack range
    public bool IsPlayerInAttackRange()
    {
        return distanceToPlayer <= attackRange;
    }
}
