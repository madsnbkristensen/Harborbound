using UnityEngine;

// Helper class to create items of the right type
public static class ItemFactory
{
    public static Item CreateItem(ItemDefinition definition)
    {
        if (definition == null)
        {
            Debug.LogError("Cannot create item: ItemDefinition is null");
            return null;
        }

        // Create a game object for the item
        GameObject itemObj = new GameObject($"Item_{definition.itemName}");

        // Then add specialized component based on type
        Item item = null;

        itemObj.AddComponent<RectTransform>();

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
                // Default simple item
                item = itemObj.AddComponent<Item>();
                break;
        }

        // NOW set the definition AFTER component has been added
        // This is the key fix - we set the definition after the component is created
        if (item != null)
        {
            item.definition = definition;
            // Debug.Log($"Setting definition {definition.itemName} on {item.GetType().Name}");

            // And now refresh from definition
            item.RefreshFromDefinition();
        }

        return item;
    }

}
