using UnityEngine;
using UnityEngine.UI;

public class InventorySlot2 : MonoBehaviour
{
    public int x, y;
    public Image image;
    public bool isOccupied = false;

    public void SetOccupied()
    {
        isOccupied = true;
        image.color = new Color32(100, 100, 100, 255); // Semi-transparent
    }

    public void SetFreed()
    {
        isOccupied = false;
        image.color = new Color(1f, 1f, 1f, 1f); // Fully opaque
    }
}