using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    public GameObject equipItemPrefab;
    public Transform weaponAttachPoint;
    public Transform fishingRodAttachPoint;

    // Track the currently equipped item
    private Item currentEquippedItem;
    public GameObject currentEquippedVisual;
    private Transform currentAttachPoint;

    private Player player;

    private void Start()
    {
        player = GetComponent<Player>();
    }

    // Equip an item (or unequip if null)
    public void EquipItem(Item item)
    {
        // Unequip current item if there is one
        if (currentEquippedVisual != null)
        {
            Destroy(currentEquippedVisual);
            currentEquippedVisual = null;
        }

        // Update the reference to the current item
        currentEquippedItem = item;

        // If null was passed, we're just unequipping
        if (item == null)
            return;

        // Determine the appropriate attach point based on item type
        if (item.definition == null)
            return;

        Transform attachPoint = null;
        Vector3 posOffset = Vector3.zero;
        Vector3 rotOffset = Vector3.zero;

        switch (item.definition.type)
        {
            case ItemDefinition.ItemType.WEAPON:
                attachPoint = weaponAttachPoint;
                posOffset = new Vector3(0.2f, 0, 0);
                rotOffset = Vector3.zero;
                break;

            case ItemDefinition.ItemType.FISHING_ROD:
                attachPoint = fishingRodAttachPoint;
                posOffset = new Vector3(0.3f, 0.2f, 0);
                rotOffset = new Vector3(0, 0, 15);
                break;

            default:
                Debug.Log($"Cannot equip item of type {item.definition.type}");
                return;
        }

        if (attachPoint != null)
        {
            // Create visual
            currentAttachPoint = attachPoint;
            currentEquippedVisual = Instantiate(equipItemPrefab, attachPoint);
            ItemEquipVisual visual = currentEquippedVisual.GetComponent<ItemEquipVisual>();

            if (visual != null)
            {
                visual.positionOffset = posOffset;
                visual.rotationOffset = rotOffset;
                visual.SetupVisual(item.definition, attachPoint);
            }
        }
    }

    // Use the currently equipped item
    public void UseEquippedItem()
    {
        if (currentEquippedItem != null)
        {
            // Play animation
            if (currentEquippedVisual != null)
            {
                ItemEquipVisual visual = currentEquippedVisual.GetComponent<ItemEquipVisual>();
                if (visual != null)
                    visual.PlayUseAnimation();
            }

            // Use the item
            currentEquippedItem.Use(player);
        }
    }

    // Getter for the current item definition (for compatibility)
    public ItemDefinition CurrentEquippedItem
    {
        get { return currentEquippedItem?.definition; }
    }
}
