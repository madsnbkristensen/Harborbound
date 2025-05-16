using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private RectTransform slotsContainer; // The container within the border
    [SerializeField] private GameObject slotPrefab;

    private GameObject[,] slotObjects;
    private Dictionary<Item, GameObject> itemUIObjects = new Dictionary<Item, GameObject>();
    private float slotSize; // Will be calculated based on available space
    private float spacing;  // Will be calculated based on slot size

    private void Start()
    {
        if (playerInventory == null)
            playerInventory = FindFirstObjectByType<PlayerInventory>();

        // Create inventory grid UI
        CreateInventoryGridUI();

        // Subscribe to inventory changes
        playerInventory.OnInventoryChanged += UpdateInventoryUI;
    }

    private void CreateInventoryGridUI()
    {
        // Use dimensions from the PlayerInventory
        int gridColumns = playerInventory.Width;
        int gridRows = playerInventory.Height;
        float spacingPercentage = playerInventory.SpacingPercentage;
        float paddingLeft = playerInventory.PaddingLeft;
        float paddingRight = playerInventory.PaddingRight;
        float paddingTop = playerInventory.PaddingTop;
        float paddingBottom = playerInventory.PaddingBottom;

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
                slotUI.Initialize(x, y, playerInventory);
            }
        }

        // Initial UI update
        UpdateInventoryUI();
    }

    private void UpdateInventoryUI()
    {
        // Clear existing item visuals
        foreach (var itemObj in itemUIObjects.Values)
        {
            Destroy(itemObj);
        }
        itemUIObjects.Clear();

        // Create visuals for items
        List<Item> items = playerInventory.GetAllItems();
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
        int gridColumns = playerInventory.Width;
        int gridRows = playerInventory.Height;

        for (int y = 0; y < gridRows && !foundItem; y++)
        {
            for (int x = 0; x < gridColumns && !foundItem; x++)
            {
                if (playerInventory.mainInventory.GetItemAt(x, y) == item)
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
        float paddingLeft = playerInventory.PaddingLeft;
        float paddingRight = playerInventory.PaddingRight;
        float paddingTop = playerInventory.PaddingTop;
        float paddingBottom = playerInventory.PaddingBottom;

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
        bgImage.color = playerInventory.GetItemColor(item.definition.type);

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
            float fishScaleFactor = playerInventory.FishScaleFactor;
            iconRect.localScale = new Vector3(fishScaleFactor, fishScaleFactor, 1.0f);

            // Center it better
            iconRect.anchoredPosition = Vector2.zero;
        }

        // Add item UI component for drag & drop
        ItemUI itemUI = itemObj.AddComponent<ItemUI>();
        itemUI.Initialize(item, playerInventory, this);

        // Store reference
        itemUIObjects[item] = itemObj;
    }

    // Method for handling item dropping on slots
    public void OnItemDroppedOnSlot(Item item, int targetX, int targetY)
    {
        // Get current position
        int currentX = -1, currentY = -1;
        int gridColumns = playerInventory.Width;
        int gridRows = playerInventory.Height;

        foreach (Item existingItem in itemUIObjects.Keys)
        {
            if (existingItem == item)
            {
                // Find current position
                for (int y = 0; y < gridRows; y++)
                {
                    for (int x = 0; x < gridColumns; x++)
                    {
                        if (playerInventory.mainInventory.GetItemAt(x, y) == item)
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
            playerInventory.RemoveItemAt(currentX, currentY);
        }

        // Try to place at new position
        if (!playerInventory.AddItemAt(item, targetX, targetY))
        {
            // If failed to place at new position, try to put back at original position
            if (currentX != -1 && currentY != -1)
            {
                playerInventory.AddItemAt(item, currentX, currentY);
            }
        }
    }

    private void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= UpdateInventoryUI;
        }
    }
}
