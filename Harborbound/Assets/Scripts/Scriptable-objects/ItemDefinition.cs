using UnityEngine;
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    // Base properties all items have
    [SerializeField] private int _id = 0; // Private backing field
    public int id
    {
        get
        {
            // If ID is 0, generate a random one when first accessed
            if (_id == 0)
            {
                _id = Random.Range(1000, 1000000);
            }
            return _id;
        }
        private set { _id = value; } // Private setter so only this class can change it
    }

    public string itemName;
    public string description;
    public Sprite icon;
    public int value;
    public enum ItemType { WEAPON, AMMO, FISH, TRASH, FISHING_ROD }
    public ItemType type;
    public int inventoryWidth;
    public int inventoryHeight;
    public bool isStackable;
    public int maxStackSize;
    public GameObject prefab;

    // Weapon-specific properties (only used if type == WEAPON)
    [Header("Weapon Properties")]
    public float damage;
    public float range;
    public float attackSpeed;

    // Fish-specific properties (only used if type == FISH)
    [Header("Fish Properties")]
    public float minSize;
    public float maxSize;
    public int[] availableZones;
    public int rarity;

    // Fishing Rod properties (only used if type == FISHING_ROD)
    [Header("Fishing Rod Properties")]
    public float castRange;
    public float bobberTravelSpeed;

    // Called when the scriptable object is first created
    private void OnEnable()
    {
        // Only generate a new ID if it hasn't been set yet
        if (_id == 0)
        {
            _id = Random.Range(1000, 1000000);
        }
    }
}
