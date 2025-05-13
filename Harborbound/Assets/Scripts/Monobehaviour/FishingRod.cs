using UnityEngine;

public class FishingRod : Item
{
    public float castRange = 5f;
    private Transform rodVisualTransform;

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
        Debug.Log($"Using fishing rod {GetName()}");

        // Get the player's equipment component
        PlayerEquipment equipment = player.GetComponent<PlayerEquipment>();

        // This will be the transform we use for the fishing line origin
        Transform lineOrigin = player.transform; // Default to player if nothing else works

        // If we have equipment and a visual, use that
        if (equipment != null && equipment.currentEquippedVisual != null)
        {
            // Use the equipped visual's transform directly
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
        FishingManager fishingManager = Object.FindFirstObjectByType<FishingManager>();
        if (fishingManager != null)
        {
            fishingManager.CastBobber(playerPos, bobberPos, GetPlayerZone(player), lineOrigin);
        } 
        else
        {
            Debug.LogError("No FishingManager found in scene!");
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        // Get mouse position in screen space
        Vector3 mousePos = Input.mousePosition;

        // Convert to world space
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mousePos.z = -mainCamera.transform.position.z;
            return mainCamera.ScreenToWorldPoint(mousePos);
        }

        return Vector3.zero;
    }

    private int GetPlayerZone(Player player)
    {
        // Zone detection code
        return 1;
    }
}
