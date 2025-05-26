using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyBoatPrefab;
    public GameObject enemyPiratePrefab;
    public GameObject enemySharkPrefab;

    [Header("Spawn Configuration")]
    public int baseEnemyBoatsPerZone = 2; // Base number of enemy boats for first zone
    public float boatIncreaseFactor = 1.2f; // How much to increase boats in outer zones
    public float minDistanceBetweenBoats = 15f; // Minimum distance between boats
    public float minDistanceFromRocks = 5f; // Minimum distance from rocks
    public float minDistanceFromIsland = 10f; // Minimum distance from island center

    [Header("Enemy Configuration")]
    public int minEnemiesPerBoat = 1;
    public int maxEnemiesPerBoat = 3;

    [Range(0f, 1f)]
    public float sharkProbability = 0; // Probability of spawning a shark instead of pirate

    [Header("Weapon Configuration")]
    public List<Weapon> availableWeapons = new List<Weapon>();

    [Header("Enemy Weapons")]
    public ItemDefinition kalashnikovDef;
    public ItemDefinition pistolDef;
    public ItemDefinition shotgunDef;
    public GameObject equipItemPrefab; // The same prefab used in PlayerEquipment

    public ZoneManager zoneManager;

    private List<Vector2> enemyBoatPositions = new List<Vector2>();
    public WorldGenerator worldGenerator;

    private void Awake()
    {
      zoneManager = FindFirstObjectByType<ZoneManager>();
        if (zoneManager == null)
        {
            Debug.LogException(new System.Exception("ZoneManager not found! Make sure it is present in the scene."));
        }
    }

    private void Start()
    {
        worldGenerator = FindFirstObjectByType<WorldGenerator>();
        if (worldGenerator == null)
        {
            Debug.LogException(new System.Exception("WorldGenerator not found!"));
        }
    }

    public void SpawnEnemyBoats()
    {
        Debug.LogError("SpawnEnemyBoats called");
        if (zoneManager == null || zoneManager.zones.Count == 0)
        {
            Debug.LogError(
                "Cannot spawn enemy boats: ZoneManager not initialized or no zones created."
            );
            return;
        }

        if (enemyBoatPrefab == null || enemyPiratePrefab == null)
        {
            Debug.LogError("Enemy boat or enemy prefabs not assigned.");
            return;
        }

        enemyBoatPositions.Clear();

        int zonesCount = zoneManager.zones.Count;
        Vector2 centerPoint = zoneManager.centerPoint;

        // Calculate the total area of all zones combined (similar to FishingSpotSpawner)
        float totalArea = 0f;
        float[] zoneAreas = new float[zonesCount];

        for (int i = 0; i < zonesCount; i++)
        {
            Zone zone = zoneManager.zones[i];
            float zoneArea =
                Mathf.PI * (Mathf.Pow(zone.outerRadius, 2) - Mathf.Pow(zone.innerRadius, 2));
            zoneAreas[i] = zoneArea;
            totalArea += zoneArea;
        }

        // Spawn enemy boats in each zone
        for (int zoneIndex = 0; zoneIndex < zonesCount; zoneIndex++)
        {
            Zone currentZone = zoneManager.zones[zoneIndex];

            // Skip the first zone (closest to island) to give player a safe starting area
            if (zoneIndex == 0)
                continue;

            // Calculate enemy boats for this zone - more in outer zones
            int zoneEnemyBoats = Mathf.RoundToInt(
                baseEnemyBoatsPerZone * Mathf.Pow(boatIncreaseFactor, zoneIndex)
            );

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
                bool tooCloseToRocks =
                    worldGenerator != null
                    && worldGenerator.CheckRockPositions(
                        candidatePos,
                        minDistanceFromRocks
                    );

                if (!tooCloseToOtherBoats && !tooCloseToRocks)
                {
                    // Position is valid, create enemy boat
                    enemyBoatPositions.Add(candidatePos);
                    GameObject enemyBoatObject = Instantiate(
                        enemyBoatPrefab,
                        candidatePos,
                        Quaternion.identity
                    );
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
        }
    }

    private void ConfigureEnemyBoat(EnemyBoat boat, int zoneIndex)
    {
        if (boat == null)
        {
            Debug.LogError("ConfigureEnemyBoat: Boat is null");
            return;
        }

        // Clear any existing enemies in the boat's list
        boat.ClearEnemies();

        // Determine number of enemies based on zone
        int numEnemies = Random.Range(minEnemiesPerBoat, maxEnemiesPerBoat + 1);

        // Potentially increase enemy count in harder zones
        if (zoneIndex > 2)
        {
            numEnemies = Mathf.Min(numEnemies + 1, maxEnemiesPerBoat);
        }

        // Find enemy positions on the boat
        List<Transform> foundPositions = new List<Transform>();

        // Find enemy position transforms in the boat - try both direct and recursive search
        for (int i = 0; i < 3; i++)
        {
            // Try direct child first
            Transform position = boat.transform.Find($"EnemyPosition{i + 1}");

            if (position == null)
            {
                // Try recursive search
                position = FindChildRecursively(boat.transform, $"EnemyPosition{i + 1}");
            }

            if (position != null)
            {
                foundPositions.Add(position);
            }
            else
            {
                Debug.LogWarning($"Could not find EnemyPosition{i + 1} on boat");
            }
        }

        if (foundPositions.Count == 0)
        {
            Debug.LogError("No enemy positions found on boat! Creating emergency positions.");
            // Create emergency positions if none found
            for (int i = 0; i < 3; i++)
            {
                GameObject posObj = new GameObject($"EnemyPosition{i + 1}");
                posObj.transform.SetParent(boat.transform);
                posObj.transform.localPosition = new Vector3(i - 1, 0, 0); // Simple line formation
                foundPositions.Add(posObj.transform);
            }
        }

        // Create enemies and place them on the boat
        for (int i = 0; i < numEnemies && i < foundPositions.Count; i++)
        {
            // Determine if this should be a shark or pirate
            bool isShark = false; // Force pirates only
            GameObject enemyPrefab = enemyPiratePrefab;

            if (enemyPrefab == null)
            {
                Debug.LogError("Enemy prefab is null!");
                enemyPrefab = enemyPiratePrefab; // Fallback
                if (enemyPrefab == null)
                    continue; // Skip if still null
            }

            // Instantiate enemy
            GameObject enemyObject = Instantiate(
                enemyPrefab,
                foundPositions[i].position,
                Quaternion.identity
            );
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

                // Give pirates a weapon but not sharks
                if (enemy.enemyType == Enemy.type.PIRATE)
                {
                    AssignRandomWeapon(enemy, zoneIndex);
                }

                // Add to boat's enemies list
                boat.AddEnemy(enemy);
            }
            else
            {
                Debug.LogError("Instantiated enemy doesn't have Enemy component!");
            }
        }
    }

    private void AssignRandomWeapon(Enemy enemy, int zoneIndex)
    {
        if (enemy == null)
        {
            Debug.LogWarning("Cannot assign weapon: Enemy is null");
            return;
        }

        if (enemy.weaponMountPoint == null)
        {
            Debug.LogWarning($"Cannot assign weapon: Enemy {enemy.name} has no weaponMountPoint");
            return;
        }

        // Choose a random weapon type
        float randomValue = Random.value;
        ItemDefinition weaponDef = null;

        // More powerful weapons in higher zones
        if (zoneIndex >= 3)
        {
            // Higher zones get better weapons
            if (randomValue < 0.5f)
                weaponDef = kalashnikovDef; // 50% chance for kalashnikov
            else if (randomValue < 0.8f)
                weaponDef = shotgunDef; // 30% chance for shotgun
            else
                weaponDef = pistolDef; // 20% chance for pistol
        }
        else if (zoneIndex == 2)
        {
            // Mid zone
            if (randomValue < 0.3f)
                weaponDef = kalashnikovDef; // 30% chance for kalashnikov
            else if (randomValue < 0.6f)
                weaponDef = shotgunDef; // 30% chance for shotgun
            else
                weaponDef = pistolDef; // 40% chance for pistol
        }
        else
        {
            // Starting zone
            if (randomValue < 0.1f)
                weaponDef = kalashnikovDef; // 10% chance for kalashnikov
            else if (randomValue < 0.3f)
                weaponDef = shotgunDef; // 20% chance for shotgun
            else
                weaponDef = pistolDef; // 70% chance for pistol
        }

        // Fallback to pistol if weapon definition is null
        if (weaponDef == null)
        {
            Debug.LogWarning("Selected weapon definition is null, falling back to pistol");
            weaponDef = pistolDef;

            // If still null, we can't proceed
            if (weaponDef == null)
            {
                Debug.LogError("Cannot assign weapon: All weapon definitions are null!");
                return;
            }
        }

        // Create the weapon using ItemFactory
        Item weaponItem = ItemFactory.CreateItem(weaponDef);
        if (weaponItem == null)
        {
            Debug.LogError($"ItemFactory.CreateItem failed for {weaponDef.itemName}");
            return;
        }

        if (!(weaponItem is Weapon))
        {
            Debug.LogError($"Created item is not a weapon: {weaponItem.GetType().Name}");
            return;
        }

        Weapon weapon = (Weapon)weaponItem;

        // Check if we have the prefab
        if (equipItemPrefab == null)
        {
            Debug.LogError("equipItemPrefab is null! Cannot create weapon visual");
            return;
        }

        // Create visual representation of the weapon
        GameObject weaponVisual = GameObject.Instantiate(equipItemPrefab, enemy.weaponMountPoint);

        // Scale down the weapon visual for enemies
        weaponVisual.transform.localScale = new Vector3(0.25f, 0.25f, 1f);

        // Set up the visual
        ItemEquipVisual visual = weaponVisual.GetComponent<ItemEquipVisual>();
        if (visual != null)
        {
            // Add enemy weapon controller to make it aim at player
            weaponVisual.AddComponent<EnemyWeaponController>();

            // Configure appropriate offsets for enemy weapons
            visual.positionOffset = new Vector3(0.2f, 0, 0);
            visual.rotationOffset = Vector3.zero;
            visual.SetupVisual(weaponDef, enemy.weaponMountPoint);

            // Find/create the fire point
            Transform firePoint = weaponVisual.transform.Find("FirePoint");
            if (firePoint == null)
            {
                // Create a fire point if one doesn't exist
                GameObject firePointObj = new GameObject("FirePoint");
                firePoint = firePointObj.transform;
                firePoint.SetParent(weaponVisual.transform);
                firePoint.localPosition = new Vector3(0.5f, 0, 0); // Adjust position as needed
            }

            // Set the fire point on the weapon
            weapon.SetFirePoint(firePoint);
        }
        else
        {
            Debug.LogError("ItemEquipVisual component missing on equipItemPrefab!");
            return;
        }

        // Adjust weapon stats based on zone
        float zoneFactor = 1f + ((zoneIndex - 1) * 0.15f); // 15% increase per zone
        weapon.damage = Mathf.RoundToInt(weapon.damage * zoneFactor);

        // Assign to enemy
        enemy.equippedWeapon = weapon;
    }

    // Call this method to spawn enemies when the world is generated
    public void SpawnEnemies()
    {
        SpawnEnemyBoats();
    }

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        if (zoneManager == null)
            return;

        // Draw min distance from island
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(zoneManager.centerPoint, minDistanceFromIsland);

        // Draw placed enemy boat positions
        Gizmos.color = Color.yellow;
        foreach (Vector2 pos in enemyBoatPositions)
        {
            Gizmos.DrawSphere(pos, 1f);
            // Draw min distance between boats
            Gizmos.DrawWireSphere(pos, minDistanceBetweenBoats / 2f);
        }
    }

    // Helper method to find a child recursively
    private Transform FindChildRecursively(Transform parent, string name)
    {
        // Check each child
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            // Check if this child matches
            if (child.name == name)
                return child;

            // Check child's children recursively
            Transform found = FindChildRecursively(child, name);
            if (found != null)
                return found;
        }

        // Not found
        return null;
    }
}
