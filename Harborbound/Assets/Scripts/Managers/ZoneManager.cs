using System.Collections.Generic;
using UnityEngine;

public class ZoneManager : MonoBehaviour
{
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
    }

    public void Initialize(Vector2 islandCenter, float islandSize)
    {
        centerPoint = islandCenter;
        islandRadius = islandSize;

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

    // Add this method to create the boundary with edge collider
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

        // set tag 
        boundaryCollider.tag = "ZoneBoundary";

        // Create multiple box colliders placed around the circumference
        int segments = 180; // Increased from 64 to 180 for better coverage
        float wallThickness = 1f; // Thickness of wall
        float wallHeight = 5f; // Increased from 3f to 5f for better coverage

        // Calculate the arc length for each segment
        float circumference = 2f * Mathf.PI * boundaryRadius;
        float segmentArcLength = circumference / segments;

        // Ensure wall height is sufficient to cover gaps
        if (wallHeight < segmentArcLength * 1.2f)
        {
            wallHeight = segmentArcLength * 1.2f; // Make walls at least 20% wider than segment spacing
        }

        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            float nextAngle = (i + 1) * Mathf.PI * 2f / segments;

            // Position at the boundary radius
            float midAngle = (angle + nextAngle) / 2;
            float x = Mathf.Cos(midAngle) * boundaryRadius;
            float y = Mathf.Sin(midAngle) * boundaryRadius;

            // Create wall segment
            GameObject wallSegment = new GameObject($"WallSegment_{i}");
            wallSegment.transform.SetParent(boundaryCollider.transform);
            wallSegment.transform.position = new Vector3(x, y, 0) + boundaryCollider.transform.position;

            // Rotate to face outward
            wallSegment.transform.up = new Vector3(x, y, 0).normalized;

            // Add box collider
            BoxCollider2D boxCollider = wallSegment.AddComponent<BoxCollider2D>();
            boxCollider.size = new Vector2(wallHeight, wallThickness);
            boxCollider.offset = new Vector2(0, wallThickness / 2); // Offset slightly outward

            // add tag to wall segments
            wallSegment.tag = "ZoneBoundary";
        }

        Debug.Log($"Created boundary with {segments} wall segments at radius {boundaryRadius}");
    }

    // Add this method to update the boundary if zones change
    public void UpdateBoundary()
    {
        if (boundaryCollider != null && zones.Count > 0)
        {
            float boundaryRadius = zones[zones.Count - 1].outerRadius;
            int segments = boundaryCollider.transform.childCount;

            // Calculate the arc length for each segment
            float circumference = 2f * Mathf.PI * boundaryRadius;
            float segmentArcLength = circumference / segments;

            // Determine optimal wall height based on spacing
            float wallHeight = segmentArcLength * 1.2f; // Make walls at least 20% wider than segment spacing
            if (wallHeight < 5f) wallHeight = 5f; // Minimum wall height

            // Update position of each wall segment
            for (int i = 0; i < segments; i++)
            {
                Transform wallSegment = boundaryCollider.transform.GetChild(i);
                BoxCollider2D boxCollider = wallSegment.GetComponent<BoxCollider2D>();

                float angle = i * Mathf.PI * 2f / segments;
                float nextAngle = (i + 1) * Mathf.PI * 2f / segments;

                // Position at the boundary radius
                float midAngle = (angle + nextAngle) / 2;
                float x = Mathf.Cos(midAngle) * boundaryRadius;
                float y = Mathf.Sin(midAngle) * boundaryRadius;

                // Update position
                wallSegment.position = new Vector3(x, y, 0) + boundaryCollider.transform.position;

                // Update rotation to face outward
                wallSegment.up = new Vector3(x, y, 0).normalized;

                // Update collider size if needed
                if (boxCollider != null)
                {
                    boxCollider.size = new Vector2(wallHeight, boxCollider.size.y);
                }
            }
        }
        else
        {
            CreateBoundaryCollider();
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
