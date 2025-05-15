using UnityEngine;
public class Trash : Item
{
    public string color;

    // Replace your Awake method with RefreshFromDefinition
    public override void RefreshFromDefinition()
    {
        // Call the base implementation first to handle shared properties
        base.RefreshFromDefinition();

        if (definition != null && definition.type == ItemDefinition.ItemType.TRASH)
        {
            // Random color for trash (just for visual variety)
            string[] colors = { "red", "blue", "green", "yellow", "black", "white" };
            color = colors[Random.Range(0, colors.Length)];

            Debug.Log($"Trash properties refreshed from definition: {definition.itemName} with color={color}");
        }
    }

    public override void Use(Player player)
    {
        Debug.Log($"Used trash item {GetName()} - not much happens");
        // Maybe small chance of finding something valuable in the trash?
    }
}
