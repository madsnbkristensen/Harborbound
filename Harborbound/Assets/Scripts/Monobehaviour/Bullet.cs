using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    public float maxLifetime = 2f;
    public GameObject shooter; // Only keep the reference to who shot it
    public float damage;
    private float creationTime;
    private float lifetime;

    private Weapon sourceWeapon; // Reference to the weapon that fired this bullet

    void OnEnable()
    {
        // Instead, reset creationTime to track the bullet's lifetime
        creationTime = Time.time;
    }

    // Update this method to use the shooter GameObject directly
    public void Initialize(Weapon weapon, GameObject shooter)
    {
        this.sourceWeapon = weapon;
        this.shooter = shooter;

        // Get damage from weapon
        damage = weapon.damage;

        // Calculate speed based on bullet travel time instead of range
        speed = weapon.GetBulletSpeed();

        // Store creation time to track lifetime
        creationTime = Time.time;

        // Set bullet lifetime to the weapon's bulletTravelTime
        // Use a default if not specified
        lifetime = weapon.bulletTravelTime > 0 ? weapon.bulletTravelTime : 1f;
    }

    void Update()
    {
        // Move the bullet
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        // Check if bullet has exceeded its lifetime
        if (Time.time - creationTime >= lifetime)
        {
            // Return to pool
            BulletPool.Instance.ReturnBullet(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Don't hit the shooter
        if (other.gameObject == shooter)
            return;

        // Ignore zone boundaries
        if (other.gameObject.name.Contains("ZoneBoundary") || other.gameObject.CompareTag("Boundary"))
            return;

        // Ignore other bullets
        if (other.CompareTag("Bullet") || other.GetComponent<Bullet>() != null)
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
        else if (other.CompareTag("Player") && shooter.CompareTag("Player"))
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
        else if (other.CompareTag("Enemy") && shooter.CompareTag("Enemy"))
        {
            // Enemy bullet hit another enemy - do nothing, let bullet pass through
            // This allows enemies to shoot without hitting each other
            return;
        }
        else if (other.CompareTag("Obstacle") || other.CompareTag("Wall"))
        {
            // Hit environment
            BulletPool.Instance.ReturnBullet(gameObject);
        }
        else
        {
            BulletPool.Instance.ReturnBullet(gameObject);
        }
    }
}
