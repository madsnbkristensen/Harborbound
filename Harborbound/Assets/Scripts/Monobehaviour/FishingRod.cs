using UnityEngine;

public class FishingRod : Item
{
    public float castRange = 5f;
    private Transform rodVisualTransform;

    public ZoneManager zoneManager;

    private void Awake()
    {
      zoneManager = Object.FindFirstObjectByType<ZoneManager>();
      if (zoneManager == null)
      {
          Debug.LogError("FishingRod: ZoneManager not found! Make sure it is present in the scene.");
      }
    }
    private void Start()
    {
        // Try to find the visual representation of the rod
        FindRodVisual();
    }

    // Find the rod visual either from player equipment or nearby
    private void FindRodVisual()
    {
        // Try to find from PlayerEquipment
        Player player = Object.FindFirstObjectByType<Player>();
        if (player != null)
        {
            // Check if player has a PlayerEquipment component
            PlayerEquipment equipment = player.GetComponent<PlayerEquipment>();
            if (equipment != null)
            {
                // Get the currently equipped visual (based on your code structure)
                if (equipment.currentEquippedVisual != null)
                {
                    rodVisualTransform = equipment.currentEquippedVisual.transform;
                    return;
                }

                // Fallback to fishing rod attach point if no visual
                if (equipment.fishingRodAttachPoint != null)
                {
                    rodVisualTransform = equipment.fishingRodAttachPoint;
                    return;
                }
            }
        }

        // If all else fails, use the player transform
        // if (player != null)
        // {
        //
        //     rodVisualTransform = player.transform;
        // }
    }

    // In your FishingRod.cs
    public override void Use(Player player)
    {
        // Check if we can cast again (using the cooldown)
        FishingManager fishingManager = Object.FindFirstObjectByType<FishingManager>();

        if (fishingManager != null && !fishingManager.CanCastAgain())
        {
            // Still in cooldown, don't cast
            return;
        }

        Debug.Log($"Using fishing rod {GetName()}");
        AudioManager.Instance.Play(AudioManager.SoundType.Cast);

        // Get the player's equipment component
        PlayerEquipment equipment = player.GetComponent<PlayerEquipment>();

        // This will be the transform we use for the fishing line origin
        Transform lineOrigin = player.transform;

        // If we have equipment and a visual, use that
        if (equipment != null && equipment.currentEquippedVisual != null)
        {
            lineOrigin = equipment.currentEquippedVisual.transform;
        }

        // Get mouse position and do casting logic
        Vector3 mousePos = GetMouseWorldPosition();
        Vector3 playerPos = player.transform.position;
        Vector3 direction = (mousePos - playerPos).normalized;

        // Limit cast distance
        float distanceToMouse = Vector3.Distance(playerPos, mousePos);
        float castDistance = Mathf.Min(distanceToMouse, castRange);

        // Calculate final bobber position
        Vector3 bobberPos = playerPos + (direction * castDistance);

        // Get fishing manager and cast
        if (fishingManager != null)
        {
            // Get the zone at the bobber's destination for fallback
            int targetZone = GetZoneAtPosition(bobberPos);
            fishingManager.CastBobber(playerPos, bobberPos, targetZone, lineOrigin);
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

    // Add this method to your FishingRod class
    public override void RefreshFromDefinition()
    {
        // Call the base implementation first to handle shared properties
        base.RefreshFromDefinition();

        if (definition != null && definition.type == ItemDefinition.ItemType.FISHING_ROD)
        {

            Debug.Log($"FishingRod properties refreshed from definition: {definition.itemName}");
        }
    }
    private Vector3 GetMouseWorldPosition()
    {
        // Existing mouse position code...
        Vector3 mousePos = Input.mousePosition;

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mousePos.z = -mainCamera.transform.position.z;
            return mainCamera.ScreenToWorldPoint(mousePos);
        }

        return Vector3.zero;
    }

    // New method to get zone at a position
    private int GetZoneAtPosition(Vector3 position)
    {
        if (zoneManager == null || zoneManager.zones.Count == 0)
            return 1; // Default to zone 1

        Vector2 centerPoint = zoneManager.centerPoint;
        float distance = Vector2.Distance(position, centerPoint);

        // Check each zone
        for (int i = 0; i < zoneManager.zones.Count; i++)
        {
            Zone zone = zoneManager.zones[i];
            if (distance >= zone.innerRadius && distance <= zone.outerRadius)
            {
                return i + 1; // Zones are 1-indexed
            }
        }

        // If not in any zone (should not happen with proper boundary collider)
        return 1;
    }
}
