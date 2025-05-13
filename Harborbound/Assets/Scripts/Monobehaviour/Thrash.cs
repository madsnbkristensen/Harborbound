using UnityEngine;
public class Trash : Item
{
    public string color;

    private void Awake()
    {
        if (definition != null && definition.type == ItemDefinition.ItemType.TRASH)
        {
            // Random color for trash (just for visual variety)
            string[] colors = { "red", "blue", "green", "yellow", "black", "white" };
            color = colors[Random.Range(0, colors.Length)];
        }
    }

    public override void Use(Player player)
    {
        Debug.Log($"Used trash item {GetName()} - not much happens");
        // Maybe small chance of finding something valuable in the trash?
    }
}
