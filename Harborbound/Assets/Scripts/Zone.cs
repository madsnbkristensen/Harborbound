using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour
{
    // This script holds the zone class that is used to define the zones in the game.

    public int id;
    public string name;
    public float innerRadius;
    public float outerRadius;
    public Color color;

    public float fishSpawnRate;
    public float enemySpawnRate;
    public List<Fish> availableFish;
    public int difficulty;

    public List<EnemyType> enemyTypes;

    public Fish GetRandomFishForZone(Zone zone)
    {
        return availableFish[UnityEngine.Random.Range(0, availableFish.Count)];
    }

    public bool IsPositionInZone(Vector2 playerPosition, Vector2 islandCenterPosition)
    {
        float distance = Vector2.Distance(playerPosition, islandCenterPosition);
        return distance >= innerRadius && distance <= outerRadius;
    }
}
