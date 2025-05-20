using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

public class WorldGenerator : MonoBehaviour
{
    // Singleton instance
    public static WorldGenerator Instance { get; private set; }

    public int worldZoneCount = 4;
    public GameObject[] rockPrefabs;
    public List<Vector2> rockPositions = new();

    public int rockCount = 200;
    public Vector2 islandCenterPosition = new(0, 0);

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

    void Start()
    {
        GenerateWorld();
    }

    public void GenerateWorld()
    {
        // cleanup corpses
        CleanupCorpses();

        // Initialize ZoneManager with island center and size
        if (ZoneManager.Instance != null)
        {
            ZoneManager.Instance.Initialize(islandCenterPosition, 20f); // Pass island radius here
        }

        // Step 1: Generate world zones
        for (int i = 0; i < worldZoneCount; i++)
        {
            if (ZoneManager.Instance != null)
            {
                ZoneManager.Instance.GenerateZone(i + 1);
            }
            else
            {
                Debug.LogError("ZoneManager is not assigned.");
                return;
            }
        }

        // Create the boundary collider after all zones are generated
        if (ZoneManager.Instance != null)
        {
            ZoneManager.Instance.CreateBoundaryCollider();
        }

        // Step 2. Generate water tiles to represent the zones
        if (WaterTileManager.Instance != null)
        {
            WaterTileManager.Instance.GenerateWaterTiles();
        }

        // Step 3: Check and generate rock positions if needed
        if (rockPositions == null || rockPositions.Count == 0)
        {
            GenerateNewRockPositions();
        }

        // Step 4: Place rocks
        PlaceRocks();

        // Step 5: Spawn fishing spots
        if (FishingSpotSpawner.Instance != null)
        {
            FishingSpotSpawner.Instance.SpawnFishingSpots();
        }
        else
        {
            Debug.LogWarning("FishingSpotSpawner not found. Fishing spots will not be generated.");
        }

        // Step 6: Spawn enemy boats
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.SpawnEnemies();
        }
        else
        {
            Debug.LogWarning("EnemySpawner not found. Enemies will not be generated.");
        }
    }

    private void GenerateNewRockPositions()
    {
        rockPositions = new List<Vector2>();

        if (ZoneManager.Instance == null || ZoneManager.Instance.zones.Count == 0)
        {
            Debug.LogError(
                "Cannot generate rocks: ZoneManager not initialized or no zones created."
            );
            return;
        }

        int zonesCount = ZoneManager.Instance.zones.Count;

        // Calculate the total area of all zones combined
        float totalArea = 0f;
        float[] zoneAreas = new float[zonesCount];

        for (int i = 0; i < zonesCount; i++)
        {
            Zone zone = ZoneManager.Instance.zones[i];
            // Area of a ring = π(R²-r²) where R is outer radius and r is inner radius
            float zoneArea =
                Mathf.PI * (Mathf.Pow(zone.outerRadius, 2) - Mathf.Pow(zone.innerRadius, 2));
            zoneAreas[i] = zoneArea;
            totalArea += zoneArea;
        }

        // Distribute rocks proportionally based on zone area
        for (int zoneIndex = 0; zoneIndex < zonesCount; zoneIndex++)
        {
            Zone currentZone = ZoneManager.Instance.zones[zoneIndex];

            // Calculate rock count proportional to zone's area
            float zoneProportion = zoneAreas[zoneIndex] / totalArea;
            int zoneRockCount = Mathf.RoundToInt(rockCount * zoneProportion);

            // Ensure at least a few rocks per zone
            zoneRockCount = Mathf.Max(zoneRockCount, 5);

            int zoneAttempts = 0;
            int zoneMaxAttempts = zoneRockCount * 10;
            int zoneRocksPlaced = 0;

            while (zoneRocksPlaced < zoneRockCount && zoneAttempts < zoneMaxAttempts)
            {
                zoneAttempts++;

                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

                // Generate radius between current zone's inner and outer radius
                // Using sqrt for uniform distribution across the area
                float innerSqr = Mathf.Pow(currentZone.innerRadius + 0.5f, 2);
                float outerSqr = Mathf.Pow(currentZone.outerRadius - 0.5f, 2);
                float r = Mathf.Sqrt(Random.Range(innerSqr, outerSqr));

                Vector2 candidate = new Vector2(
                    islandCenterPosition.x + Mathf.Cos(angle) * r,
                    islandCenterPosition.y + Mathf.Sin(angle) * r
                );

                bool tooClose = false;
                foreach (Vector2 existing in rockPositions)
                {
                    if (Vector2.Distance(candidate, existing) < 1.5f) // Minimum spacing
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    rockPositions.Add(candidate);
                    zoneRocksPlaced++;
                }
            }
        }
    }

    private void PlaceRocks()
    {
        foreach (Vector2 position in rockPositions)
        {
            // Select a random prefab from the array
            GameObject selectedPrefab = rockPrefabs[Random.Range(0, rockPrefabs.Length)];

            // Instantiate the selected prefab
            GameObject rock = Instantiate(selectedPrefab, position, Quaternion.identity);
            rock.transform.SetParent(transform);

            // Flip the rock randomly
            bool flipHorizontal = Random.value > 0.5f;
            rock.transform.localScale = new Vector2(
                flipHorizontal ? -1 : 1, // Flip X scale to flip horizontally
                1 // Keep Y scale the same
            );

            // set scale of rock to random value between 1 and 1.5
            float randomScale = Random.Range(1f, 1.5f);
            rock.transform.localScale = new Vector2(
                rock.transform.localScale.x * randomScale,
                rock.transform.localScale.y * randomScale
            );
        }
    }

    // Update the rock check method to allow for a custom distance check
    public bool CheckRockPositions(Vector2 position, float minDistance = 1f)
    {
        foreach (Vector2 rockPosition in rockPositions)
        {
            if (Vector2.Distance(position, rockPosition) < minDistance)
            {
                return true; // Rock is in the way
            }
        }
        return false; // No rocks are in the way
    }

    private void CleanupCorpses()
    {
        // Find all objects with the tag "EnemyCorpse"
        GameObject[] corpses = GameObject.FindGameObjectsWithTag("EnemyCorpse");

        Debug.Log($"Cleaning up {corpses.Length} enemy corpses");

        // Destroy each corpse
        foreach (GameObject corpse in corpses)
        {
            Destroy(corpse);
        }
    }
}
