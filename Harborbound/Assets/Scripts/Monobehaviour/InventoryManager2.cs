using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class InventoryManager2 : MonoBehaviour
{

    public static InventoryManager2 Instance;
    public GameObject fishContainer;
    [SerializeField] private RectTransform gridContainerRect;
    public List<GameObject> items = new List<GameObject>();
    public GameObject gridContainer;
    public GameObject slotPrefab;

    private InventorySlot2[,] slots = null;

    public void Initialize()
    {
        gridContainerRect = gridContainer.GetComponent<RectTransform>();

        if (!gridContainerRect)
            Debug.LogError("Fish container RectTransform not found!");

        // Get layout settings from PlayerInventory
        paddingLeft = PlayerInventory.Instance.PaddingLeft;
        paddingRight = PlayerInventory.Instance.PaddingRight;
        paddingTop = PlayerInventory.Instance.PaddingTop;
        paddingBottom = PlayerInventory.Instance.PaddingBottom;
        spacingPercentage = PlayerInventory.Instance.SpacingPercentage;

        // Use dimensions from the PlayerInventory
        gridColumns = PlayerInventory.Instance.Width;
        gridRows = PlayerInventory.Instance.Height;
        CreateInventoryGrid();
    }

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        this.Initialize();
    }

    public void SnapItemToSlot(RectTransform item, int x, int y)
    {
        item.position = slots[x, y].transform.position;
    }

    // This function is called when a fish is caught
    public bool TryAddItemToInventory(Item caughtItem)
    {
        caughtItem.transform.SetParent(fishContainer.transform, true);

        int x = 0;
        int y = 0;
        bool fits = TryFitItemInGrid(caughtItem, ref x, ref y);
        Debug.Log("item fits in inventor: " + fits);

        if (fits)
        {
            for (int k = x; k < caughtItem.definition.inventoryWidth + x; k++)
            {
                for (int l = y; l < caughtItem.definition.inventoryHeight + y; l++)
                {
                    slots[k, l].SetOccupied();
                    var inventorySlot = slots[k, l];
                    caughtItem.occupiedSlots.Add(inventorySlot);
                }
            }
            items.Add(caughtItem.gameObject);
            SnapItemToSlot(caughtItem.GetComponent<RectTransform>(), x, y);
        }
        else
        {
            Debug.Log("No space for item");
        }
        return fits;
    }

    public bool TryFitItemInGrid(Item item, ref int outX, ref int outY)
    {
        int itemGridWidth = item.definition.inventoryWidth;
        int itemGridHeight = item.definition.inventoryHeight;

        int inventoryGridWidth = PlayerInventory.Instance.Width;
        int inventoryGridHeight = PlayerInventory.Instance.Height;

        for (int i = 0; i <= inventoryGridWidth - itemGridWidth; i++)
        {
            for (int j = 0; j <= inventoryGridHeight - itemGridHeight; j++)
            {
                bool doesFishFit = true;
                for (int k = i; k < itemGridWidth + i; k++)
                {
                    if (!doesFishFit) break;
                    for (int l = j; l < itemGridHeight + j; l++)
                    {
                        if (slots[k, l].isOccupied)
                        {
                            doesFishFit = false;
                        }
                        if (!doesFishFit) break;
                    }
                }
                if (doesFishFit)
                {
                    outX = i;
                    outY = j;
                    return true;
                }
            }

        }
        return false;
    }

    // Get layout settings from PlayerInventory
    private float paddingLeft;
    private float paddingRight;
    private float paddingTop;
    private float paddingBottom;
    private float spacingPercentage;

    // Use dimensions from the PlayerInventory
    private int gridColumns;
    private int gridRows;

    // This function creates the slots in the inventory grid
    private void CreateInventoryGrid()
    {

        // Calculate available space within the container (accounting for padding)
        float availableWidth = gridContainerRect.rect.width - paddingLeft - paddingRight;
        float availableHeight = gridContainerRect.rect.height - paddingTop - paddingBottom;

        // Calculate slot size based on available space and grid dimensions
        float maxSlotWidth = availableWidth / gridColumns;
        float maxSlotHeight = availableHeight / gridRows;

        // Use the smaller dimension to ensure square slots that fit
        float slotSize = Mathf.Min(maxSlotWidth, maxSlotHeight);

        // Calculate spacing based on slot size
        float spacing = slotSize * spacingPercentage;

        // Adjust slot size to account for spacing
        slotSize -= spacing;

        // Calculate total grid size
        float totalGridWidth = gridColumns * (slotSize + spacing) + spacing;
        float totalGridHeight = gridRows * (slotSize + spacing) + spacing;

        // Center the grid within the container
        float startX = paddingLeft + (availableWidth - totalGridWidth) / 2 + spacing;
        float startY = paddingTop + (availableHeight - totalGridHeight) / 2 + spacing;

        slots = new InventorySlot2[gridColumns, gridRows];

        for (int y = 0; y < gridRows; y++)
        {
            for (int x = 0; x < gridColumns; x++)
            {
                GameObject slotObj = Instantiate(slotPrefab, gridContainer.transform);
                slots[x, y] = slotObj.GetComponent<InventorySlot2>();

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
    }

    public void SetupItemSprite(Item item)
    {
        // Get the item dimensions
        int itemWidth = item.definition.inventoryWidth;
        int itemHeight = item.definition.inventoryHeight;

        // Create item visual container
        RectTransform rectTransform = item.gameObject.GetComponent<RectTransform>();

        // Set anchors and pivot
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);

        rectTransform.anchoredPosition = Vector2.zero;

        // Calculate available space within the container (accounting for padding)
        float availableWidth = gridContainerRect.rect.width - paddingLeft - paddingRight;
        float availableHeight = gridContainerRect.rect.height - paddingTop - paddingBottom;

        // Calculate slot size based on available space and grid dimensions
        float maxSlotWidth = availableWidth / gridColumns;
        float maxSlotHeight = availableHeight / gridRows;

        // Use the smaller dimension to ensure square slots that fit
        float slotSize = slots[0, 0].GetComponent<RectTransform>().rect.width;

        // Calculate spacing based on slot size
        float spacing = slotSize * spacingPercentage;

        // Calculate the exact size based on cells and spacing
        float width = slotSize * itemWidth + ((itemWidth - 1) * spacing);
        float height = slotSize * itemHeight + ((itemHeight - 1) * spacing);
        rectTransform.sizeDelta = new Vector2(width, height);

        rectTransform.anchoredPosition = new Vector2(0, 0);

        // Create background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(item.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = PlayerInventory.Instance.GetItemColor(item.definition.type);

        // icon image child
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(item.transform, false);
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.05f, 0.05f);
        iconRect.anchorMax = new Vector2(0.95f, 0.95f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
        // iconRect.sizeDelta = new Vector2(width, height);
        iconRect.anchoredPosition = new Vector2(0, 0);
        iconRect.localScale = new Vector3(1, 1, 1);
        var iconImage = iconObj.AddComponent<Image>();
        iconImage.sprite = item.GetIcon();
        iconImage.preserveAspect = true;


        // scale to 1
        Debug.LogWarning("scale: " + rectTransform.localScale);
        rectTransform.localScale = new Vector3(1, 1, 1);

    }

    public void DiscardItem(Item itemToDestroy)
    {
        //remove specific fish from inventory and destroy gameobject
        // and free slots from gridcontainer

        for (int i = 0; i < itemToDestroy.occupiedSlots.Count; i++)
        {
            itemToDestroy.occupiedSlots[i].SetFreed();
        }
        items.Remove(itemToDestroy.gameObject);
        Destroy(itemToDestroy.gameObject);
        Tooltip.Instance.HideTooltip();
    }

    public void SellItem(Item itemToSell)
    {
        GameManager.Instance.AddMoney(itemToSell.GetValue());
        UIManager.Instance.UpdateMoneyDisplay(GameManager.Instance.money);
        DiscardItem(itemToSell);
    }

    public void Update()
    {
        foreach (var item in items)
        {
            item.gameObject.transform.localScale = new Vector3(1, 1, 1);
        }
    }
}