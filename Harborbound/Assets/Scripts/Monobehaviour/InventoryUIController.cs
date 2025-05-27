using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class InventoryUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform slotsContainer; // The container within the border
    [SerializeField] private GameObject slotPrefab;


    private GameObject[,] slotObjects = null;
    private Dictionary<Item, GameObject> itemUIObjects = new Dictionary<Item, GameObject>();
    private float slotSize; // Will be calculated based on available space
    private float spacing;  // Will be calculated based on slot size

    public static InventoryUIController Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }

        if (slotObjects == null)
        {
            Debug.LogWarning("Creating inventory grid");
            CreateInventoryGridUI();
        }
        else
        {
            Debug.LogWarning("Inventory grid already exists");
        }
    }

    private void Start()
    {
        // Create inventory grid UI

        // Subscribe to inventory changes
        PlayerInventory.Instance.OnInventoryChanged += UpdateInventoryUI;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("InventoryUIController: Scene loaded, refreshing references");

        // Force EventSystem refresh
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            Debug.Log($"EventSystem current: {EventSystem.current.name}");
        }

        // Check Canvas raycast status
        GraphicRaycaster raycaster = GetComponentInParent<GraphicRaycaster>();
        Debug.Log($"GraphicRaycaster found: {raycaster != null}, enabled: {raycaster?.enabled}");

        // Recreate ItemUI components to fix event connections
        RecreateItemUIComponents();
    }

    private void RecreateItemUIComponents()
    {
        Debug.Log("Recreating ItemUI components...");

        // Destroy existing ItemUI visuals (they're broken anyway)
        foreach (var itemObj in itemUIObjects.Values)
        {
            Destroy(itemObj);
        }
        itemUIObjects.Clear();

        // Recreate them fresh from the persistent inventory data
        List<Item> items = PlayerInventory.Instance.GetAllItems();
        Debug.Log($"Recreating visuals for {items.Count} items");

        foreach (Item item in items)
        {
            CreateItemVisual(item);
        }

        Debug.Log("ItemUI recreation complete");
    }

    private void CreateInventoryGridUI()
    {
        // Use dimensions from the PlayerInventory
        int gridColumns = PlayerInventory.Instance.Width;
        int gridRows = PlayerInventory.Instance.Height;
        float spacingPercentage = PlayerInventory.Instance.SpacingPercentage;
        float paddingLeft = PlayerInventory.Instance.PaddingLeft;
        float paddingRight = PlayerInventory.Instance.PaddingRight;
        float paddingTop = PlayerInventory.Instance.PaddingTop;
        float paddingBottom = PlayerInventory.Instance.PaddingBottom;

        // Calculate available space within the container (accounting for padding)
        float availableWidth = slotsContainer.rect.width - paddingLeft - paddingRight;
        float availableHeight = slotsContainer.rect.height - paddingTop - paddingBottom;

        // Calculate slot size based on available space and grid dimensions
        float maxSlotWidth = availableWidth / gridColumns;
        float maxSlotHeight = availableHeight / gridRows;

        // Use the smaller dimension to ensure square slots that fit
        slotSize = Mathf.Min(maxSlotWidth, maxSlotHeight);

        // Calculate spacing based on slot size
        spacing = slotSize * spacingPercentage;

        // Adjust slot size to account for spacing
        slotSize -= spacing;

        // Calculate total grid size
        float totalGridWidth = gridColumns * (slotSize + spacing) + spacing;
        float totalGridHeight = gridRows * (slotSize + spacing) + spacing;

        // Center the grid within the container
        float startX = paddingLeft + (availableWidth - totalGridWidth) / 2 + spacing;
        float startY = paddingTop + (availableHeight - totalGridHeight) / 2 + spacing;

        slotObjects = new GameObject[gridColumns, gridRows];

        for (int y = 0; y < gridRows; y++)
        {
            for (int x = 0; x < gridColumns; x++)
            {
                GameObject slotObj = Instantiate(slotPrefab, slotsContainer);
                slotObjects[x, y] = slotObj;

                RectTransform rectTransform = slotObj.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.pivot = new Vector2(0, 1);
                rectTransform.sizeDelta = new Vector2(slotSize, slotSize);

                // Position slots with calculated spacing
                float posX = startX + x * (slotSize + spacing);
                float posY = -startY - y * (slotSize + spacing);
                rectTransform.anchoredPosition = new Vector2(posX, posY);

                // Initialize slot UI
                InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
                if (slotUI == null)
                    slotUI = slotObj.AddComponent<InventorySlotUI>();
                slotUI.Initialize(x, y, PlayerInventory.Instance);
            }
        }

        // Initial UI update
        UpdateInventoryUI();
    }

    private void UpdateInventoryUI()
    {
        Debug.Log("InventoryUIController: UpdateInventoryUI called");

        // Should be reimplemented because it doubles items when dropped
        // Clear existing item visuals
        foreach (var itemObj in itemUIObjects.Values)
        {
            Debug.Log($"Destroying item visual: {itemObj.name}");
            Destroy(itemObj);
        }
        itemUIObjects.Clear();

        // Create visuals for items
        List<Item> items = PlayerInventory.Instance.GetAllItems();
        Debug.Log($"Creating visuals for {items.Count} items");

        foreach (Item item in items)
        {
            CreateItemVisual(item);
        }
    }

    private void CreateItemVisual(Item item)
    {
        // Find the item's position in the grid
        bool foundItem = false;
        int mainX = 0, mainY = 0;
        int gridColumns = PlayerInventory.Instance.Width;
        int gridRows = PlayerInventory.Instance.Height;

        for (int y = 0; y < gridRows && !foundItem; y++)
        {
            for (int x = 0; x < gridColumns && !foundItem; x++)
            {
                if (PlayerInventory.Instance.mainInventory.GetItemAt(x, y) == item)
                {
                    mainX = x;
                    mainY = y;
                    foundItem = true;
                    break;
                }
            }
        }

        if (!foundItem) return;

        // Get layout settings from PlayerInventory
        float paddingLeft = PlayerInventory.Instance.PaddingLeft;
        float paddingRight = PlayerInventory.Instance.PaddingRight;
        float paddingTop = PlayerInventory.Instance.PaddingTop;
        float paddingBottom = PlayerInventory.Instance.PaddingBottom;

        // Get the item dimensions
        int itemWidth = item.definition.inventoryWidth;
        int itemHeight = item.definition.inventoryHeight;

        // Create item visual container
        GameObject itemObj = new GameObject("Item_" + item.GetName());
        RectTransform rectTransform = itemObj.AddComponent<RectTransform>();
        itemObj.transform.SetParent(slotsContainer, false);

        // Set anchors and pivot
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);

        // Calculate the exact size based on cells and spacing
        float width = (itemWidth * slotSize) + ((itemWidth - 1) * spacing);
        float height = (itemHeight * slotSize) + ((itemHeight - 1) * spacing);
        rectTransform.sizeDelta = new Vector2(width, height);

        // Calculate position based on the same logic as the slots
        float availableWidth = slotsContainer.rect.width - paddingLeft - paddingRight;
        float availableHeight = slotsContainer.rect.height - paddingTop - paddingBottom;
        float totalGridWidth = gridColumns * (slotSize + spacing) + spacing;
        float totalGridHeight = gridRows * (slotSize + spacing) + spacing;

        float startX = paddingLeft + (availableWidth - totalGridWidth) / 2 + spacing;
        float startY = paddingTop + (availableHeight - totalGridHeight) / 2 + spacing;

        float posX = startX + (mainX * (slotSize + spacing));
        float posY = -startY - (mainY * (slotSize + spacing));
        rectTransform.anchoredPosition = new Vector2(posX, posY);

        // Create background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(itemObj.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = PlayerInventory.Instance.GetItemColor(item.definition.type);

        // Create icon with proper scaling
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(itemObj.transform, false);
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();

        // Fill most of the area but leave a small margin
        iconRect.anchorMin = new Vector2(0.05f, 0.05f);
        iconRect.anchorMax = new Vector2(0.95f, 0.95f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;

        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.sprite = item.GetIcon();
        iconImage.preserveAspect = true;

        // For fish specifically, use the scale factor from PlayerInventory
        if (item.definition.type == ItemDefinition.ItemType.FISH)
        {
            // Set image to fill the allocated space better
            iconImage.type = Image.Type.Simple;
            iconImage.preserveAspect = true;

            // Apply the fish scale factor from PlayerInventory
            float fishScaleFactor = PlayerInventory.Instance.FishScaleFactor;
            iconRect.localScale = new Vector3(fishScaleFactor, fishScaleFactor, 1.0f);

            // Center it better
            iconRect.anchoredPosition = Vector2.zero;
        }

        // Add item UI component for drag & drop
        ItemUI itemUI = itemObj.AddComponent<ItemUI>();
        itemUI.Initialize(item, this);

        // Store reference
        itemUIObjects[item] = itemObj;
    }

    // Method for handling item dropping on slots
    public void OnItemDroppedOnSlot(Item item, int targetX, int targetY)
    {
        // Get current position
        int currentX = -1, currentY = -1;
        int gridColumns = PlayerInventory.Instance.Width;
        int gridRows = PlayerInventory.Instance.Height;

        foreach (Item existingItem in itemUIObjects.Keys)
        {
            if (existingItem == item)
            {
                // Find current position
                for (int y = 0; y < gridRows; y++)
                {
                    for (int x = 0; x < gridColumns; x++)
                    {
                        if (PlayerInventory.Instance.mainInventory.GetItemAt(x, y) == item)
                        {
                            currentX = x;
                            currentY = y;
                            break;
                        }
                    }
                    if (currentX != -1) break;
                }
                break;
            }
        }

        // Remove from current position
        if (currentX != -1 && currentY != -1)
        {
            PlayerInventory.Instance.RemoveItemAt(currentX, currentY);
        }

        // Try to place at new position
        if (!PlayerInventory.Instance.AddItemAt(item, targetX, targetY))
        {
            // If failed to place at new position, try to put back at original position
            if (currentX != -1 && currentY != -1)
            {
                PlayerInventory.Instance.AddItemAt(item, currentX, currentY);
            }
        }
    }

    private void OnDestroy()
    {
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnInventoryChanged -= UpdateInventoryUI;
        }
    }
}
