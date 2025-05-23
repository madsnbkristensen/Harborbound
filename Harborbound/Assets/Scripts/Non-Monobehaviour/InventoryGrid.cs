using System.Collections.Generic;
using UnityEngine;

public class InventoryGrid
{
    public int width;
    public int height;
    private InventorySlot[,] slots;

    public InventoryGrid(int width, int height)
    {
        this.width = width;
        this.height = height;
        slots = new InventorySlot[width, height];

        // Initialize all slots
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                slots[x, y] = new InventorySlot(x, y);
            }
        }
    }

    // Check if item can be placed at position
    public bool CanPlaceItem(Item item, int posX, int posY)
    {
        if (item == null) return false;

        int itemWidth = item.definition.inventoryWidth;
        int itemHeight = item.definition.inventoryHeight;

        // Check if position is valid
        if (posX < 0 || posY < 0 || posX + itemWidth > width || posY + itemHeight > height)
            return false;

        // Check if all required slots are free
        for (int x = posX; x < posX + itemWidth; x++)
        {
            for (int y = posY; y < posY + itemHeight; y++)
            {
                if (slots[x, y].isTaken)
                    return false;
            }
        }

        return true;
    }

    // Place item at position
    public bool PlaceItem(Item item, int posX, int posY)
    {
        if (!CanPlaceItem(item, posX, posY))
            return false;

        int itemWidth = item.definition.inventoryWidth;
        int itemHeight = item.definition.inventoryHeight;

        // Mark slots as occupied
        for (int x = posX; x < posX + itemWidth; x++)
        {
            for (int y = posY; y < posY + itemHeight; y++)
            {
                slots[x, y].SetItem(item);
                slots[x, y].isTaken = true;
                slots[x, y].isPartOfItem = true;
                slots[x, y].mainItemSlotX = posX;
                slots[x, y].mainItemSlotY = posY;
            }
        }

        return true;
    }

    // Remove item from grid
    public bool RemoveItem(int posX, int posY)
    {
        // Check if position is valid
        if (posX < 0 || posY < 0 || posX >= width || posY >= height)
            return false;

        InventorySlot slot = slots[posX, posY];
        if (!slot.isTaken)
            return false;

        // Get main slot position if this is part of a larger item
        int mainX = slot.mainItemSlotX;
        int mainY = slot.mainItemSlotY;

        // Get the item from the main slot
        Item item = slots[mainX, mainY].item;
        if (item == null)
            return false;

        int itemWidth = item.definition.inventoryWidth;
        int itemHeight = item.definition.inventoryHeight;

        // Clear all slots used by this item
        for (int x = mainX; x < mainX + itemWidth; x++)
        {
            for (int y = mainY; y < mainY + itemHeight; y++)
            {
                slots[x, y].ClearSlot();
            }
        }

        return true;
    }

    // Find first available position for an item
    public Vector2Int? FindFirstFreeSlot(Item item)
    {
        if (item == null) return null;

        int itemWidth = item.definition.inventoryWidth;
        int itemHeight = item.definition.inventoryHeight;

        for (int y = 0; y <= height - itemHeight; y++)
        {
            for (int x = 0; x <= width - itemWidth; x++)
            {
                if (CanPlaceItem(item, x, y))
                    return new Vector2Int(x, y);
            }
        }

        return null;
    }

    // Get item at position
    public Item GetItemAt(int posX, int posY)
    {
        if (posX < 0 || posY < 0 || posX >= width || posY >= height)
            return null;

        return slots[posX, posY].item;
    }

    // Get all unique items in the grid
    public List<Item> GetAllItems()
    {
        HashSet<Item> uniqueItems = new HashSet<Item>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (slots[x, y].isTaken && slots[x, y].item != null &&
                    x == slots[x, y].mainItemSlotX && y == slots[x, y].mainItemSlotY)
                {
                    uniqueItems.Add(slots[x, y].item);
                }
            }
        }

        return new List<Item>(uniqueItems);
    }

    // Check if inventory is completely full
    public bool IsFull()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!slots[x, y].isTaken)
                    return false;
            }
        }

        return true;
    }

    // Clear the entire inventory
    public void ClearInventory()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                slots[x, y].ClearSlot();
            }
        }
    }
}
