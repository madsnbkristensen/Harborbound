using System.Collections.Generic;
using UnityEngine;

public class ZoneManager : MonoBehaviour
{
    // Singleton instance
    public static ZoneManager Instance { get; private set; }

    public List<Zone> zones = new();
    public Vector2 centerPoint;

    public float baseZoneWidth = 10f;
    public float radiusMultiplier = 1.5f;

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicates
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: keeps the object across scene loads
    }

    public void GenerateZone(int layerCount)
    {
        // Create a new GameObject for the zone
        GameObject zoneObject = new GameObject($"Zone_{layerCount}");
        zoneObject.transform.SetParent(transform);

        // Add Zone component
        Zone zone = zoneObject.AddComponent<Zone>();

        // Set basic zone properties based on layer count
        zone.id = layerCount;
        zone.zoneName = $"Zone {layerCount}";

        // Scale inner and outer radius based on layer count
        // Using the default values from Zone class multiplied by layer count
        zone.innerRadius = zone.innerRadius * Mathf.Pow(radiusMultiplier, layerCount - 1);
        zone.outerRadius = zone.outerRadius * Mathf.Pow(radiusMultiplier, layerCount - 1);

        // Increase difficulty based on layer count
        zone.difficulty = layerCount;

        // Set different colors for each zone
        float hue = (float)layerCount / 10f; // Cycle through colors
        zone.color = Color.HSVToRGB(hue % 1f, 0.7f, 0.9f);

        // Adjust spawn rates based on difficulty
        zone.fishSpawnRate = 1f + (layerCount * 0.2f);
        zone.enemySpawnRate = 0.5f + (layerCount * 0.3f);

        // Add the zone to the list
        zones.Add(zone);

        Debug.Log($"Generated Zone {layerCount} with inner radius {zone.innerRadius} and outer radius {zone.outerRadius}");
    }
}
