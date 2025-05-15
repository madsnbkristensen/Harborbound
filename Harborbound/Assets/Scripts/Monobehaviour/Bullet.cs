using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    public float maxLifetime = 2f;
    public GameObject shooter; // Only keep the reference to who shot it

    private float timer;
    private Weapon sourceWeapon; // Reference to the weapon that fired this bullet

    void OnEnable()
    {
        timer = 0f;
    }

    // Update this method to use the shooter GameObject directly
    public void Initialize(Weapon weapon, GameObject shooter = null)
    {
        // Store reference to the source weapon for damage calculations
        sourceWeapon = weapon;

        // If no shooter specified, use the weapon's root
        this.shooter = shooter ?? weapon.transform.root.gameObject;

        // Set speed based on weapon properties
        speed = weapon.range / weapon.bulletTravelTime;
    }

    void Update()
    {
        // Safety check for invalid speed
        if (float.IsNaN(speed) || float.IsInfinity(speed) || speed <= 0)
        {
            Debug.LogWarning($"Invalid bullet speed: {speed}. Using default.");
            speed = 10f; // Default fallback speed
        }

        // Move forward based on the bullet's rotation
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.Self);

        // Track lifetime
        timer += Time.deltaTime;

        // Return the bullet to the pool if it has exceeded its lifetime
        if (timer >= maxLifetime)
        {
            BulletPool.Instance.ReturnBullet(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Don't hit the shooter
        if (other.gameObject == shooter)
            return;

        // Damage logic for different entity types
        if (other.CompareTag("Enemy") && shooter.CompareTag("Player"))
        {
            // Player bullet hit enemy
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && sourceWeapon != null)
            {
                enemy.TakeDamage(sourceWeapon.damage);
                BulletPool.Instance.ReturnBullet(gameObject);
            }
        }
        else if (other.CompareTag("Player") && shooter.CompareTag("Enemy"))
        {
            // Enemy bullet hit player
            Player player = other.GetComponent<Player>();
            if (player != null && sourceWeapon != null)
            {
                player.TakeDamage(sourceWeapon.damage);
                BulletPool.Instance.ReturnBullet(gameObject);
            }
        }
        else if (other.CompareTag("Obstacle") || other.CompareTag("Wall"))
        {
            // Hit environment
            BulletPool.Instance.ReturnBullet(gameObject);
        }
    }
}

