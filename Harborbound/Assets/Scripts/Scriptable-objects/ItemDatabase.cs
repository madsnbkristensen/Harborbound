using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    // This list will be populated by the importer
    public List<ItemDefinition> allItems = new List<ItemDefinition>();

    // Cached dictionaries for faster item lookup
    private Dictionary<ItemDefinition.ItemType, List<ItemDefinition>> itemsByType;
    private Dictionary<int, List<ItemDefinition>> fishByZone;
    private Dictionary<int, ItemDefinition> itemsById;

    // Called when the scriptable object is loaded or values change in the inspector
    private void OnEnable()
    {
        BuildCaches();
    }

    // Builds lookup caches for faster access
    public void BuildCaches()
    {
        // Reset caches
        itemsByType = new Dictionary<ItemDefinition.ItemType, List<ItemDefinition>>();
        fishByZone = new Dictionary<int, List<ItemDefinition>>();
        itemsById = new Dictionary<int, ItemDefinition>();

        // Index all items by their type
        foreach (ItemDefinition item in allItems)
        {
            // Skip null items (in case of errors)
            if (item == null)
                continue;

            // Add to ID lookup
            if (!itemsById.ContainsKey(item.id))
                itemsById[item.id] = item;
            else
                Debug.LogWarning($"Duplicate item ID found: {item.id} on {item.itemName}!");

            // Add to type lookup
            if (!itemsByType.ContainsKey(item.type))
                itemsByType[item.type] = new List<ItemDefinition>();

            itemsByType[item.type].Add(item);

            // If it's a fish, add it to zone lookup
            if (item.type == ItemDefinition.ItemType.FISH && item.availableZones != null)
            {
                foreach (int zoneId in item.availableZones)
                {
                    if (!fishByZone.ContainsKey(zoneId))
                        fishByZone[zoneId] = new List<ItemDefinition>();

                    fishByZone[zoneId].Add(item);
                }
            }
        }

        // Log results for debugging
        int fishCount = itemsByType.ContainsKey(ItemDefinition.ItemType.FISH)
            ? itemsByType[ItemDefinition.ItemType.FISH].Count : 0;

        Debug.Log($"ItemDatabase initialized with {allItems.Count} items ({fishCount} fish in {fishByZone.Count} zones)");
    }

    // Get an item by its ID
    public ItemDefinition GetItemById(int id)
    {
        // Rebuild caches if needed
        if (itemsById == null)
            BuildCaches();

        if (itemsById.TryGetValue(id, out ItemDefinition item))
            return item;

        return null;
    }

    // Get all items of a specific type
    public List<ItemDefinition> GetItemsByType(ItemDefinition.ItemType type)
    {
        // Rebuild caches if needed
        if (itemsByType == null)
            BuildCaches();

        if (itemsByType.TryGetValue(type, out List<ItemDefinition> items))
            return new List<ItemDefinition>(items); // Return a copy to prevent modifications

        return new List<ItemDefinition>();
    }

    // Get all fish available in a specific zone
    public List<ItemDefinition> GetFishInZone(int zoneId)
    {
        // Rebuild caches if needed
        if (fishByZone == null)
            BuildCaches();

        if (fishByZone.TryGetValue(zoneId, out List<ItemDefinition> fish))
            return new List<ItemDefinition>(fish); // Return a copy to prevent modifications

        return new List<ItemDefinition>();
    }

    // Get a random item of a specific type
    public ItemDefinition GetRandomItemOfType(ItemDefinition.ItemType type)
    {
        List<ItemDefinition> typeItems = GetItemsByType(type);
        if (typeItems.Count == 0)
            return null;

        return typeItems[Random.Range(0, typeItems.Count)];
    }

    // Get a random fish from a specific zone using rarity weighting
    public ItemDefinition GetRandomFishInZone(int zoneId)
    {
        List<ItemDefinition> zonefish = GetFishInZone(zoneId);
        if (zonefish.Count == 0)
            return null;

        // Calculate total weight (inverted rarity - lower rarity = higher chance)
        float totalWeight = 0;
        foreach (ItemDefinition fish in zonefish)
        {
            totalWeight += (100 - fish.rarity);
        }

        // Get random point in total weight
        float randomPoint = Random.Range(0f, totalWeight);

        // Find which fish that corresponds to
        float currentWeight = 0;
        foreach (ItemDefinition fish in zonefish)
        {
            currentWeight += (100 - fish.rarity);
            if (randomPoint <= currentWeight)
                return fish;
        }

        // Fallback (shouldn't reach here)
        return zonefish[0];
    }

    // Validate the database (check for duplicate IDs, etc.)
    public void ValidateDatabase()
    {
        HashSet<int> usedIds = new HashSet<int>();
        List<string> errors = new List<string>();

        foreach (ItemDefinition item in allItems)
        {
            if (item == null)
            {
                errors.Add("Null item found in database");
                continue;
            }

            // Check for duplicate IDs
            if (usedIds.Contains(item.id))
                errors.Add($"Duplicate ID: {item.id} on item {item.itemName}");
            else
                usedIds.Add(item.id);

            // Check for valid icons
            if (item.icon == null)
                errors.Add($"Missing icon for {item.itemName}");

            // If it's a fish, check for zone assignments
            if (item.type == ItemDefinition.ItemType.FISH)
            {
                if (item.availableZones == null || item.availableZones.Length == 0)
                    errors.Add($"Fish {item.itemName} has no assigned zones");
            }
        }

        // Log all errors
        if (errors.Count > 0)
        {
            Debug.LogWarning($"ItemDatabase validation found {errors.Count} issues:");
            foreach (string error in errors)
                Debug.LogWarning(error);
        }
        else
        {
            Debug.Log("ItemDatabase validation passed");
        }
    }
}

#if UNITY_EDITOR
// Only include this code in the Unity Editor
[UnityEditor.CustomEditor(typeof(ItemDatabase))]
public class ItemDatabaseEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ItemDatabase database = (ItemDatabase)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Rebuild Caches"))
        {
            database.BuildCaches();
            UnityEditor.EditorUtility.SetDirty(database);
        }

        if (GUILayout.Button("Validate Database"))
        {
            database.ValidateDatabase();
        }
    }
}
#endif
