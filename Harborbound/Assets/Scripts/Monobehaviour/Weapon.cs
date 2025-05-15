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

    [SerializeField] private Transform firePoint;
    private float lastFireTime;

    // Add these new fields to your Weapon class
    public enum WeaponType { SINGLE, AUTOMATIC, SHOTGUN, BURST }
    public WeaponType weaponType = WeaponType.SINGLE;
    public int bulletsPerShot = 1;
    public int burstCount = 1;
    public float burstInterval = 0.1f;

    private bool isFiring = false;
    private Coroutine burstCoroutine;

    private void Awake()
    {
        Debug.Log($"Weapon Awake: {name} - definition is {(definition == null ? "NULL" : "assigned")}");

        // Initialize properties from definition when the component wakes up
        if (definition != null && definition.type == ItemDefinition.ItemType.WEAPON)
        {
            damage = definition.damage;
            range = definition.range;
            fireRate = definition.attackSpeed;
            // Other properties from definition
            Debug.Log($"Weapon initialized from definition: {definition.itemName} with attackSpeed={definition.attackSpeed}");
        }
        else
        {
            Debug.LogWarning($"Weapon {name} has no valid weapon definition assigned. Using default values.");
            // Set default values
            damage = 1;
            range = 10f;
            fireRate = 2f;
        }

        // Initialize lastFireTime to allow immediate first shot
        lastFireTime = -999f;
    }

    // the use method is called when the weapon is used, and should be callable from the player and from enemy 
    public override void Use(Player player)
    {
        // This version is called by players via the PlayerEquipment system
        Debug.Log("Weapon used by player: " + (player != null ? player.name : "unknown"));

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
                    burstCoroutine = StartCoroutine(FireBurst(player.gameObject));
                break;

            case WeaponType.SHOTGUN:
                // For shotguns, fire multiple bullets in a spread pattern
                FireShotgun(player.gameObject);
                break;

            case WeaponType.SINGLE:
            default:
                // For single-shot weapons, just fire once
                FireWeapon(player.gameObject);
                break;
        }
    }

    // Add this overload method to allow enemies to use the weapon
    public void Use(GameObject shooter)
    {
        // This version can be called by enemies
        Debug.Log("Weapon used by: " + (shooter != null ? shooter.name : "unknown"));
        FireWeapon(shooter);
    }

    // Add this helper method that handles the common firing logic
    private void FireWeapon(GameObject shooter)
    {
        // Debug the fire rate check
        float cooldownTime = lastFireTime + (1f / fireRate);
        float currentTime = Time.time;
        bool canFire = currentTime > cooldownTime;

        Debug.Log($"Fire check: Time={currentTime:F2}, Last={lastFireTime:F2}, Cooldown={cooldownTime:F2}, FireRate={fireRate}, CanFire={canFire}");

        // Check if enough time has passed since last fire based on fire rate
        if (canFire)
        {
            FireBullet(shooter);
            lastFireTime = Time.time;
        }
        else
        {
            Debug.Log("Cannot fire yet - cooldown not complete");
        }
    }

    // Update FireBullet to accept a shooter parameter
    private void FireBullet(GameObject shooter)
    {
        if (BulletPool.Instance == null)
        {
            Debug.LogError("BulletPool instance not found!");
            return;
        }

        Debug.Log("Firing bullet from " + name);

        // Get a bullet from the pool
        GameObject bulletObj = BulletPool.Instance.GetBullet();
        Debug.Log("Bullet obtained: " + (bulletObj != null));

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

            Debug.Log($"Weapon properties refreshed: {definition.itemName}, Type={weaponType}, BulletsPerShot={bulletsPerShot}");
        }
    }

    // Add this new method to your Weapon class
    private Quaternion GetMouseAimRotation()
    {
        // Get the mouse position in screen space
        Vector3 mousePosition = Input.mousePosition;

        // Convert mouse position to world space
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(
            mousePosition.x,
            mousePosition.y,
            -Camera.main.transform.position.z)); // Set the z to match the distance from the camera

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
                // Get the shooter (player or enemy) - in this case we assume it's our parent transform
                GameObject shooter = transform.root.gameObject;
                FireWeapon(shooter);
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

        // Fire multiple bullets in a spread pattern
        for (int i = 0; i < bulletsPerShot; i++)
        {
            // Modify the spread for each bullet to create a shotgun pattern
            float adjustedSpread = spread;
            if (bulletsPerShot > 1)
            {
                // Create a wider pattern based on bullet index
                float spreadFactor = (i / (float)(bulletsPerShot - 1)) * 2 - 1; // Range: -1 to 1
                adjustedSpread = spread * 0.5f + spreadFactor * spread;
            }

            FireBullet(shooter, adjustedSpread);
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

    // Modify FireBullet to accept an optional spread parameter
    private void FireBullet(GameObject shooter, float? customSpread = null)
    {
        if (BulletPool.Instance == null)
        {
            Debug.LogError("BulletPool instance not found!");
            return;
        }

        Debug.Log("Firing bullet from " + name);

        // Get a bullet from the pool
        GameObject bulletObj = BulletPool.Instance.GetBullet();

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
        float actualSpread = customSpread ?? spread;
        if (actualSpread > 0)
        {
            float randomSpread = Random.Range(-actualSpread, actualSpread);
            bulletObj.transform.Rotate(0, 0, randomSpread);
        }

        // Configure the bullet with a reference to this weapon and the shooter
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Initialize(this, shooter);
        }
    }
}
