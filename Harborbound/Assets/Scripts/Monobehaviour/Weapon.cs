using UnityEngine;

// Weapon class
public class Weapon : Item
{
    // These properties can be initialized from the ItemDefinition
    public float damage;
    public float range;
    public float fireRate;
    public float spread;
    public float bulletTravelTime;

    private void Awake()
    {
        // Initialize properties from definition when the component wakes up
        if (definition != null && definition.type == ItemDefinition.ItemType.WEAPON)
        {
            damage = definition.damage;
            range = definition.range;
            fireRate = definition.attackSpeed;
            // Other properties from definition
        }
    }

    public override void Use(Player player)
    {
        Debug.Log($"Using weapon {GetName()} with damage {damage}");

        // Find nearest enemy in range
        Enemy nearestEnemy = FindNearestEnemyInRange(player.transform.position, range);

        if (nearestEnemy != null)
        {
            // Apply damage
            int finalDamage = Mathf.RoundToInt(damage);
            nearestEnemy.TakeDamage(finalDamage);
            Debug.Log($"Hit {nearestEnemy.humanoidName} for {finalDamage} damage!");
        }
        else
        {
            Debug.Log("No enemies in range!");
        }
    }

    private Enemy FindNearestEnemyInRange(Vector3 position, float maxRange)
    {
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        Enemy nearestEnemy = null;
        float nearestDistance = maxRange;

        foreach (Enemy enemy in enemies)
        {
            float distance = Vector3.Distance(position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }
}
