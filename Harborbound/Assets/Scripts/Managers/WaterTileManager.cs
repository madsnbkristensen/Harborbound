using UnityEngine;

public class WaterTileManager : MonoBehaviour
{
    // Use a sprite reference instead of a GameObject prefab
    public Sprite waterTileSprite;
    public float tileSize = 2f;
    public Color baseWaterColor = new Color(0.2f, 0.5f, 0.8f);
    public float darknessFactor = 0.15f;

    public ZoneManager zoneManager;

    private void Awake()
    {
        zoneManager = FindFirstObjectByType<ZoneManager>();
        if (zoneManager == null)
        {
            Debug.LogError("WaterTileManager: ZoneManager not found! Make sure it is present in the scene.");
        }
    }

    public void GenerateWaterTiles()
    {
        if (zoneManager == null || zoneManager.zones.Count == 0)
        {
            Debug.LogError("Cannot generate water tiles: ZoneManager not initialized or no zones created.");
            return;
        }

        // Clear any existing water tiles
        Transform existingWaterParent = transform.Find("WaterTiles");
        if (existingWaterParent != null)
        {
            DestroyImmediate(existingWaterParent.gameObject);
        }

        // Create a parent object for all water tiles
        GameObject waterParent = new GameObject("WaterTiles");
        waterParent.transform.SetParent(transform);

        // Get the outermost zone's radius to determine the water area
        float maxRadius = 0f;
        foreach (Zone zone in zoneManager.zones)
        {
            if (zone.outerRadius > maxRadius)
                maxRadius = zone.outerRadius;
        }

        // Add extra buffer for tiles beyond camera view
        float extraBuffer = 15f; // Adjust this value based on camera size
        float extendedRadius = maxRadius + extraBuffer;

        // Calculate how many tiles we need based on the extended radius
        float diameter = extendedRadius * 2f;
        int tilesAcross = Mathf.CeilToInt(diameter / tileSize) + 4; // Add extra buffer

        // Ensure we have an odd number of tiles for perfect centering
        if (tilesAcross % 2 == 0) tilesAcross++;

        // Calculate the offset to center the grid precisely at the island center
        float gridSize = tilesAcross * tileSize;
        float startX = zoneManager.centerPoint.x - (gridSize / 2f);
        float startY = zoneManager.centerPoint.y - (gridSize / 2f);

        // Debug grid size and position
        Debug.Log($"Water grid: {tilesAcross}x{tilesAcross} tiles, size: {gridSize}x{gridSize}, " +
                  $"starting at ({startX}, {startY})");

        // Get color for extended water (same as last zone)
        Color extendedWaterColor = Color.black;
        if (zoneManager.zones.Count > 0)
        {
            int lastZoneIndex = zoneManager.zones.Count - 1;
            float hue = (float)(lastZoneIndex) / 8f;
            Color zoneColor = Color.HSVToRGB(hue % 1f, 0.6f, 0.8f);

            // Apply darkening effect for deep waters
            float darkness = lastZoneIndex * darknessFactor * 1.2f; // Make extended water slightly darker
            extendedWaterColor = new Color(
                zoneColor.r * (1f - darkness),
                zoneColor.g * (1f - darkness),
                zoneColor.b * (1f - darkness),
                0.8f
            );
        }

        for (int x = 0; x < tilesAcross; x++)
        {
            for (int y = 0; y < tilesAcross; y++)
            {
                // Calculate the center of this tile
                float posX = startX + (x * tileSize) + (tileSize / 2f);
                float posY = startY + (y * tileSize) + (tileSize / 2f);
                Vector2 tileCenter = new Vector2(posX, posY);

                // Calculate the distance from the island center to this tile's center
                float distFromCenter = Vector2.Distance(tileCenter, zoneManager.centerPoint);

                // Skip tiles that are well inside the island
                // Apply the visual buffer here for inner radius
                if (distFromCenter < zoneManager.islandRadius - zoneManager.islandBuffer - 0.2f)
                    continue;

                // Use extended radius check instead of maxRadius
                if (distFromCenter > extendedRadius)
                    continue;

                // Create empty GameObject for the water tile
                GameObject waterTile = new GameObject($"WaterTile_{x}_{y}");
                waterTile.transform.SetParent(waterParent.transform);
                waterTile.transform.position = new Vector3(tileCenter.x, tileCenter.y, 0.1f);

                // Add SpriteRenderer and set the sprite
                SpriteRenderer renderer = waterTile.AddComponent<SpriteRenderer>();
                renderer.sprite = waterTileSprite;
                renderer.sortingOrder = -100; // Make sure water is behind everything

                // Set the scale to match tile size exactly
                waterTile.transform.localScale = new Vector3(tileSize / waterTileSprite.bounds.size.x,
                                                            tileSize / waterTileSprite.bounds.size.y, 1);

                // Determine which zone this tile belongs to or use extended water color
                int zoneIndex = GetZoneIndexAtDistance(distFromCenter);
                if (zoneIndex >= 0)
                {
                    // Always use baseWaterColor as the base for all zones
                    Color zoneColor = baseWaterColor;

                    // Calculate darkness based on zone index
                    float darkness;
                    if (zoneIndex == 0)
                    {
                        // First zone - no darkening
                        darkness = 0;
                    }
                    else
                    {
                        // More subtle darkening for deeper waters
                        // Use sqrt to make transitions less dramatic
                        darkness = Mathf.Sqrt(zoneIndex) * darknessFactor * 0.3f;
                    }

                    renderer.color = new Color(
                        zoneColor.r * (1f - darkness),
                        zoneColor.g * (1f - darkness),
                        zoneColor.b * (1f - darkness),
                        0.8f
                    );
                }
                else
                {
                    // Use a darker version of baseWaterColor for extended water
                    float extendedDarkness = 0.4f; // Fixed darkness for extended water
                    renderer.color = new Color(
                        baseWaterColor.r * (1f - extendedDarkness),
                        baseWaterColor.g * (1f - extendedDarkness),
                        baseWaterColor.b * (1f - extendedDarkness),
                        0.8f
                    );
                }
            }
        }

        Debug.Log("Water tiles generated successfully with extended boundary.");
    }

    private int GetZoneIndexAtDistance(float distance)
    {
        // First check if we're in the buffer zone (between island edge and zone 1 start)
        if (distance >= zoneManager.islandRadius - zoneManager.islandBuffer &&
            distance < zoneManager.zones[0].innerRadius)
        {
            return 0; // Treat buffer zone tiles as zone 1 for coloring purposes
        }

        // Regular zone checks
        for (int i = 0; i < zoneManager.zones.Count; i++)
        {
            Zone zone = zoneManager.zones[i];
            if (distance >= zone.innerRadius && distance <= zone.outerRadius)
            {
                return i;
            }
        }

        // Extended water beyond the last zone
        if (distance > zoneManager.zones[zoneManager.zones.Count - 1].outerRadius)
        {
            return zoneManager.zones.Count - 1; // Use last zone color
        }

        return -1; // Inside island or invalid
    }

}
