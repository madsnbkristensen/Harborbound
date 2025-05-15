using UnityEngine;

public class EquipmentTester : MonoBehaviour
{
    public PlayerEquipment playerEquipment;

    // Only keep a single weapon definition
    public ItemDefinition testWeaponDef;
    public ItemDefinition testFishingRodDef;

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
        // Press 1 to equip weapon
        if (Input.GetKeyDown(KeyCode.Alpha1) && testWeapon != null)
        {
            Debug.Log("Equipping weapon");
            playerEquipment.EquipItem(testWeapon);
        }

        // Press 2 to equip fishing rod
        if (Input.GetKeyDown(KeyCode.Alpha2) && testFishingRod != null)
        {
            Debug.Log("Equipping fishing rod");
            playerEquipment.EquipItem(testFishingRod);
        }

        // Press 3 to unequip (pass null)
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("Unequipping item");
            playerEquipment.EquipItem(null);
        }

        // Handle weapon firing
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Starting to use equipped item");
            playerEquipment.StartUsingEquippedWeapon();
        }

        // For automatic weapons, we still need to stop firing when key is released
        if (Input.GetKeyUp(KeyCode.Space))
        {
            Debug.Log("Stopping use of equipped item");
            playerEquipment.StopUsingEquippedWeapon();
        }
    }
}
