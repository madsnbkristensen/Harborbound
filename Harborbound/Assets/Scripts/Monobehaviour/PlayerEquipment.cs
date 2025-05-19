using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    public GameObject equipItemPrefab;
    public Transform weaponAttachPoint;
    public Transform fishingRodAttachPoint;

    // Track equipped items
    public Item equippedWeapon;
    private Item equippedFishingRod;

    // Track visual gameObjects
    private GameObject weaponVisual;
    private GameObject fishingRodVisual;
    // Track the currently equipped item
    public Item currentEquippedItem;
    public GameObject currentEquippedVisual;
    private Transform currentAttachPoint;

    private Player player;

    private void Start()
    {
        player = GetComponent<Player>();
    }

    // Equip an item based on its type
    public void EquipItem(Item item)
    {
        if (item == null)
        {
            // Unequip all items if null is passed
            UnequipWeapon();
            UnequipFishingRod();
            return;
        }

        // Equipment logic based on item type
        if (item.definition == null)
            return;

        switch (item.definition.type)
        {
            case ItemDefinition.ItemType.WEAPON:
                EquipWeapon(item);
                break;

            case ItemDefinition.ItemType.FISHING_ROD:
                EquipFishingRod(item);
                break;

            default:
                Debug.Log($"Cannot equip item of type {item.definition.type}");
                break;
        }
    }

    // Equipment type-specific methods
    private void EquipWeapon(Item weapon)
    {
        // If we currently have a weapon equipped, stop using it first
        if (equippedWeapon != null && equippedWeapon is Weapon currentWeapon)
        {
            currentWeapon.StopUse();
        }

        // Unequip current weapon if any
        if (weaponVisual != null)
            Destroy(weaponVisual);

        // Store reference
        equippedWeapon = weapon;
        // Unequip fishing rod when equipping a weapon
        UnequipFishingRod();

        if (weapon != null && weaponAttachPoint != null)
        {
            // Create visual
            weaponVisual = Instantiate(equipItemPrefab, weaponAttachPoint);
            ItemEquipVisual visual = weaponVisual.GetComponent<ItemEquipVisual>();

            if (visual != null)
            {
                // Set up with weapon-specific offsets
                visual.positionOffset = new Vector3(0.2f, 0, 0);
                visual.rotationOffset = Vector3.zero;
                visual.SetupVisual(weapon.definition, weaponAttachPoint);

                // Set the firePoint reference on the weapon
                if (weapon is Weapon weaponComponent)
                {
                    // Find the FirePoint transform in the visual
                    Transform firePoint = weaponVisual.transform.Find("FirePoint");
                    if (firePoint != null)
                    {
                        // Set the reference in the weapon
                        weaponComponent.SetFirePoint(firePoint);
                    }
                    else
                    {
                        Debug.LogWarning("FirePoint not found in weapon visual");
                    }
                }
            }
        }
    }

    private void EquipFishingRod(Item fishingRod)
    {
        // Unequip current fishing rod if any
        if (fishingRodVisual != null)
            Destroy(fishingRodVisual);

        // Store reference
        equippedFishingRod = fishingRod;
        // Unequip weapon when equipping a fishing rod
        UnequipWeapon();

        if (fishingRod != null && fishingRodAttachPoint != null)
        {
            // Create visual
            fishingRodVisual = Instantiate(equipItemPrefab, fishingRodAttachPoint);
            ItemEquipVisual visual = fishingRodVisual.GetComponent<ItemEquipVisual>();

            if (visual != null)
            {
                // Set up with fishing rod-specific offsets
                visual.positionOffset = new Vector3(0.3f, 0.2f, 0);
                visual.rotationOffset = new Vector3(0, 0, 15);
                visual.SetupVisual(fishingRod.definition, fishingRodAttachPoint);
            }
        }
    }

    private void UnequipWeapon()
    {
        // Stop any active use effects first
        if (equippedWeapon != null && equippedWeapon is Weapon weapon)
        {
            weapon.StopUse();
        }

        if (weaponVisual != null)
            Destroy(weaponVisual);

        equippedWeapon = null;
    }

    private void UnequipFishingRod()
    {
        if (fishingRodVisual != null)
            Destroy(fishingRodVisual);

        equippedFishingRod = null;
    }

    // Get the currently equipped item (weapon or fishing rod)
    public Item GetEquippedItem()
    {
        if (equippedWeapon != null)
            return equippedWeapon;

        if (equippedFishingRod != null)
            return equippedFishingRod;

        return null;
    }

    // Check if an item of specified type is equipped
    public bool IsEquippedItemOfType(ItemDefinition.ItemType itemType)
    {
        ItemDefinition def = CurrentEquippedItem;
        return def != null && def.type == itemType;
    }

    // Start using the equipped item with appropriate behavior
    public void StartUsingEquippedItem()
    {
        Item equippedItem = GetEquippedItem();
        if (equippedItem == null) return;

        // Play animation based on equipped item type
        if (equippedWeapon != null)
        {
            if (weaponVisual != null)
            {
                ItemEquipVisual visual = weaponVisual.GetComponent<ItemEquipVisual>();
                if (visual != null)
                    visual.PlayUseAnimation();
            }
        }
        else if (equippedFishingRod != null)
        {
            if (fishingRodVisual != null)
            {
                ItemEquipVisual visual = fishingRodVisual.GetComponent<ItemEquipVisual>();
                if (visual != null)
                    visual.PlayUseAnimation();
            }
        }

        // Use the item
        equippedItem.Use(player);
    }

    // Stop using the equipped item - only needed for continuous-use items like weapons
    public void StopUsingEquippedItem()
    {
        if (equippedWeapon != null && equippedWeapon is Weapon weapon)
        {
            weapon.StopUse();
        }
        // Fishing rods don't need a stop method as they're single-action
    }

    // Read-only property to get the currently equipped item definition
    public ItemDefinition CurrentEquippedItem
    {
        get
        {
            if (equippedWeapon != null)
                return equippedWeapon.definition;

            if (equippedFishingRod != null)
                return equippedFishingRod.definition;

            return null;
        }
    }
}
