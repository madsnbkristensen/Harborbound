using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour
{
    // This script holds the zone class that is used to define the zones in the game.

    public int id;
    public string zoneName;
    public float innerRadius = 20f;
    public float outerRadius = 30f;
    public float fishSpawnRate = 1f;
    public float enemySpawnRate = 1f;
    //public List<Fish> availableFish = new();
    public int difficulty = 1;
    //public List<EnemyType> enemyTypes = new();

    /* public Fish GetRandomFishForZone(Zone zone)
    {
        return availableFish[UnityEngine.Random.Range(0, availableFish.Count)];
    } */

}
