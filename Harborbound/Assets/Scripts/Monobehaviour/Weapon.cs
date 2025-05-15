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
        FireWeapon(player?.gameObject);
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
            bulletObj.transform.rotation = firePoint.rotation;
        }
        else
        {
            bulletObj.transform.position = transform.position;
            bulletObj.transform.rotation = transform.rotation;
        }

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

            Debug.Log($"Weapon properties refreshed from definition: {definition.itemName} with attackSpeed={definition.attackSpeed}");
        }
    }
}
