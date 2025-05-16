public class InventorySlot
{
    public Item item;
    public bool isTaken;
    public bool isPartOfItem;
    public int mainItemSlotX;
    public int mainItemSlotY;
    public int x;
    public int y;

    public InventorySlot(int x, int y)
    {
        this.x = x;
        this.y = y;
        ClearSlot();
    }

    public void SetItem(Item newItem)
    {
        item = newItem;
        isTaken = newItem != null;
    }

    public void ClearSlot()
    {
        item = null;
        isTaken = false;
        isPartOfItem = false;
        mainItemSlotX = x;
        mainItemSlotY = y;
    }
}
