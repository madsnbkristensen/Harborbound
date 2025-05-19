using UnityEngine;

public class FishingSpot : MonoBehaviour
{
    // This script holds the Fishingspot class.

    public int maxNumberOfFish = 5;

    public int minNumberOfFish = 1;
    public int numberOfFish;
    public float minSize = 0.5f;
    public float maxSize = 1.5f;
    public float size;
    public int fishingSpotZone; // The zone this fishing spot belongs to

    public void SetRandomNumberOfFish()
    {
        numberOfFish = Random.Range(minNumberOfFish, maxNumberOfFish);
    }

    public void DetermineFishingSpotSize()
    {
        // scale the fishing spot size based on the number of fish
        size = Mathf.Lerp(minSize, maxSize, (float)numberOfFish / maxNumberOfFish);

        // transform the sprite size
        transform.localScale = new Vector2(size, size);
    }

    public void DestroyFishingSpot()
    {
        if (numberOfFish <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void RemoveFish(int amount)
    {
        numberOfFish -= amount;

        // Update visual size to match remaining fish
        DetermineFishingSpotSize();

        // Check if depleted
        DestroyFishingSpot();
    }

}
