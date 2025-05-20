using UnityEngine;

public class Enemy : Humanoid
{
    public enum state
    {
        PATROLLING,
        ATTACKING,
        CHASING,
        SEARCHING,
    }

    public enum type
    {
        PIRATE,
        SHARK,
    }

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

    [Header("Debug")]
    public bool showDebugColliders = false;

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
        if (targetPlayer == null)
            return;

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

        if (equippedWeapon != null)
        {
            UpdateWeaponAim();
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
        if (equippedWeapon == null || targetPlayer == null)
            return;

        // Face the target
        FaceTarget(targetPlayer.transform);

        // Check if in range to fire
        float distanceToTarget = Vector2.Distance(
            transform.position,
            targetPlayer.transform.position
        );
        if (distanceToTarget <= equippedWeapon.range)
        {
            // Fire at the player using the target-specific Fire method
            equippedWeapon.Fire(targetPlayer.transform.position, gameObject);
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
        if (target == null)
            return;

        Vector2 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Get sprite renderer
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Flip the sprite horizontally based on direction
            if (Mathf.Abs(angle) > 90)
            {
                // Facing left
                spriteRenderer.flipX = true;
            }
            else
            {
                // Facing right
                spriteRenderer.flipX = false;
            }

            // Optionally, apply limited vertical tilt
            transform.rotation = Quaternion.Euler(
                0,
                0,
                Mathf.Clamp(direction.normalized.y * 15f, -15f, 15f)
            );
        }
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

    // Add this method to your Enemy class
    public void UpdateWeaponAim()
    {
        if (equippedWeapon == null || targetPlayer == null)
            return;

        // Calculate direction to player
        Vector3 directionToPlayer = (
            targetPlayer.transform.position - transform.position
        ).normalized;

        // Get the angle in degrees
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

        // Check if aiming left
        bool isAimingLeft = (angle > 90 || angle < -90);

        // Apply the rotation to the weapon mount point, with adjustment for left-facing weapons
        if (weaponMountPoint != null)
        {
            if (isAimingLeft)
            {
                // Add 180 degrees to make weapon point correctly when flipped
                weaponMountPoint.rotation = Quaternion.Euler(0, 0, angle + 180);
            }
            else
            {
                weaponMountPoint.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        // Update the visual representation
        Transform weaponVisual = weaponMountPoint
            .GetComponentInChildren<ItemEquipVisual>()
            ?.transform;
        if (weaponVisual != null)
        {
            // Get the ItemEquipVisual component to adjust position offsets
            ItemEquipVisual itemEquipVisual = weaponVisual.GetComponent<ItemEquipVisual>();
            if (itemEquipVisual != null)
            {
                // Adjust position offset based on aiming direction
                if (isAimingLeft)
                {
                    // Adjust position for left-facing weapons
                    itemEquipVisual.positionOffset = new Vector3(-0.2f, 0, 0);
                }
                else
                {
                    // Restore normal position for right-facing weapons
                    itemEquipVisual.positionOffset = new Vector3(0.2f, 0, 0);
                }

                // Apply the position offset
                weaponVisual.localPosition = itemEquipVisual.positionOffset;
            }

            SpriteRenderer weaponSprite = weaponVisual.GetComponent<SpriteRenderer>();
            if (weaponSprite != null)
            {
                // No need to flip the sprite since we're rotating properly
                weaponSprite.flipX = false;

                // Keep scale values positive
                Vector3 currentScale = weaponVisual.localScale;
                weaponVisual.localScale = new Vector3(
                    Mathf.Abs(currentScale.x),
                    Mathf.Abs(currentScale.y),
                    currentScale.z
                );
            }
            else
            {
                Debug.LogWarning("No SpriteRenderer found on weapon visual");
            }
        }
    }

    // Add this method to visualize colliders
    private void OnDrawGizmos()
    {
        if (showDebugColliders)
        {
            Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
            Gizmos.color = Color.red;

            foreach (Collider2D collider in colliders)
            {
                if (collider is BoxCollider2D boxCollider)
                {
                    // Draw box outline where the collider is
                    Vector3 pos = boxCollider.transform.position;
                    Vector3 size = boxCollider.size;
                    Vector3 offset = boxCollider.offset;

                    // Apply local offset to position
                    pos += boxCollider.transform.TransformDirection(new Vector3(offset.x, offset.y, 0));

                    // Draw wireframe box
                    Gizmos.DrawWireCube(pos, new Vector3(size.x, size.y, 0.1f));
                }
                // Add support for other collider types if needed
            }
        }
    }

    public override void TakeDamage(int damage)
    {
        // Add debug to verify method is called and damage amount
        Debug.Log($"Enemy {humanoidName} taking {damage} damage. Health before: {currentHealth}");

        // Call the base implementation in Humanoid
        base.TakeDamage(damage);

        // Add debug to check health after damage
        Debug.Log($"Enemy health after: {currentHealth}");
    }

    public System.Collections.IEnumerator FlashRed()
    {
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }

    protected override void Die()
    {
        Debug.Log($"Enemy {humanoidName} has died!");

        // Drop loot or play death animation here if needed

        // Notify the parent boat if there is one
        if (parentBoat != null)
        {
            parentBoat.EnemyKilled(this);
        }

        // Destroy the enemy GameObject
        Destroy(gameObject);
    }
}
