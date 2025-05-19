using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Configuration")]
    [SerializeField] private int inventoryWidth = 10;
    [SerializeField] private int inventoryHeight = 8;

    [Header("UI Configuration")]
    [SerializeField] private float spacingPercentage = 0.07f; // Spacing as % of slot size
    [SerializeField] private float paddingLeft = 0f;
    [SerializeField] private float paddingRight = 0f;
    [SerializeField] private float paddingTop = 0f;
    [SerializeField] private float paddingBottom = 0f;

    [Header("Item Appearance")]
    [SerializeField] private Color weaponColor = new Color(0.7f, 0.3f, 0.3f, 0.6f);
    [SerializeField] private Color fishColor = new Color(0.3f, 0.5f, 0.7f, 0.6f);
    [SerializeField] private Color defaultItemColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
    [SerializeField] private float fishScaleFactor = 1f; // Scale up fish sprites

    // Public accessors
    public int Width => inventoryWidth;
    public int Height => inventoryHeight;
    public float SpacingPercentage => spacingPercentage;
    public float PaddingLeft => paddingLeft;
    public float PaddingRight => paddingRight;
    public float PaddingTop => paddingTop;
    public float PaddingBottom => paddingBottom;
    public Color GetItemColor(ItemDefinition.ItemType itemType)
    {
        switch (itemType)
        {
            case ItemDefinition.ItemType.WEAPON:
                return weaponColor;
            case ItemDefinition.ItemType.FISH:
                return fishColor;
            default:
                return defaultItemColor;
        }
    }
    public float FishScaleFactor => fishScaleFactor;

    // Inventory data structure
    public InventoryGrid mainInventory { get; private set; }

    // Events for UI updates
    public delegate void InventoryChangeHandler();
    public event InventoryChangeHandler OnInventoryChanged;

    private void Awake()
    {
        mainInventory = new InventoryGrid(inventoryWidth, inventoryHeight);
    }

    // Add an item at a specific position
    public bool AddItemAt(Item item, int posX, int posY)
    {
        if (mainInventory.PlaceItem(item, posX, posY))
        {
            OnInventoryChanged?.Invoke();
            return true;
        }
        return false;
    }

    // Auto-place an item in first available slot
    public bool AddItem(Item item)
    {
        Vector2Int? position = mainInventory.FindFirstFreeSlot(item);
        if (position.HasValue)
        {
            return AddItemAt(item, position.Value.x, position.Value.y);
        }
        return false;
    }

    // Remove an item at position
    public Item RemoveItemAt(int posX, int posY)
    {
        Item item = mainInventory.GetItemAt(posX, posY);
        if (item != null && mainInventory.RemoveItem(posX, posY))
        {
            OnInventoryChanged?.Invoke();
            return item;
        }
        return null;
    }

    // Check if we can place an item
    public bool CanPlaceItem(Item item, int posX, int posY)
    {
        return mainInventory.CanPlaceItem(item, posX, posY);
    }

    // Get all items in inventory
    public List<Item> GetAllItems()
    {
        return mainInventory.GetAllItems();
    }

    // Check if inventory can fit a new item
    public bool HasRoomForItem(Item item)
    {
        return mainInventory.FindFirstFreeSlot(item) != null;
    }
}
