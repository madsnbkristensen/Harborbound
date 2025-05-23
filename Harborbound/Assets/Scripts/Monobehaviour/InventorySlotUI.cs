using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IDropHandler
{
    [SerializeField]
    private int slotX;
    [SerializeField]
    private int slotY;
    [SerializeField]
    private PlayerInventory inventory;
    [SerializeField]
    private Image background;

    public void Initialize(int x, int y, PlayerInventory playerInventory)
    {
        slotX = x;
        slotY = y;
        inventory = playerInventory;

        background = GetComponent<Image>();
        if (background == null)
        {
            background = gameObject.AddComponent<Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Get the dragged item
        GameObject draggedObject = eventData.pointerDrag;
        if (draggedObject == null) return;

        ItemUI itemUI = draggedObject.GetComponent<ItemUI>();
        if (itemUI == null) return;

        // Notify inventory UI controller
        InventoryUIController controller = GetComponentInParent<InventoryUIController>();
        if (controller != null)
        {
            controller.OnItemDroppedOnSlot(itemUI.item, slotX, slotY);
        }
    }
}
