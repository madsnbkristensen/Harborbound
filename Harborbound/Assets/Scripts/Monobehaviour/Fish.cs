using UnityEngine;
// Fish class
public class Fish : Item
{
    // Fish-specific properties
    public float size;

    private void Awake()
    {
        if (definition != null && definition.type == ItemDefinition.ItemType.FISH)
        {
            // Generate a random size for this fish instance
            size = Random.Range(definition.minSize, definition.maxSize);
        }
    }

    public override int GetValue()
    {
        // Fish value depends on size
        int baseValue = base.GetValue();
        if (definition != null)
        {
            float avgSize = (definition.minSize + definition.maxSize) / 2f;
            float sizeMultiplier = size / avgSize;
            return Mathf.RoundToInt(baseValue * sizeMultiplier);
        }
        return baseValue;
    }

    public override void Use(Player player)
    {
        // Fish might be used for healing or other effects
        Debug.Log($"Used fish {GetName()} of size {size}");

        // Example: Heal player when eating fish
        if (player != null)
        {
            int healAmount = Mathf.RoundToInt(size * 5); // Size affects healing
            player.currentHealth = Mathf.Min(player.maxHealth, player.currentHealth + healAmount);
            Debug.Log($"Healed player for {healAmount} health");
        }
    }
}

