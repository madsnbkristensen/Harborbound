using System.Collections.Generic;
using UnityEngine;

public class FishingSpotSpawner : MonoBehaviour
{
    // This script is responsible for spawning fishing spots in the game.
    public GameObject fishingSpotPrefab; // Prefab for the fishing spot

    public int baseFishingSpotsPerZone = 5; // Base number of fishing spots for first zone
    public float spotIncreaseFactor = 1.5f; // How much to increase spots in outer zones
    public float minDistanceBetweenSpots = 5f; // Minimum distance between fishing spots
    public float minDistanceFromRocks = 2f; // Minimum distance from rocks

    private List<Vector2> fishingSpotPositions = new List<Vector2>();

    public WorldGenerator worldGenerator;
    public ZoneManager zoneManager;

    private void Awake()
    {
        worldGenerator = FindFirstObjectByType<WorldGenerator>();
        if (worldGenerator == null)
        {
            Debug.LogException(new System.Exception("FishingSpotSpawner: WorldGenerator not found! Make sure it is present in the scene."));
        }
        zoneManager = FindFirstObjectByType<ZoneManager>();
        if (zoneManager == null)
        {
            Debug.LogException(new System.Exception("FishingSpotSpawner: ZoneManager not found! Make sure it is present in the scene."));
        }
    }

    private void Start()
    {
    }

    public void SpawnFishingSpots()
    {
        if (zoneManager == null || zoneManager.zones.Count == 0)
        {
            Debug.LogError("Cannot spawn fishing spots: ZoneManager not initialized or no zones created.");
            return;
        }

        fishingSpotPositions.Clear();

        int zonesCount = zoneManager.zones.Count;
        Vector2 centerPoint = zoneManager.centerPoint;

        // Calculate the total area of all zones combined (just like in rock generation)
        float totalArea = 0f;
        float[] zoneAreas = new float[zonesCount];

        for (int i = 0; i < zonesCount; i++)
        {
            Zone zone = zoneManager.zones[i];
            float zoneArea = Mathf.PI * (Mathf.Pow(zone.outerRadius, 2) - Mathf.Pow(zone.innerRadius, 2));
            zoneAreas[i] = zoneArea;
            totalArea += zoneArea;
        }

        // Spawn fishing spots in each zone
        for (int zoneIndex = 0; zoneIndex < zonesCount; zoneIndex++)
        {
            Zone currentZone = zoneManager.zones[zoneIndex];

            // Calculate fishing spots for this zone - more in outer zones
            int zoneFishingSpots = Mathf.RoundToInt(baseFishingSpotsPerZone * Mathf.Pow(spotIncreaseFactor, zoneIndex));

            // Also scale with zone area proportion, similar to rocks
            float zoneProportion = zoneAreas[zoneIndex] / totalArea;
            zoneFishingSpots = Mathf.RoundToInt(zoneFishingSpots * zoneProportion * 2); // *2 to have more spots than rocks

            // Ensure minimum number of spots
            zoneFishingSpots = Mathf.Max(zoneFishingSpots, baseFishingSpotsPerZone);

            int attempts = 0;
            int maxAttempts = zoneFishingSpots * 20; // More attempts for better placement
            int spotsPlaced = 0;

            while (spotsPlaced < zoneFishingSpots && attempts < maxAttempts)
            {
                attempts++;

                // Generate random position within the zone
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

                // Use sqrt for uniform distribution across area (same as rocks)
                float innerSqr = Mathf.Pow(currentZone.innerRadius + 0.5f, 2);
                float outerSqr = Mathf.Pow(currentZone.outerRadius - 0.5f, 2);
                float r = Mathf.Sqrt(Random.Range(innerSqr, outerSqr));

                Vector2 candidatePos = new Vector2(
                    centerPoint.x + Mathf.Cos(angle) * r,
                    centerPoint.y + Mathf.Sin(angle) * r
                );

                // Check if too close to other fishing spots
                bool tooCloseToOtherSpots = false;
                foreach (Vector2 existingSpot in fishingSpotPositions)
                {
                    if (Vector2.Distance(candidatePos, existingSpot) < minDistanceBetweenSpots)
                    {
                        tooCloseToOtherSpots = true;
                        break;
                    }
                }

                // Check if too close to rocks
                bool tooCloseToRocks = worldGenerator.CheckRockPositions(candidatePos, minDistanceFromRocks);

                if (!tooCloseToOtherSpots && !tooCloseToRocks)
                {
                    // Position is valid, create fishing spot
                    fishingSpotPositions.Add(candidatePos);
                    GameObject fishingSpotObject = Instantiate(fishingSpotPrefab, candidatePos, Quaternion.identity);
                    fishingSpotObject.transform.SetParent(transform);

                    // Get the FishingSpot component
                    FishingSpot fishingSpot = fishingSpotObject.GetComponent<FishingSpot>();
                    if (fishingSpot != null)
                    {
                        // Preserve the original values from the prefab
                        float originalMinSize = fishingSpot.minSize;
                        float originalMaxSize = fishingSpot.maxSize;
                        int originalMaxFish = fishingSpot.maxNumberOfFish;

                        // Optional - apply a small multiplier for zone progression
                        if (zoneIndex > 0)
                        {
                            float zoneMultiplier = 1f + (zoneIndex * 0.1f); // 10% increase per zone

                            // Scale the size values based on zone
                            fishingSpot.minSize = originalMinSize * zoneMultiplier;
                            fishingSpot.maxSize = originalMaxSize * zoneMultiplier;

                            // Scale the fish count based on zone
                            fishingSpot.maxNumberOfFish = Mathf.RoundToInt(originalMaxFish * zoneMultiplier);

                            // Ensure min fish count always stays at least 1
                            fishingSpot.minNumberOfFish = Mathf.Max(1, Mathf.RoundToInt(fishingSpot.minNumberOfFish * zoneMultiplier));
                        }

                        // Set random number of fish for this spot
                        fishingSpot.SetRandomNumberOfFish();

                        // Adjust the visual size based on fish count
                        fishingSpot.DetermineFishingSpotSize();

                        // Set the fishing spot zone
                        fishingSpot.fishingSpotZone = zoneIndex + 1; // Zones are 1-indexed for easier understanding
                    }

                    spotsPlaced++;
                }
            }

            Debug.Log($"Placed {spotsPlaced} fishing spots in Zone {zoneIndex + 1}");
        }

        Debug.Log($"Total fishing spots placed: {fishingSpotPositions.Count}");
    }
}
