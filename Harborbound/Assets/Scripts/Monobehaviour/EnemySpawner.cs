using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // Singleton pattern
    public static EnemySpawner Instance { get; private set; }

    [Header("Spawn Settings")]
    public GameObject enemyBoatPrefab;
    public GameObject enemyPiratePrefab;
    public GameObject enemySharkPrefab;

    [Header("Spawn Configuration")]
    public int baseEnemyBoatsPerZone = 2;           // Base number of enemy boats for first zone
    public float boatIncreaseFactor = 1.2f;         // How much to increase boats in outer zones
    public float minDistanceBetweenBoats = 15f;     // Minimum distance between boats
    public float minDistanceFromRocks = 5f;         // Minimum distance from rocks
    public float minDistanceFromIsland = 10f;       // Minimum distance from island center

    [Header("Enemy Configuration")]
    public int minEnemiesPerBoat = 1;
    public int maxEnemiesPerBoat = 3;
    [Range(0f, 1f)]
    public float sharkProbability = 0.3f;           // Probability of spawning a shark instead of pirate

    [Header("Weapon Configuration")]
    public List<Weapon> availableWeapons = new List<Weapon>();

    private List<Vector2> enemyBoatPositions = new List<Vector2>();

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

    public void SpawnEnemyBoats()
    {
        if (ZoneManager.Instance == null || ZoneManager.Instance.zones.Count == 0)
        {
            Debug.LogError("Cannot spawn enemy boats: ZoneManager not initialized or no zones created.");
            return;
        }

        if (enemyBoatPrefab == null || enemyPiratePrefab == null)
        {
            Debug.LogError("Enemy boat or enemy prefabs not assigned.");
            return;
        }

        enemyBoatPositions.Clear();

        int zonesCount = ZoneManager.Instance.zones.Count;
        Vector2 centerPoint = ZoneManager.Instance.centerPoint;

        // Calculate the total area of all zones combined (similar to FishingSpotSpawner)
        float totalArea = 0f;
        float[] zoneAreas = new float[zonesCount];

        for (int i = 0; i < zonesCount; i++)
        {
            Zone zone = ZoneManager.Instance.zones[i];
            float zoneArea = Mathf.PI * (Mathf.Pow(zone.outerRadius, 2) - Mathf.Pow(zone.innerRadius, 2));
            zoneAreas[i] = zoneArea;
            totalArea += zoneArea;
        }

        // Spawn enemy boats in each zone
        for (int zoneIndex = 0; zoneIndex < zonesCount; zoneIndex++)
        {
            Zone currentZone = ZoneManager.Instance.zones[zoneIndex];

            // Skip the first zone (closest to island) to give player a safe starting area
            if (zoneIndex == 0)
                continue;

            // Calculate enemy boats for this zone - more in outer zones
            int zoneEnemyBoats = Mathf.RoundToInt(baseEnemyBoatsPerZone * Mathf.Pow(boatIncreaseFactor, zoneIndex));

            // Scale with zone area proportion
            float zoneProportion = zoneAreas[zoneIndex] / totalArea;
            zoneEnemyBoats = Mathf.RoundToInt(zoneEnemyBoats * zoneProportion * 1.5f);

            // Ensure minimum number of boats in larger zones
            zoneEnemyBoats = Mathf.Max(zoneEnemyBoats, baseEnemyBoatsPerZone);

            int attempts = 0;
            int maxAttempts = zoneEnemyBoats * 20;
            int boatsPlaced = 0;

            while (boatsPlaced < zoneEnemyBoats && attempts < maxAttempts)
            {
                attempts++;

                // Generate random position within the zone
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

                // Use sqrt for uniform distribution across area
                float innerSqr = Mathf.Pow(currentZone.innerRadius + 0.5f, 2);
                float outerSqr = Mathf.Pow(currentZone.outerRadius - 0.5f, 2);
                float r = Mathf.Sqrt(Random.Range(innerSqr, outerSqr));

                Vector2 candidatePos = new Vector2(
                    centerPoint.x + Mathf.Cos(angle) * r,
                    centerPoint.y + Mathf.Sin(angle) * r
                );

                // Check if too close to the island center
                if (Vector2.Distance(candidatePos, centerPoint) < minDistanceFromIsland)
                    continue;

                // Check if too close to other enemy boats
                bool tooCloseToOtherBoats = false;
                foreach (Vector2 existingBoat in enemyBoatPositions)
                {
                    if (Vector2.Distance(candidatePos, existingBoat) < minDistanceBetweenBoats)
                    {
                        tooCloseToOtherBoats = true;
                        break;
                    }
                }

                // Check if too close to rocks
                bool tooCloseToRocks = WorldGenerator.Instance != null &&
                    WorldGenerator.Instance.CheckRockPositions(candidatePos, minDistanceFromRocks);

                if (!tooCloseToOtherBoats && !tooCloseToRocks)
                {
                    // Position is valid, create enemy boat
                    enemyBoatPositions.Add(candidatePos);
                    GameObject enemyBoatObject = Instantiate(enemyBoatPrefab, candidatePos, Quaternion.identity);
                    enemyBoatObject.transform.SetParent(transform);

                    // Get the EnemyBoat component
                    EnemyBoat enemyBoat = enemyBoatObject.GetComponent<EnemyBoat>();
                    if (enemyBoat != null)
                    {
                        // Configure boat based on zone
                        ConfigureEnemyBoat(enemyBoat, zoneIndex + 1);
                    }

                    boatsPlaced++;
                }
            }

            Debug.Log($"Placed {boatsPlaced} enemy boats in Zone {zoneIndex + 1}");
        }

        Debug.Log($"Total enemy boats placed: {enemyBoatPositions.Count}");
    }

    private void ConfigureEnemyBoat(EnemyBoat boat, int zoneIndex)
    {
        if (boat == null) return;

        // Determine number of enemies based on zone
        int numEnemies = Random.Range(minEnemiesPerBoat, maxEnemiesPerBoat + 1);

        // Potentially increase enemy count in harder zones
        if (zoneIndex > 2)
        {
            numEnemies = Mathf.Min(numEnemies + 1, maxEnemiesPerBoat);
        }

        // Find enemy positions on the boat
        Transform[] enemyPositions = new Transform[3]; // Max 3 positions

        // Find enemy position transforms in the boat
        for (int i = 0; i < 3; i++)
        {
            Transform position = boat.transform.Find($"EnemyPosition{i + 1}");
            if (position != null)
            {
                enemyPositions[i] = position;
            }
        }

        // Create enemies and place them on the boat
        for (int i = 0; i < numEnemies; i++)
        {
            if (i >= enemyPositions.Length || enemyPositions[i] == null) continue;

            // Determine if this should be a shark or pirate
            bool isShark = Random.value < sharkProbability;
            GameObject enemyPrefab = isShark ? enemySharkPrefab : enemyPiratePrefab;

            if (enemyPrefab == null)
            {
                enemyPrefab = enemyPiratePrefab; // Fallback to pirate if shark is null
            }

            // Instantiate enemy
            GameObject enemyObject = Instantiate(enemyPrefab, enemyPositions[i].position, Quaternion.identity);
            enemyObject.transform.SetParent(boat.transform);

            // Configure the enemy
            Enemy enemy = enemyObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Set enemy type
                enemy.enemyType = isShark ? Enemy.type.SHARK : Enemy.type.PIRATE;

                // Connect to boat
                enemy.parentBoat = boat;

                // Set initial state
                enemy.SetState(Enemy.state.PATROLLING);

                // Give it a weapon
                AssignRandomWeapon(enemy, zoneIndex);

                // Increase stats based on zone
                float zoneFactor = 1f + ((zoneIndex - 1) * 0.2f); // 20% increase per zone
                enemy.attackDamage = Mathf.RoundToInt(enemy.attackDamage * zoneFactor);
                enemy.attackRange = enemy.attackRange * zoneFactor;
            }
        }
    }

    private void AssignRandomWeapon(Enemy enemy, int zoneIndex)
    {
        if (enemy == null || availableWeapons.Count == 0 || enemy.weaponMountPoint == null)
            return;

        // Get a random weapon from the available weapons
        int weaponIndex = Random.Range(0, availableWeapons.Count);
        Weapon weaponPrefab = availableWeapons[weaponIndex];

        if (weaponPrefab != null)
        {
            // Instantiate the weapon
            Weapon weapon = Instantiate(weaponPrefab, enemy.weaponMountPoint);

            // Adjust weapon stats based on zone
            float zoneFactor = 1f + ((zoneIndex - 1) * 0.15f); // 15% increase per zone
            weapon.damage = Mathf.RoundToInt(weapon.damage * zoneFactor);

            // Assign to enemy
            enemy.equippedWeapon = weapon;
        }
    }

    // Call this method to spawn enemies when the world is generated
    public void SpawnEnemies()
    {
        SpawnEnemyBoats();
    }

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        if (ZoneManager.Instance == null) return;

        // Draw min distance from island
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(ZoneManager.Instance.centerPoint, minDistanceFromIsland);

        // Draw placed enemy boat positions
        Gizmos.color = Color.yellow;
        foreach (Vector2 pos in enemyBoatPositions)
        {
            Gizmos.DrawSphere(pos, 1f);
            // Draw min distance between boats
            Gizmos.DrawWireSphere(pos, minDistanceBetweenBoats / 2f);
        }
    }
}
