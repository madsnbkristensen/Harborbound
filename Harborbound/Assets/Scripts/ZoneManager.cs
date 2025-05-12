using System.Collections.Generic;
using UnityEngine;

public class ZoneManager : MonoBehaviour
{
    // Singleton instance
    public static ZoneManager Instance { get; private set; }

    public List<Zone> zones = new();

    // This should match the WorldGenerator's islandCenterPosition
    public Vector2 centerPoint = Vector2.zero;

    // Island radius - the inner radius of the first zone should match this
    public float islandRadius = 20f;

    public float baseZoneWidth = 30f;
    public float radiusMultiplier = 1.5f;

    public float islandBuffer = 2; // Buffer distance for overlap with island

    // Reference to the boundary collider object
    private GameObject boundaryCollider;

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicates
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize(Vector2 islandCenter, float islandSize)
    {
        centerPoint = islandCenter;
        islandRadius = islandSize / 2;

        Debug.Log($"ZoneManager initialized with center: {centerPoint}, islandRadius: {islandRadius}");
    }

    public void GenerateZone(int layerCount)
    {
        // Create a new GameObject for the zone
        GameObject zoneObject = new GameObject($"Zone_{layerCount}");
        zoneObject.transform.SetParent(transform);

        // Add Zone component
        Zone zone = zoneObject.AddComponent<Zone>();

        // Set basic zone properties
        zone.id = layerCount;
        zone.zoneName = $"Zone {layerCount}";

        // For the first zone, inner radius should match exactly the island radius
        if (layerCount == 1)
        {
            // No buffer applied here - keep logical separation
            zone.innerRadius = islandRadius;
        }
        else if (zones.Count > 0 && layerCount > 1)
        {
            // For subsequent zones, use the previous zone's outer radius
            zone.innerRadius = zones[layerCount - 2].outerRadius;
        }

        // Calculate outer radius based on inner radius and width
        float zoneWidth = baseZoneWidth * Mathf.Pow(radiusMultiplier, layerCount - 1);
        zone.outerRadius = zone.innerRadius + zoneWidth;

        // Increase difficulty based on layer count
        zone.difficulty = layerCount;

        // Adjust spawn rates based on difficulty
        zone.fishSpawnRate = 1f + (layerCount * 0.2f);
        zone.enemySpawnRate = 0.5f + (layerCount * 0.3f);

        // Add the zone to the list
        zones.Add(zone);

        Debug.Log($"Generated Zone {layerCount} with inner radius {zone.innerRadius} and outer radius {zone.outerRadius}");
    }

    // Add this method to create the boundary
    public void CreateBoundaryCollider()
    {
        // Remove existing boundary if any
        if (boundaryCollider != null)
        {
            DestroyImmediate(boundaryCollider);
        }

        // Get the outermost zone
        if (zones.Count == 0)
        {
            Debug.LogError("Cannot create boundary: no zones exist.");
            return;
        }

        Zone outermostZone = zones[zones.Count - 1];
        float boundaryRadius = outermostZone.outerRadius;

        // Create a new GameObject for the boundary
        boundaryCollider = new GameObject("ZoneBoundary");
        boundaryCollider.transform.SetParent(transform);
        boundaryCollider.transform.position = new Vector3(centerPoint.x, centerPoint.y, 0);

        // Add a circle collider
        CircleCollider2D collider = boundaryCollider.AddComponent<CircleCollider2D>();
        collider.radius = boundaryRadius;

        // Set it as a trigger if you just want to detect when player tries to leave
        // collider.isTrigger = true;

        // Add a rigidbody to ensure collisions work properly
        Rigidbody2D rb = boundaryCollider.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        Debug.Log($"Created boundary collider with radius {boundaryRadius}");
    }

    // Add this method to update the boundary if zones change
    public void UpdateBoundary()
    {
        if (boundaryCollider != null && zones.Count > 0)
        {
            CircleCollider2D collider = boundaryCollider.GetComponent<CircleCollider2D>();
            if (collider != null)
            {
                collider.radius = zones[zones.Count - 1].outerRadius;
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Draw island boundary
        Gizmos.color = Color.green;
        DrawGizmoCircle(centerPoint, islandRadius);

        // Draw each zone
        foreach (Zone zone in zones)
        {
            // Different color for each zone
            Gizmos.color = new Color(0.3f, 0.7f, 0.9f, 0.3f);
            DrawGizmoCircle(centerPoint, zone.innerRadius);

            Gizmos.color = new Color(0.8f, 0.4f, 0.3f, 0.3f);
            DrawGizmoCircle(centerPoint, zone.outerRadius);
        }
    }

    private void DrawGizmoCircle(Vector2 center, float radius)
    {
        int segments = 32;
        Vector3 prevPos = new Vector3(center.x + radius, center.y, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            Vector3 pos = new Vector3(
                center.x + Mathf.Cos(angle) * radius,
                center.y + Mathf.Sin(angle) * radius,
                0
            );
            Gizmos.DrawLine(prevPos, pos);
            prevPos = pos;
        }
    }
}
