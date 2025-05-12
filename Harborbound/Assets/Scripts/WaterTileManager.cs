using System.Collections.Generic;
using UnityEngine;

public class WaterTileManager : MonoBehaviour
{
    public static WaterTileManager Instance { get; private set; }

    // Use a sprite reference instead of a GameObject prefab
    public Sprite waterTileSprite;
    public float tileSize = 2f;
    public Color baseWaterColor = new Color(0.2f, 0.5f, 0.8f);
    public float darknessFactor = 0.15f;

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void GenerateWaterTiles()
    {
        if (ZoneManager.Instance == null || ZoneManager.Instance.zones.Count == 0)
        {
            Debug.LogError("Cannot generate water tiles: ZoneManager not initialized or no zones created.");
            return;
        }

        // Create a parent object for all water tiles
        GameObject waterParent = new GameObject("WaterTiles");
        waterParent.transform.SetParent(transform);

        // Get the outermost zone's radius to determine the water area
        float maxRadius = 0f;
        foreach (Zone zone in ZoneManager.Instance.zones)
        {
            if (zone.outerRadius > maxRadius)
                maxRadius = zone.outerRadius;
        }

        // Calculate how many tiles we need based on the max radius
        int tilesAcross = Mathf.CeilToInt(maxRadius * 2 / tileSize) + 2;  // Add some buffer

        // Create a grid of water tiles
        Vector2 centerPoint = ZoneManager.Instance.centerPoint;
        for (int x = -tilesAcross / 2; x < tilesAcross / 2; x++)
        {
            for (int y = -tilesAcross / 2; y < tilesAcross / 2; y++)
            {
                Vector2 tilePos = new Vector2(x * tileSize, y * tileSize) + centerPoint;
                float distFromCenter = Vector2.Distance(tilePos, centerPoint);

                // Skip tiles that are too close to center (for island)
                if (distFromCenter < 5f)
                    continue;

                // Create empty GameObject for the water tile
                GameObject waterTile = new GameObject($"WaterTile_{x}_{y}");
                waterTile.transform.SetParent(waterParent.transform);
                waterTile.transform.position = new Vector3(tilePos.x, tilePos.y, 0.1f); // Slight z offset

                // Add SpriteRenderer and set the sprite
                SpriteRenderer renderer = waterTile.AddComponent<SpriteRenderer>();
                renderer.sprite = waterTileSprite;
                renderer.sortingOrder = -100; // Make sure water is behind everything

                // Calculate color based on distance from center
                int zoneIndex = GetZoneIndexAtDistance(distFromCenter);
                if (zoneIndex >= 0)
                {
                    float darkness = zoneIndex * darknessFactor;
                    renderer.color = new Color(
                        baseWaterColor.r * (1f - darkness),
                        baseWaterColor.g * (1f - darkness),
                        baseWaterColor.b * (1f - darkness),
                        baseWaterColor.a
                    );
                }

                // Scale sprite to fit tile size
                waterTile.transform.localScale = new Vector3(tileSize, tileSize, 1);
            }
        }

        Debug.Log("Water tiles generated successfully.");
    }

    private int GetZoneIndexAtDistance(float distance)
    {
        for (int i = 0; i < ZoneManager.Instance.zones.Count; i++)
        {
            Zone zone = ZoneManager.Instance.zones[i];
            if (distance >= zone.innerRadius && distance <= zone.outerRadius)
            {
                return i;
            }
        }

        // If we're beyond all zones, return the last zone index
        if (distance > ZoneManager.Instance.zones[ZoneManager.Instance.zones.Count - 1].outerRadius)
        {
            return ZoneManager.Instance.zones.Count - 1;
        }

        // Inside the island or invalid area
        return -1;
    }
}