using UnityEngine;

// this is a humanoid class, that will be used to create humanoid classes
// such as player, enemy, npc, etc.
public class Humanoid : MonoBehaviour
{
    [Header("Humanoid Properties")]
    public float speed = 5f;
    public int maxHealth = 100;
    public int currentHealth;
    public string humanoidName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    // simple function to move the humanoid that takes a direction as arg
    protected virtual void Move(Vector2 direction)
    {
        Vector2 newPosition = (Vector2)transform.position + direction * speed * Time.deltaTime;
        transform.position = newPosition;
    }

    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        // Override in derived classes for specific death behavior
        Destroy(gameObject);
        Debug.Log($"{humanoidName} has died.");
    }
}
