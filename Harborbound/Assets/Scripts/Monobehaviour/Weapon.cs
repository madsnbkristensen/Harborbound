using UnityEngine;

// Weapon class
public class Weapon : Item
{
    // These properties can be initialized from the ItemDefinition
    public int damage;
    public float range;
    public float fireRate;
    public float spread;
    public float bulletTravelTime;

    [SerializeField]
    private Transform firePoint;
    private float lastFireTime;

    // Add this to store the shooter reference
    private GameObject activeShooter;

    // Add this property to expose lastFireTime
    public float LastFireTime
    {
        get { return lastFireTime; }
    }

    // Add these new fields to your Weapon class
    public enum WeaponType
    {
        SINGLE,
        AUTOMATIC,
        SHOTGUN,
        BURST,
    }

    public WeaponType weaponType = WeaponType.SINGLE;
    public int bulletsPerShot = 1;
    public int burstCount = 1;
    public float burstInterval = 0.1f;

    private bool isFiring = false;
    private Coroutine burstCoroutine;

    private void Awake()
    {
        // Initialize lastFireTime to allow immediate first shot
        lastFireTime = -999f;
    }

    // the use method is called when the weapon is used, and should be callable from the player and from enemy
    public override void Use(Player player)
    {
        // This version is called by players via the PlayerEquipment system

        // Store the player as the active shooter for continuous firing
        activeShooter = player.gameObject;

        // Start firing based on weapon type
        switch (weaponType)
        {
            case WeaponType.AUTOMATIC:
                // For automatic weapons, we just set isFiring flag
                isFiring = true;
                break;

            case WeaponType.BURST:
                // For burst weapons, start a burst coroutine
                if (burstCoroutine == null)
                    burstCoroutine = StartCoroutine(FireBurst(activeShooter));
                break;

            case WeaponType.SHOTGUN:
                // For shotguns, fire multiple bullets in a spread pattern
                FireShotgun(activeShooter);
                break;

            case WeaponType.SINGLE:
            default:
                // For single-shot weapons, just fire once
                FireWeapon(activeShooter);
                break;
        }
    }

    // Add this overload method to allow enemies to use the weapon
    public void Use(GameObject shooter)
    {
        // Store the shooter reference
        activeShooter = shooter;

        // This version can be called by enemies
        FireWeapon(activeShooter);
    }

    // Add this helper method that handles the common firing logic
    private void FireWeapon(GameObject shooter)
    {
        // Debug the fire rate check
        float cooldownTime = lastFireTime + (1f / fireRate);
        float currentTime = Time.time;
        bool canFire = currentTime > cooldownTime;

        // Check if enough time has passed since last fire based on fire rate
        if (canFire)
        {
            AudioManager.Instance.Play(AudioManager.SoundType.Shoot);
            FireBullet(shooter);
            lastFireTime = Time.time;
        }
        else
        {
            Debug.Log("Cannot fire yet - cooldown not complete");
        }
    }

    // Update FireBullet to accept a shooter parameter and use weapon-specific bullets
    private void FireBullet(GameObject shooter)
    {
        if (BulletPool.Instance == null)
        {
            Debug.LogError("BulletPool instance not found!");
            return;
        }

        // Get the weapon-specific bullet prefab
        GameObject bulletPrefab = definition?.bulletPrefab;

        // Get a bullet from the pool (will use default if bulletPrefab is null)
        GameObject bulletObj = BulletPool.Instance.GetBullet(bulletPrefab);

        if (bulletObj == null)
        {
            Debug.LogError("Failed to get bullet from pool!");
            return;
        }

        // Position the bullet at the fire point
        if (firePoint != null)
        {
            bulletObj.transform.position = firePoint.position;
        }
        else
        {
            bulletObj.transform.position = transform.position;
        }

        // Get the aim rotation (toward mouse)
        Quaternion aimRotation = GetMouseAimRotation();

        // Set the bullet rotation to aim toward mouse
        bulletObj.transform.rotation = aimRotation;

        // Apply spread if needed
        if (spread > 0)
        {
            float randomSpread = Random.Range(-spread, spread);
            bulletObj.transform.Rotate(0, 0, randomSpread);
        }

        // Configure the bullet with a reference to this weapon and the shooter
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Initialize(this, shooter);
        }
    }

    // Add this method to allow setting the firePoint from outside
    public void SetFirePoint(Transform newFirePoint)
    {
        firePoint = newFirePoint;
    }

    // Override in Weapon class
    public override void RefreshFromDefinition()
    {
        // Call the base implementation first to handle shared properties
        base.RefreshFromDefinition();

        if (definition != null && definition.type == ItemDefinition.ItemType.WEAPON)
        {
            damage = definition.damage;
            range = definition.range;
            fireRate = definition.attackSpeed;
            spread = definition.spread;
            bulletTravelTime = definition.bulletTravelTime;

            // Get special weapon behavior properties
            weaponType = (WeaponType)definition.weaponType;
            bulletsPerShot = definition.bulletsPerShot;
            burstCount = definition.burstCount;
            burstInterval = definition.burstInterval;

            Debug.Log($"Weapon {definition.itemName} configured with bullet: {(definition.bulletPrefab ? definition.bulletPrefab.name : "default")}");
        }
        else
        {
            // Add this else clause to handle missing definition
            Debug.LogWarning(
                $"Weapon {name} has no valid weapon definition assigned during refresh. Using default values."
            );
            // Set default values
            damage = 1;
            range = 10f;
            fireRate = 2f;
        }
    }

    // Add this new method to your Weapon class
    private Quaternion GetMouseAimRotation()
    {
        // Get the mouse position in screen space
        Vector3 mousePosition = Input.mousePosition;

        // Convert mouse position to world space
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(
            new Vector3(mousePosition.x, mousePosition.y, -Camera.main.transform.position.z)
        ); // Set the z to match the distance from the camera

        // Calculate direction from weapon/firePoint to mouse position
        Vector3 aimDirection;
        if (firePoint != null)
        {
            aimDirection = mouseWorldPosition - firePoint.position;
        }
        else
        {
            aimDirection = mouseWorldPosition - transform.position;
        }

        // Remove z component to keep it 2D
        aimDirection.z = 0;

        // Calculate the angle in degrees
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        // Create and return a rotation
        return Quaternion.Euler(0, 0, angle);
    }

    // Add a StopUse method to stop automatic firing
    public void StopUse()
    {
        isFiring = false;

        // Also stop any ongoing burst
        if (burstCoroutine != null)
        {
            StopCoroutine(burstCoroutine);
            burstCoroutine = null;
        }
    }

    // Add an Update method to handle continuous firing for automatic weapons
    private void Update()
    {
        if (isFiring && weaponType == WeaponType.AUTOMATIC)
        {
            // For automatic weapons, call FireWeapon in Update
            // The fire rate check is inside FireWeapon
            if (Time.time > lastFireTime + (1f / fireRate))
            {
                // Use the stored activeShooter instead of trying to find it again
                if (activeShooter != null)
                {
                    FireWeapon(activeShooter);
                }
                else
                {
                    // Only try to find the shooter if we lost the reference somehow
                    Debug.LogWarning("Lost shooter reference in automatic firing!");

                    // First try to find a player component in the hierarchy
                    Player playerComponent = GetComponentInParent<Player>();
                    if (playerComponent != null)
                    {
                        activeShooter = playerComponent.gameObject;
                    }
                    else
                    {
                        // Fallback to root if no player found
                        activeShooter = transform.root.gameObject;
                        Debug.Log($"No player component found, using root: {activeShooter.name} with tag {activeShooter.tag}");
                    }

                    FireWeapon(activeShooter);
                }
            }
        }
    }

    // Implement shotgun firing
    private void FireShotgun(GameObject shooter)
    {
        // Calculate cooldown time
        float cooldownTime = lastFireTime + (1f / fireRate);

        // Check if weapon can fire
        if (Time.time <= cooldownTime)
        {
            Debug.Log("Cannot fire shotgun yet - cooldown not complete");
            return;
        }

        // Get the weapon-specific bullet prefab
        GameObject bulletPrefab = definition?.bulletPrefab;

        // Fire multiple bullets in a spread pattern
        for (int i = 0; i < bulletsPerShot; i++)
        {
            // Get a bullet from the pool
            GameObject bulletObj = BulletPool.Instance.GetBullet(bulletPrefab);
            if (bulletObj == null)
                continue;

            // Position the bullet at the fire point
            bulletObj.transform.position = firePoint != null ? firePoint.position : transform.position;

            // Get the aim rotation (toward mouse)
            Quaternion aimRotation = GetMouseAimRotation();

            // Set the bullet rotation to aim toward mouse
            bulletObj.transform.rotation = aimRotation;

            // Modify the spread for each bullet to create a shotgun pattern
            float adjustedSpread = spread;
            if (bulletsPerShot > 1)
            {
                // Create a wider pattern based on bullet index
                float spreadFactor = (i / (float)(bulletsPerShot - 1)) * 2 - 1; // Range: -1 to 1
                adjustedSpread = spread * 0.5f + spreadFactor * spread;
            }

            // Apply spread
            bulletObj.transform.Rotate(0, 0, adjustedSpread);

            // Configure the bullet
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.Initialize(this, shooter);
            }
        }

        // Update last fire time
        lastFireTime = Time.time;
    }

    // Implement burst firing using a coroutine
    private System.Collections.IEnumerator FireBurst(GameObject shooter)
    {
        // Calculate cooldown time
        float cooldownTime = lastFireTime + (1f / fireRate);

        // Check if weapon can fire
        if (Time.time <= cooldownTime)
        {
            Debug.Log("Cannot fire burst yet - cooldown not complete");
            burstCoroutine = null;
            yield break;
        }

        // Fire a burst of bullets
        for (int i = 0; i < burstCount; i++)
        {
            FireBullet(shooter);

            // Wait for burst interval
            if (i < burstCount - 1)
                yield return new WaitForSeconds(burstInterval);
        }

        // Update last fire time after the full burst
        lastFireTime = Time.time;
        burstCoroutine = null;
    }

    public bool Fire(Vector3 targetPosition, GameObject shooter = null)
    {
        // If no shooter is provided, try to find a player component
        if (shooter == null)
        {
            Player playerComponent = GetComponentInParent<Player>();
            if (playerComponent != null)
            {
                shooter = playerComponent.gameObject;

                // Make sure the player GameObject has the Player tag
                if (!shooter.CompareTag("Player"))
                {
                    Debug.LogWarning("Player GameObject is missing 'Player' tag!");
                }
            }
            else
            {
                // Try to find a player in the scene as fallback
                Player foundPlayer = FindFirstObjectByType<Player>();
                if (foundPlayer != null)
                {
                    shooter = foundPlayer.gameObject;
                }
                else
                {
                    // Last resort fallback
                    shooter = transform.root.gameObject;
                    Debug.LogWarning($"No player found, using {shooter.name} with tag {shooter.tag} as shooter");
                }
            }
        }

        // Calculate cooldown time
        float cooldownTime = lastFireTime + (1f / fireRate);
        bool canFire = Time.time > cooldownTime;

        if (!canFire)
            return false;

        Debug.Log("FAAAKING FIRE!");
        // Play the shooting sound effect
        AudioManager.Instance.Play(AudioManager.SoundType.Shoot);

        // Handle different weapon types
        switch (weaponType)
        {
            case WeaponType.SHOTGUN:
                FireShotgunAt(targetPosition, shooter);
                break;
            case WeaponType.BURST:
                StartCoroutine(FireBurstAt(targetPosition, shooter));
                break;
            case WeaponType.AUTOMATIC:
            case WeaponType.SINGLE:
            default:
                FireBulletAt(targetPosition, shooter);
                break;
        }

        lastFireTime = Time.time;
        return true;
    }

    // Add this helper method to fire at a specific position
    private void FireBulletAt(Vector3 targetPosition, GameObject shooter)
    {
        if (BulletPool.Instance == null)
        {
            Debug.LogError("BulletPool instance not found!");
            return;
        }

        // Get the weapon-specific bullet prefab
        GameObject bulletPrefab = definition?.bulletPrefab;

        // Get a bullet from the pool
        GameObject bulletObj = BulletPool.Instance.GetBullet(bulletPrefab);
        if (bulletObj == null)
            return;

        // Position the bullet at the fire point
        bulletObj.transform.position = firePoint != null ? firePoint.position : transform.position;

        // Calculate direction to the target position
        Vector2 direction = targetPosition - bulletObj.transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Set the bullet rotation to aim toward the target
        bulletObj.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Apply spread if needed
        if (spread > 0)
        {
            float randomSpread = Random.Range(-spread, spread);
            bulletObj.transform.Rotate(0, 0, randomSpread);
        }

        // Configure the bullet with a reference to this weapon and the shooter
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Initialize(this, shooter);
        }
    }

    // Add shotgun firing at a position
    private void FireShotgunAt(Vector3 targetPosition, GameObject shooter)
    {
        // Get the weapon-specific bullet prefab
        GameObject bulletPrefab = definition?.bulletPrefab;

        // Fire multiple bullets in a spread pattern
        for (int i = 0; i < bulletsPerShot; i++)
        {
            // Get a bullet from the pool
            GameObject bulletObj = BulletPool.Instance.GetBullet(bulletPrefab);
            if (bulletObj == null)
                continue;

            // Position the bullet at the fire point
            bulletObj.transform.position =
                firePoint != null ? firePoint.position : transform.position;

            // Calculate direction to the target position
            Vector2 direction = targetPosition - bulletObj.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Set the bullet rotation to aim toward the target
            bulletObj.transform.rotation = Quaternion.Euler(0, 0, angle);

            // Modify the spread for each bullet to create a shotgun pattern
            float adjustedSpread = spread;
            if (bulletsPerShot > 1)
            {
                // Create a wider pattern based on bullet index
                float spreadFactor = (i / (float)(bulletsPerShot - 1)) * 2 - 1; // Range: -1 to 1
                adjustedSpread = spread * 0.5f + spreadFactor * spread;
            }

            // Apply spread
            bulletObj.transform.Rotate(0, 0, adjustedSpread);

            // Configure the bullet
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.Initialize(this, shooter);
            }
        }
    }

    // Implement burst firing at a position
    private System.Collections.IEnumerator FireBurstAt(Vector3 targetPosition, GameObject shooter)
    {
        // Get the weapon-specific bullet prefab
        GameObject bulletPrefab = definition?.bulletPrefab;

        // Fire a burst of bullets
        for (int i = 0; i < burstCount; i++)
        {
            // Get a bullet from the pool
            GameObject bulletObj = BulletPool.Instance.GetBullet(bulletPrefab);
            if (bulletObj == null)
            {
                // Wait for burst interval even if bullet creation failed
                if (i < burstCount - 1)
                    yield return new WaitForSeconds(burstInterval);
                continue;
            }

            // Position the bullet at the fire point
            bulletObj.transform.position = firePoint != null ? firePoint.position : transform.position;

            // Calculate direction to the target position
            Vector2 direction = targetPosition - bulletObj.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Set the bullet rotation to aim toward the target
            bulletObj.transform.rotation = Quaternion.Euler(0, 0, angle);

            // Apply spread if needed
            if (spread > 0)
            {
                float randomSpread = Random.Range(-spread, spread);
                bulletObj.transform.Rotate(0, 0, randomSpread);
            }

            // Configure the bullet
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.Initialize(this, shooter);
            }

            // Wait for burst interval
            if (i < burstCount - 1)
                yield return new WaitForSeconds(burstInterval);
        }
    }

    // Add this method to your Weapon class
    public float GetBulletSpeed()
    {
        // Calculate speed based only on bulletTravelTime
        // Using a default travel time of 1 second if not specified
        float travelTime = bulletTravelTime > 0 ? bulletTravelTime : 1f;

        // Use a fixed standard distance instead of range
        float standardDistance = 10f; // A consistent reference distance

        // This gives us a consistent speed based only on travel time
        return standardDistance / travelTime;
    }
}
