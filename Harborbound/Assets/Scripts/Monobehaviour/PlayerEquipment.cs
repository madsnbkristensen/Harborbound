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
        // Unequip current weapon if any
        if (weaponVisual != null)
            Destroy(weaponVisual);

        // Store reference
        equippedWeapon = weapon;

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

    // Use the currently equipped items
    public void UseEquippedWeapon()
    {
        if (equippedWeapon != null)
        {
            // Play animation
            if (weaponVisual != null)
            {
                ItemEquipVisual visual = weaponVisual.GetComponent<ItemEquipVisual>();
                if (visual != null)
                    visual.PlayUseAnimation();
            }

            // Use the weapon
            equippedWeapon.Use(player);
        }
    }

    public void UseEquippedFishingRod()
    {
        if (equippedFishingRod != null)
        {
            // Play animation
            if (fishingRodVisual != null)
            {
                ItemEquipVisual visual = fishingRodVisual.GetComponent<ItemEquipVisual>();
                if (visual != null)
                    visual.PlayUseAnimation();
            }

            // Use the fishing rod
            equippedFishingRod.Use(player);
        }
    }

    // This maintains compatibility with your current code
    public void UseEquippedItem()
    {
        // Try to use whatever is equipped, prioritizing weapon
        if (equippedWeapon != null)
        {
            UseEquippedWeapon();
        }
        else if (equippedFishingRod != null)
        {
            UseEquippedFishingRod();
        }
    }

    // Read-only property to get the currently equipped item definition
    // This maintains backward compatibility with your existing code
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
