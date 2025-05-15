using UnityEngine;

public class FishingRod : Item
{
    public override void Use(Player player)
    {
        Debug.Log($"Using fishing rod {GetName()}");

        // Determine current zone
        int currentZone = GetPlayerZone(player);

        // Find fishing manager and start fishing
        FishingManager fishingManager = Object.FindFirstObjectByType<FishingManager>();
        if (fishingManager != null)
        {
            fishingManager.StartFishing(currentZone);
        }
        else
        {
            Debug.LogError("No FishingManager found in scene!");
        }
    }

    private int GetPlayerZone(Player player)
    {
        // Simple implementation - can be enhanced
        ZoneManager zoneManager = Object.FindFirstObjectByType<ZoneManager>();
        if (zoneManager != null)
        {
            // Check which zone contains player
            // For now, return a default
        }

        return 1; // Default to zone 1
    }
}
