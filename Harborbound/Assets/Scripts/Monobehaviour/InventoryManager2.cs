using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class InventoryManager2 : MonoBehaviour
{

    public static InventoryManager2 Instance;
    public GameObject itemContainer;
    [SerializeField] private RectTransform gridContainerRect;
    public List<GameObject> items = new List<GameObject>();
    public GameObject gridContainer;
    public GameObject slotPrefab;
    public Canvas canvas;

    private InventorySlot2[,] slots = null;

    [Header("Dragging logic")]
    public GameObject draggingPreview = null;
    [SerializeField] private bool _currentlyDragging = false;
    [SerializeField] private Vector3 _dragRelativeOffset = Vector3.zero;
    [SerializeField] private Item _itemBeingDragged = null;
    [SerializeField] private InventorySlot2 _previousSlot = null;
    [SerializeField] private InventorySlot2 _lastValidSlot = null;

    public void Initialize()
    {
        if (canvas == null)
            Debug.LogException(new Exception("canvas is not set in InventoryManager2 object"));
        if (draggingPreview == null)
            Debug.LogError("dragging preview is not set in InventoryManager2 object");
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
    public void SnapItemToSlot(Item item, InventorySlot2 slot)
    {
        SnapItemToSlot(item.GetComponent<RectTransform>(), slot.x, slot.y);
    }


    public void BindItemToSlot(Item item, int tlx, int tly)
    {
        for (int i = tlx; i < item.definition.inventoryWidth + tlx; i++)
        {
            for (int j = tly; j < item.definition.inventoryHeight + tly; j++)
            {
                slots[i, j].SetOccupied();
                var slot = slots[i, j];
                item.occupiedSlots.Add(slot);
            }
        }
    }
    public void BindItemToSlot(Item item, InventorySlot2 topLeftSlot)
    {
        BindItemToSlot(item, topLeftSlot.x, topLeftSlot.y);
    }

    // This function is called when a fish is caught
    public bool TryAddItemToInventory(Item caughtItem)
    {
        caughtItem.transform.SetParent(itemContainer.transform, true);

        int x = 0;
        int y = 0;
        bool fits = TryFitItemInGrid(caughtItem, ref x, ref y);
        Debug.Log("item fits in inventor: " + fits);

        if (fits)
        {

            BindItemToSlot(caughtItem, x, y);
            items.Add(caughtItem.gameObject);
            SnapItemToSlot(caughtItem.GetComponent<RectTransform>(), x, y);
        }
        else
        {
            Debug.Log("No space for item");
        }
        return fits;
    }

    public bool IsSpaceOccupied(Item item, int x, int y)
    {
        for (int i = x; i < item.definition.inventoryWidth + x; i++)
        {
            for (int j = y; j < item.definition.inventoryHeight + y; j++)
            {
                if (slots[i, j].isOccupied)
                {
                    return true;
                }
            }
        }
        return false;
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
                if (!IsSpaceOccupied(item, i, j))
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
                slots[x, y].x = x;
                slots[x, y].y = y;

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

        item.background = bgObj;

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

    public void StartItemDrag(Item item)
    {
        if (_currentlyDragging)
        {
            Debug.LogWarning("Tried starting an inventory drag while already dragging something");
            return;
        }

        _previousSlot = item.occupiedSlots.ElementAt(0); // 0 will always be TL (topleft)

        item.transform.SetParent(canvas.transform, true);
        _itemBeingDragged = item;

        foreach (InventorySlot2 slot in item.occupiedSlots)
        {
            slot.SetFreed();
        }
        item.occupiedSlots.Clear();

        _dragRelativeOffset = Input.mousePosition - item.transform.position;

        _currentlyDragging = true;
    }

    // returns TL slot or null if not enough space, ref bool to determine if there is an item in the way
    InventorySlot2 GetInventorySlotUnderItem(Item item)
    {
        var itemRectTransform = item.GetComponent<RectTransform>();
        Vector3 itemSize = new Vector3(itemRectTransform.rect.width, itemRectTransform.rect.height);
        Vector3 itemPos = item.background.transform.position - new Vector3(itemSize.x, -itemSize.y) * 0.5f;
        Rect itemRect = new Rect(itemPos, itemSize);

        for (int i = 0; i <= PlayerInventory.Instance.Width - item.definition.inventoryWidth; i++)
        {
            for (int j = 0; j <= PlayerInventory.Instance.Height - item.definition.inventoryHeight; j++)
            {
                var rectTransform = slots[i, j].GetComponent<RectTransform>();
                Vector2 slotPos = rectTransform.position;
                Vector2 slotSize = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
                Vector2 slotCenter = slotPos + slotSize / 2;

                if (itemRect.Contains(slotCenter))
                {
                    return slots[i, j];
                }
            }
        }

        return null;
    }

    public void SetHoveringFalse()
    {
        foreach (GameObject item in items)
        {
            item.GetComponent<Item>().isHovered = false;
        }
    }

    public void Update()
    {
        foreach (var item in items)
        {
            item.gameObject.transform.localScale = new Vector3(1, 1, 1);
        }
        if (_currentlyDragging)
        {
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                if (_lastValidSlot != null)
                {
                    BindItemToSlot(_itemBeingDragged, _lastValidSlot);
                    SnapItemToSlot(_itemBeingDragged, _lastValidSlot);
                }
                else
                {
                    BindItemToSlot(_itemBeingDragged, _previousSlot);
                    SnapItemToSlot(_itemBeingDragged, _previousSlot);
                }

                _itemBeingDragged.transform.SetParent(itemContainer.transform, true);
                draggingPreview.SetActive(false);
                _currentlyDragging = false;
            }
            else
            {
                _itemBeingDragged.transform.position = Input.mousePosition - _dragRelativeOffset;

                InventorySlot2 slot = GetInventorySlotUnderItem(_itemBeingDragged);
                if (slot != null)
                {
                    draggingPreview.SetActive(true);
                    var dragRect = draggingPreview.GetComponent<RectTransform>();
                    dragRect.position = slot.transform.position;

                    // dragRect.position = slot.transform.position;

                    bool isSpaceOccupied = IsSpaceOccupied(_itemBeingDragged, slot.x, slot.y);
                    if (!isSpaceOccupied)
                    {
                        draggingPreview.GetComponent<Image>().color = Color.green;
                        _lastValidSlot = slot;
                    }
                    else
                    {
                        draggingPreview.GetComponent<Image>().color = Color.red;

                    }
                }
            }
        }
    }
}
