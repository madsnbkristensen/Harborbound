using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingManager : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public Player player;
    public List<ItemDefinition> allItems = new List<ItemDefinition>(); // All items in game
    public ItemDatabase itemDatabase;
    public GameObject catchAnimationPrefab;
    public Transform catchAnimationSpawnPoint;
    // public PlayerInventory playerInventory;

    [Header("Fishing Settings")]
    public float minTimeUntilBite = 1.5f;
    public float maxTimeUntilBite = 5.0f;
    public float catchWindowDuration = 1.5f; // How long player has to press button
    public KeyCode catchKey = KeyCode.Space;

    [Header("State")]
    public bool isFishing = false;
    private Item currentCatch = null;
    private int currentZoneId = 1; // Default to zone 1

    // Events
    public delegate void FishingEvent(Item caughtFish);
    public event FishingEvent OnFishCaught;
    public event FishingEvent OnFishBite;

    private void Start()
    {
        // Subscribe to our own OnFishCaught event to show animations
        OnFishCaught += HandleFishCaught;
    }

    // Add this new method:
    private void HandleFishCaught(Item caughtFish)
    {
        // Don't proceed if we're missing something
        if (catchAnimationPrefab == null || catchAnimationSpawnPoint == null || caughtFish == null)
            return;

        // Create catch animation
        GameObject animObj = Instantiate(catchAnimationPrefab,
            catchAnimationSpawnPoint.position,
            Quaternion.identity);

        CatchAnimation anim = animObj.GetComponent<CatchAnimation>();

        if (anim != null)
        {
            // Start animation
            anim.SetupAnimation(caughtFish.definition, () =>
            {
                // Animation is complete - could add additional logic here
                Debug.Log("Catch animation completed");
            });
        }
    }

    // Start fishing process
    public void StartFishing(int zoneId)
    {
        if (isFishing)
            return;

        currentZoneId = zoneId;
        isFishing = true;

        // Change game state if we have a GameManager
        if (gameManager != null)
        {
            gameManager.ChangeState(GameManager.GameState.FISHING);
        }

        Debug.Log("Started fishing in zone " + zoneId);

        // Start fishing coroutine
        StartCoroutine(FishingSequence());
    }

    // Stop fishing
    public void StopFishing()
    {
        if (!isFishing)
            return;

        isFishing = false;
        StopAllCoroutines();

        // Change game state back
        if (gameManager != null)
            gameManager.ChangeState(GameManager.GameState.ROAMING);

        Debug.Log("Stopped fishing");
    }

    // Main fishing sequence
    private IEnumerator FishingSequence()
    {
        // Wait random time until fish bites
        float waitTime = Random.Range(minTimeUntilBite, maxTimeUntilBite);
        Debug.Log("Waiting for a bite... (" + waitTime + " seconds)");
        yield return new WaitForSeconds(waitTime);

        // Fish is biting!
        currentCatch = CatchFish(currentZoneId);

        if (currentCatch != null)
        {
            Debug.Log("Fish is biting! Press " + catchKey + " to catch!");

            // Trigger bite event
            OnFishBite?.Invoke(currentCatch);

            // Wait for player input
            float catchTimer = 0;
            bool caught = false;

            while (catchTimer < catchWindowDuration && !caught)
            {
                if (Input.GetKeyDown(catchKey))
                {
                    // Player caught the fish!
                    Debug.Log("Caught a " + currentCatch.GetName() + "!");

                    // Add to inventory or handle as needed
                    if (player != null)
                    {
                        // This would be a call to your inventory system
                        // player.inventory.AddItem(currentCatch);
                        Debug.Log("Fish added to inventory");
                    }

                    // Trigger caught event
                    OnFishCaught?.Invoke(currentCatch);
                    caught = true;
                }

                catchTimer += Time.deltaTime;
                yield return null;
            }

            if (!caught)
            {
                Debug.Log("The fish got away!");
                Destroy(currentCatch.gameObject); // Clean up uncaught fish
                currentCatch = null;
            }
        }
        else
        {
            Debug.Log("Nothing seems to be biting here...");
            yield return new WaitForSeconds(1.0f);
        }

        // End fishing state
        StopFishing();
    }

    // Generate a caught fish for the given zone
    public Item CatchFish(int zoneId)
    {
        // Use the database to get a random fish
        ItemDefinition fishDef = itemDatabase.GetRandomFishInZone(zoneId);
        if (fishDef == null)
            return null;

        // Create and return the fish item
        return ItemFactory.CreateItem(fishDef);
    }

    // Helper method to get random fish weighted by rarity
    private ItemDefinition GetRandomFishByRarity(List<ItemDefinition> fishList)
    {
        // Calculate total weight (inverted rarity)
        float totalWeight = 0;
        foreach (var fish in fishList)
        {
            totalWeight += (100 - fish.rarity); // Lower rarity = higher chance
        }

        // Get random point
        float randomPoint = Random.Range(0f, totalWeight);

        // Find corresponding fish
        float currentWeight = 0;
        foreach (var fish in fishList)
        {
            currentWeight += (100 - fish.rarity);
            if (randomPoint <= currentWeight)
                return fish;
        }

        // Fallback
        return fishList[0];
    }

    // Get fish for a specific zone
    private List<ItemDefinition> GetFishInZone(int zoneId)
    {
        // Filter to just fish in this zone
        return allItems.FindAll(item =>
            item.type == ItemDefinition.ItemType.FISH &&
            ArrayContains(item.availableZones, zoneId));
    }

    // Helper method to check if array contains a value
    private bool ArrayContains(int[] array, int value)
    {
        if (array == null)
            return false;

        foreach (int element in array)
        {
            if (element == value)
                return true;
        }

        return false;
    }

    // Get all item definitions
    private List<ItemDefinition> GetAllItemDefinitions()
    {
        return allItems;
    }

    // Simplified interface for fishing spots to use
    public void TriggerFishing(int zoneId)
    {
        if (!isFishing)
            StartFishing(zoneId);
    }

    // For debugging: catch a specific type of fish
    public Item DebugCatchSpecificFish(string fishName)
    {
        ItemDefinition fishDef = allItems.Find(item =>
            item.type == ItemDefinition.ItemType.FISH &&
            item.itemName.ToLower() == fishName.ToLower());

        if (fishDef != null)
            return ItemFactory.CreateItem(fishDef); // Changed this line to use ItemFactory

        Debug.LogWarning("Fish not found: " + fishName);
        return null;
    }

    private void OnDestroy()
    {
        OnFishCaught -= HandleFishCaught;
    }
}
