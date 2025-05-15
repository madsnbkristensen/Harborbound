using UnityEngine;

// Helper class to create items of the right type
public static class ItemFactory
{
    public static Item CreateItem(ItemDefinition definition)
    {
        if (definition == null)
            return null;

        GameObject itemObj = new GameObject($"Item_{definition.itemName}");
        Item item = null;

        // Create the right component type based on item definition
        switch (definition.type)
        {
            case ItemDefinition.ItemType.WEAPON:
                item = itemObj.AddComponent<Weapon>();
                break;

            case ItemDefinition.ItemType.FISHING_ROD:
                item = itemObj.AddComponent<FishingRod>();
                break;

            case ItemDefinition.ItemType.FISH:
                item = itemObj.AddComponent<Fish>();
                break;

            case ItemDefinition.ItemType.TRASH:
                item = itemObj.AddComponent<Trash>();
                break;

            default:
                item = itemObj.AddComponent<Item>();
                break;
        }

        // Assign the definition
        if (item != null)
        {
            item.definition = definition;
        }

        return item;
    }
}
