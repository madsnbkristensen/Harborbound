using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Random = UnityEngine.Random;
using Quaternion = UnityEngine.Quaternion;

public class WorldGenerator : MonoBehaviour
{
    // Singleton instance
    public static WorldGenerator Instance { get; private set; }

    public int worldZoneCount = 3;
    public GameObject rockPrefab;
    public List<Vector2> rockPositions = new();

    public int rockCount = 50;
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

        // Step 2: Check and generate rock positions if needed
        if (rockPositions == null || rockPositions.Count == 0)
        {
            GenerateNewRockPositions();
        }

        // Step 3: Place rocks
        PlaceRocks();
    }

    private void GenerateNewRockPositions()
    {
        rockPositions = new List<Vector2>();
        int attempts = 0;
        int maxAttempts = rockCount * 10;

        while (rockPositions.Count < rockCount && attempts < maxAttempts)
        {
            attempts++;
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = Random.Range(5f, 20f);
            Vector2 candidate = new Vector2(
                islandCenterPosition.x + Mathf.Cos(angle) * radius,
                islandCenterPosition.y + Mathf.Sin(angle) * radius
            );

            bool tooClose = false;
            foreach (Vector2 existing in rockPositions)
            {
                if (Vector2.Distance(candidate, existing) < 1f) // 1f = minimum spacing
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                rockPositions.Add(candidate);
            }
        }

        if (rockPositions.Count < rockCount)
        {
            Debug.LogWarning("Could not generate all rocks without overlap.");
        }
    }

    private void PlaceRocks()
    {
        foreach (Vector2 position in rockPositions)
        {
            GameObject rock = Instantiate(rockPrefab, position, Quaternion.identity);
            rock.transform.SetParent(transform);
        }
    }

    public bool CheckRockPositions(Vector2 position)
    {
        // This function is to return the rock positions, so that other scripts can use it to determine if a rock is in the way of spawning another object.
        foreach (Vector2 rockPosition in rockPositions)
        {
            if (Vector2.Distance(position, rockPosition) < 1f) // 1f = minimum spacing
            {
                return true; // Rock is in the way
            }
        }
        return false; // No rocks are in the way
    }
}
