using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] private float fishScaleFactor = 1f;

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
    [SerializeField]
    public InventoryGrid mainInventory;

    // Events for UI updates
    public delegate void InventoryChangeHandler();
    public event InventoryChangeHandler OnInventoryChanged;

    public static PlayerInventory Instance;

    private void Start()
    {
        Debug.Log($"PlayerInventory Start is called #############");
    }
    private void Awake()
    {
        Debug.Log($"PlayerInventory Awake called. Instance is currently: {(Instance == null ? "NULL" : "EXISTS")}");
        Debug.Log($"This GameObject: {gameObject.name}, InstanceID: {GetInstanceID()}");

        if (Instance == null)
        {
            Debug.Log("Setting this as the Instance and making persistent");
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Only initialize if not already initialized
            if (mainInventory == null)
            {
                mainInventory = new InventoryGrid(inventoryWidth, inventoryHeight);
                Debug.Log("New inventory grid created");
            }
            else
            {
                Debug.Log("Using existing inventory grid");
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Debug.Log($"Instance already exists! Destroying duplicate. Existing Instance ID: {Instance.GetInstanceID()}");
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"PlayerInventory: Scene loaded. Items in inventory: {GetAllItems().Count}");
        foreach (Item item in GetAllItems())
        {
            Debug.Log($"  - {item.GetName()}");
        }
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
