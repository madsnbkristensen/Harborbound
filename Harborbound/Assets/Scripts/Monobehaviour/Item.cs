using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

// Base Item class
public class Item : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Reference to the definition (blueprint)
    public ItemDefinition definition;
    public bool isHovered = false;
    public List<InventorySlot2> occupiedSlots = new List<InventorySlot2>();
    public GameObject background;

    // Instance-specific properties
    public int currentStackSize = 1;

    // Basic getters that use the definition
    public string GetName() => definition?.itemName;
    public Sprite GetIcon() => definition?.icon;

    // Value calculation
    public virtual int GetValue()
    {
        return definition != null ? definition.value : 0;
    }

    // Stack management
    public int AddToStack(int amount)
    {
        if (!definition.isStackable)
            return amount; // Can't stack, return the full amount

        int spaceInStack = definition.maxStackSize - currentStackSize;
        int amountToAdd = Mathf.Min(amount, spaceInStack);

        currentStackSize += amountToAdd;
        return amount - amountToAdd; // Return leftover amount
    }

    public int RemoveFromStack(int amount)
    {
        int amountToRemove = Mathf.Min(amount, currentStackSize);
        currentStackSize -= amountToRemove;
        return amountToRemove;
    }

    public bool CanStack(Item otherItem)
    {
        return definition.isStackable &&
               definition.id == otherItem.definition.id &&
               currentStackSize < definition.maxStackSize;
    }

    // Use method - override in subclasses
    public virtual void Use(Player player)
    {
        Debug.Log($"Using {GetName()}");
    }

    // Refresh item properties from the definition
    public virtual void RefreshFromDefinition()
    {
        if (definition == null)
        {
            Debug.LogWarning($"Cannot refresh {name}: definition is null");
            return;
        }

        // Set shared properties that all items have
        currentStackSize = 1; // Reset stack size or set from definition if needed

        // Debug.Log($"Base Item refreshed from definition: {definition.itemName}");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        FindFirstObjectByType<Player>().hoveredItem = this;
        Debug.LogWarning($"Fish {GetName()} hovered over");
        if (this != null && Tooltip.Instance != null)
        {
            Tooltip.Instance.ShowTooltip(this);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        Debug.LogWarning($"Fish {GetName()} stopped hovering");
        if (this != null && Tooltip.Instance != null)
        {
            Tooltip.Instance.HideTooltip();
        }
    }

    public void Update()
    {
        if (isHovered && Input.GetKeyDown(KeyCode.X))
        {
            InventoryManager2.Instance.DiscardItem(this);
        }
        if (isHovered && Input.GetKeyDown(KeyCode.Mouse0))
        {
            InventoryManager2.Instance.StartItemDrag(this);
        }
    }
}
