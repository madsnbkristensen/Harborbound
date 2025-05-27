using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
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

    [Header("Bobber stuff")]
    public GameObject bobberPrefab;
    private GameObject activeBobber;
    private Vector3 fishingPosition;

    [Header("Fishing Settings")]
    public float minTimeUntilBite = 1.5f;
    public float maxTimeUntilBite = 5.0f;
    public float catchWindowDuration = 1.5f; // How long player has to press button
    public KeyCode catchKey = KeyCode.Space;
    public KeyCode cancelKey = KeyCode.Space; // Same key to cancel early

    // Add a small cooldown to prevent immediate recasting
    private float castCooldown = 0.5f;
    private float lastFishingEndTime = 0f;

    [Header("State")]
    public bool isFishing = false;
    private Item currentCatch = null;
    private int currentZoneId = 1; // Default to zone 1
    private bool fishIsBiting = false; // Track if a fish is biting

    // Events
    public delegate void FishingEvent(Item caughtFish);
    public event FishingEvent OnFishCaught;
    public event FishingEvent OnFishBite;

    private void Start()
    {
        // Subscribe to our own OnFishCaught event to show animations
        OnFishCaught += HandleFishCaught;

        // Get player inventory reference if not assigned
        if (PlayerInventory.Instance == null)
        {
            // If still null, find in scene
            if (PlayerInventory.Instance == null)
            {
                if (PlayerInventory.Instance == null)
                {
                    Debug.LogWarning(
                        "PlayerInventory not found! Caught fish won't be added to inventory."
                    );
                }
            }
        }
    }

    private void Update()
    {
        // Check for early cancellation only when fishing and no fish is biting yet
        if (isFishing && !fishIsBiting && Input.GetKeyDown(cancelKey))
        {
            Debug.Log("Fishing canceled early");
            StopFishing();
        }
    }

    // Add this new method:
    private void HandleFishCaught(Item caughtFish)
    {
        // Don't proceed if we're missing something
        if (catchAnimationPrefab == null || caughtFish == null || Camera.main == null)
            return;

        // Get the camera transform
        Transform cameraTransform = Camera.main.transform;

        // Create the animation object as a child of the camera
        GameObject animObj = Instantiate(catchAnimationPrefab, cameraTransform);

        // Position it in front of the camera, centered with a slight downward offset
        float verticalOffset = 1f; // Adjust this value as needed
        animObj.transform.localPosition = new Vector3(0, verticalOffset, 10); // Local position relative to camera

        Debug.Log("Created fish animation as child of camera");

        CatchAnimation anim = animObj.GetComponent<CatchAnimation>();
        if (anim != null)
        {
            anim.SetupAnimation(caughtFish.definition, () => { });
        }

        // In FishingManager's fishing sequence where fish gets added
        bool addedToInventory = false;
        if (PlayerInventory.Instance != null)
        {
            Debug.Log(
                $"BEFORE adding fish: Inventory has {PlayerInventory.Instance.GetAllItems().Count} items"
            );
            // addedToInventory = PlayerInventory.Instance.AddItem(currentCatch);
            Debug.Log(
                $"AFTER adding fish: Inventory has {PlayerInventory.Instance.GetAllItems().Count} items. Added successfully: {addedToInventory}"
            );
        }
    }

    // New method to cast bobber and start fishing
    public void CastBobber(
        Vector3 startPos,
        Vector3 targetPos,
        int zoneId,
        Transform rodTransform = null
    )
    {
        // Don't cast if already fishing
        if (isFishing)
            return;

        if (gameManager != null)
            gameManager.ChangeState(GameManager.GameState.FISHING);

        // Clear any existing bobber
        if (activeBobber != null)
        {
            Destroy(activeBobber);
        }

        // Store the zone
        currentZoneId = zoneId;

        // Create bobber if prefab exists
        if (bobberPrefab != null)
        {
            activeBobber = Instantiate(bobberPrefab, startPos, Quaternion.identity);
            Bobber bobberComponent = activeBobber.GetComponent<Bobber>();

            if (bobberComponent == null)
            {
                bobberComponent = activeBobber.AddComponent<Bobber>();
            }

            // Initialize bobber with rod transform
            bobberComponent.Initialize(
                startPos,
                targetPos,
                8f,
                OnBobberReachedDestination,
                rodTransform
            );
        }
        else
        {
            Debug.LogWarning("Bobber prefab not assigned to FishingManager!");
            // If no bobber prefab, just start fishing immediately
            StartFishing(zoneId, targetPos);
        }
    }

    // Callback for when bobber reaches its destination
    private void OnBobberReachedDestination(Vector3 position)
    {
        AudioManager.Instance.Play(AudioManager.SoundType.CastSplash);
        // Store fishing position
        fishingPosition = position;

        // Start fishing at this position
        StartFishing(currentZoneId, position);
    }

    // Update your existing StartFishing method to accept a position
    public void StartFishing(int zoneId, Vector3 position)
    {
        if (isFishing)
            return;

        currentZoneId = zoneId;
        fishingPosition = position;
        isFishing = true;
        fishIsBiting = false;

        Debug.Log("Started fishing in zone " + zoneId + " at position " + position);

        // Start fishing coroutine
        StartCoroutine(FishingSequence());
    }

    // Update StopFishing to destroy the bobber
    public void StopFishing()
    {
        if (!isFishing)
            return;

        isFishing = false;
        fishIsBiting = false;
        StopAllCoroutines();

        // Record the time when fishing stopped
        lastFishingEndTime = Time.time;

        // Destroy bobber
        if (activeBobber != null)
        {
            Destroy(activeBobber);
            activeBobber = null;
        }

        // Change game state back
        if (gameManager != null)
            gameManager.ChangeState(GameManager.GameState.ROAMING);
    }

    public bool CanCastAgain()
    {
        return Time.time >= lastFishingEndTime + castCooldown;
    }

    // Main fishing sequence
    private IEnumerator FishingSequence()
    {
        // Get the fishing spot information from the bobber
        bool isInFishingSpot = false;
        FishingSpot spot = null;
        int fishingZoneId = 1;

        if (activeBobber != null)
        {
            Bobber bobber = activeBobber.GetComponent<Bobber>();
            if (bobber != null)
            {
                isInFishingSpot = bobber.IsInFishingSpot();
                spot = bobber.GetCurrentFishingSpot();
                fishingZoneId = bobber.GetCurrentZone();
            }
        }

        // Adjust wait time based on whether we're in a fishing spot
        float baseWaitTime = Random.Range(minTimeUntilBite, maxTimeUntilBite);
        float waitTime;

        if (isInFishingSpot && spot != null && spot.numberOfFish > 0)
        {
            // Fish bite faster in fishing spots with more fish
            float fishFactor = Mathf.Clamp01((float)spot.numberOfFish / spot.maxNumberOfFish);
            waitTime = Mathf.Lerp(baseWaitTime, minTimeUntilBite, fishFactor);
            Debug.Log(
                $"Fishing in spot with {spot.numberOfFish} fish. (Zone {fishingZoneId}) Bite time: {waitTime}s"
            );
        }
        else if (!isInFishingSpot)
        {
            // Longer wait time when not in a fishing spot, fish are rare
            waitTime = baseWaitTime * 2f;
            Debug.Log($"Fishing in open water (Zone {fishingZoneId}). Bite time: {waitTime}s");
        }
        else
        {
            // Fishing spot with no fish
            waitTime = float.MaxValue; // Will never bite
            Debug.Log("This fishing spot is depleted. No fish will bite!");
        }

        // Wait for bite
        float elapsedTime = 0f;
        while (elapsedTime < waitTime && isFishing)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        // If fishing was canceled or wait time is "infinite", exit
        if (!isFishing || waitTime >= float.MaxValue)
            yield break;

        // Fish is biting!
        if (isInFishingSpot && spot != null && spot.numberOfFish > 0)
        {
            // Use the fishing spot's zone for catching
            currentCatch = CatchFish(spot.fishingSpotZone);
        }
        else
        {
            // Use the zone determined by position
            currentCatch = CatchFish(fishingZoneId);

            // Make catches in open water more rare (might be nothing)
            if (Random.value > 0.3f) // 70% chance of catching nothing in open water
            {
                HelperManager.Instance.handleSpecificTooltip("Nothing seems to be biting here...");
                currentCatch = null;
                yield return new WaitForSeconds(1.0f);
                StopFishing();
                yield break;
            }
        }

        fishIsBiting = currentCatch != null;

        if (fishIsBiting)
        {
            AudioManager.Instance.Play(AudioManager.SoundType.Bite);
            // Play bite animation
            StartCoroutine(BobberBiteAnimation());

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

                    // Remove fish from the fishing spot if applicable
                    if (isInFishingSpot && spot != null)
                    {
                        spot.RemoveFish(1);
                        Debug.Log($"Fishing spot now has {spot.numberOfFish} fish remaining.");
                    }

                    bool addedToInventory = false;
                    if (InventoryManager2.Instance != null)
                    {
                        // Use InventoryManager2 to add fish to inventory
                        InventoryManager2.Instance.SetupItemSprite(currentCatch);
                        addedToInventory = InventoryManager2.Instance.TryAddItemToInventory(
                            currentCatch
                        );
                        if (addedToInventory)
                        {
                            Debug.Log($"Added {currentCatch.GetName()} to inventory!");
                            OnFishCaught?.Invoke(currentCatch);
                        }
                        else
                        {
                            AudioManager.Instance.Play(AudioManager.SoundType.Full_Inventory);
                            Destroy(currentCatch.gameObject); // Clean up fish if not added to inventory
                        }
                    }
                    // {
                    //     addedToInventory = PlayerInventory.Instance.AddItem(currentCatch);

                    //     if (addedToInventory)
                    //     {
                    //         Debug.Log($"Added {currentCatch.GetName()} to inventory!");
                    //     }
                    //     else
                    //     {
                    //         Debug.Log("Inventory is full! Fish was released.");
                    //         Destroy(currentCatch.gameObject); // Clean up fish if not added to inventory
                    //     }
                    // }
                    else
                    {
                        Debug.LogWarning("PlayerInventory reference is missing in FishingManager!");
                    }

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
            AudioManager.Instance.Play(AudioManager.SoundType.Full_Inventory);
            Debug.Log("Nothing seems to be biting here...");
            yield return new WaitForSeconds(1.0f);
        }

        // End fishing state
        StopFishing();
    }

    // Update your CatchFish method to consider fishing spots
    public Item CatchFish(int zoneId)
    {
        // Use the database to get a random fish for the zone
        ItemDefinition fishDef = itemDatabase.GetRandomFishInZone(zoneId);
        if (fishDef == null)
        {
            Debug.LogWarning($"No fish found for zone {zoneId}");
            return null;
        }

        // Instantiate the fish item
        Item fishItem = ItemFactory.CreateItem(fishDef);

        Debug.Log($"Created fish: {fishDef.itemName} for zone {zoneId}");
        return fishItem;
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
            item.type == ItemDefinition.ItemType.FISH && ArrayContains(item.availableZones, zoneId)
        );
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

    // For debugging: catch a specific type of fish
    public Item DebugCatchSpecificFish(string fishName)
    {
        ItemDefinition fishDef = allItems.Find(item =>
            item.type == ItemDefinition.ItemType.FISH
            && item.itemName.ToLower() == fishName.ToLower()
        );

        if (fishDef != null)
            return ItemFactory.CreateItem(fishDef); // Changed this line to use ItemFactory

        Debug.LogWarning("Fish not found: " + fishName);
        return null;
    }

    private void OnDestroy()
    {
        OnFishCaught -= HandleFishCaught;
    }

    // animation for bobber

    IEnumerator BobberBiteAnimation()
    {
        if (activeBobber == null)
            yield break;

        Vector3 originalPos = activeBobber.transform.position; // Use the bobber's position
        Vector3 downPos = originalPos + Vector3.down * 0.4f;

        // Quick down
        yield return StartCoroutine(MoveToPosition(originalPos, downPos, 0.1f));

        // Quick back up
        yield return StartCoroutine(MoveToPosition(downPos, originalPos, 0.15f));
    }

    IEnumerator MoveToPosition(Vector3 from, Vector3 to, float duration)
    {
        if (activeBobber == null)
            yield break;

        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            activeBobber.transform.position = Vector3.Lerp(from, to, t); // Move the bobber!
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        activeBobber.transform.position = to;
    }
}
