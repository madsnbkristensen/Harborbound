using UnityEngine;
using UnityEngine.UI;

public class InventorySlot2 : MonoBehaviour
{
    public int x, y;
    public Image image;
    public bool isOccupied = false;
    public bool isHighlighted = false;
    public Color originalColor;

    public void SetOccupied()
    {
        isOccupied = true;
        image.color = new Color(1f, 1f, 1f, 0.4549019608f); // Semi-transparent
    }

    public void SetFreed()
    {
        isOccupied = false;
        image.color = new Color(1f, 1f, 1f, 0.4549019608f); // 166 opacity
    }
}
