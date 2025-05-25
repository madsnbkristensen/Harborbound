using UnityEngine;

public class EquipmentTester : MonoBehaviour
{
    public PlayerEquipment playerEquipment;

    // Test item definitions
    public ItemDefinition testWeaponDef;
    public ItemDefinition testFishingRodDef;

    public Player player;

    private Item testWeapon;
    private Item testFishingRod;

    private void Start()
    {
        // Create test items using our ItemFactory
        if (testWeaponDef != null)
            testWeapon = ItemFactory.CreateItem(testWeaponDef);

        if (testFishingRodDef != null)
            testFishingRod = ItemFactory.CreateItem(testFishingRodDef);
    }

    void Update()
    {
        // Handle equipment switching
        HandleEquipmentToggle();

        // Handle item usage based on equipped item type
        if (playerEquipment.IsEquippedItemOfType(ItemDefinition.ItemType.WEAPON))
        {
            HandleWeaponInput();
        }
        else if (playerEquipment.IsEquippedItemOfType(ItemDefinition.ItemType.FISHING_ROD))
        {
            HandleFishingRodInput();
        }
    }

    private void HandleEquipmentToggle()
    {
        if (player.GetComponent<SpriteRenderer>().enabled == false)
        {
            return;
        }
        // Press 1 to equip weapon
        if (Input.GetKeyDown(KeyCode.Alpha1) && testWeapon != null)
        {
            Debug.Log("Equipping weapon");
            playerEquipment.EquipItem(testWeapon);
            if (HelperManager.Instance != null)
            {
                HelperManager.Instance.HandleEquip(testWeapon.gameObject);
            }
        }

        // Press 2 to equip fishing rod
        if (Input.GetKeyDown(KeyCode.Alpha2) && testFishingRod != null)
        {
            Debug.Log("Equipping fishing rod");
            playerEquipment.EquipItem(testFishingRod);

            if (HelperManager.Instance != null)
            {
                HelperManager.Instance.HandleEquip(testFishingRod.gameObject);
            }

        }

        // Press 3 to unequip (pass null)
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("Unequipping item");
            playerEquipment.EquipItem(null);
        }
    }

    private void HandleWeaponInput()
    {
        // Weapons support press-and-hold interaction
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0))
        {
            playerEquipment.StartUsingEquippedItem();
        }

        // For automatic weapons, we need to stop firing when key is released
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Mouse0))
        {
            playerEquipment.StopUsingEquippedItem();
        }
    }

    private void HandleFishingRodInput()
    {
        // Fishing rods use a single press interaction (no hold)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Using fishing rod");
            playerEquipment.StartUsingEquippedItem();
        }
    }
}
