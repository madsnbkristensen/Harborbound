using UnityEngine;

// this is a humanoid class, that will be used to create humanoid classes
// such as player, enemy, npc, etc.
public class Humanoid : MonoBehaviour
{
    [Header("Humanoid Properties")]
    public float speed = 5f;
    public int maxHealth = 100;
    public int currentHealth = 100;
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
        if (humanoidName == "Player")
        {
            UIManager.Instance.UpdateHealthDisplay(currentHealth, maxHealth);
            Debug.Log("##################### Player took damage: " + damage);
            AudioManager.Instance.Play(AudioManager.SoundType.Hurt_Player);
            StartCoroutine(FlashRed());
        }
        else if (humanoidName == "Pirate")
        {
            AudioManager.Instance.Play(AudioManager.SoundType.Hurt_Pirate);
        }
    }

    protected virtual void Die()
    {
        // Override in derived classes for specific death behavior
        if (humanoidName == "Player")
        {
            GameManager.Instance.OnPlayerDeath();
        }
        Destroy(gameObject);
        Debug.Log($"{humanoidName} has died.");
    }

    protected System.Collections.IEnumerator FlashRed()
    {
        // First try to find a child specifically named "sprite"
        Transform spriteTransform = transform.Find("sprite");
        SpriteRenderer spriteRenderer = null;

        if (spriteTransform != null)
        {
            spriteRenderer = spriteTransform.GetComponent<SpriteRenderer>();
        }

        // If we didn't find it that way, fall back to the generic method
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
        else
        {
            Debug.LogWarning(
                $"{gameObject.name} tried to flash red but couldn't find a SpriteRenderer"
            );
        }
    }
}
