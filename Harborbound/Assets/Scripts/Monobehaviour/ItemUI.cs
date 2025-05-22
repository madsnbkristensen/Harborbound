using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Item item { get; private set; }
    private InventoryUIController controller;
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Image iconImage;
    private bool isHovered = false; // Track if mouse is hovering over this item


    public event System.Action OnDragStarted;
    public event System.Action OnDragEnded;

    void Start()
    {
        Debug.Log($"ItemUI {name}: Canvas={canvas}, CanvasGroup={canvasGroup}, EventSystem={EventSystem.current}");
    }

    public void Initialize(Item itemRef, InventoryUIController uiController)
    {
        item = itemRef;
        controller = uiController;
        rectTransform = GetComponent<RectTransform>();

        // Ensure pivot is set to top-left
        rectTransform.pivot = new Vector2(0, 1);

        // Add canvas group for drag opacity
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Find canvas
        canvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        // Check for X key press while item is being hovered
        if (isHovered && Input.GetKeyDown(KeyCode.X))
        {
            RemoveItem();
            Tooltip.Instance.HideTooltip();
        }
        if (isHovered && Input.GetKeyDown(KeyCode.X))
        {
            RemoveItem();
            Tooltip.Instance.HideTooltip();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.anchoredPosition;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        // Bring to front while dragging
        transform.SetAsLastSibling();

        OnDragStarted?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            rectTransform.position = eventData.position;
        }
        else
        {
            // If using Camera mode
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out position);
            rectTransform.position = canvas.transform.TransformPoint(position);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // If not dropped on a slot, return to original position
        if (eventData.pointerCurrentRaycast.gameObject == null ||
            eventData.pointerCurrentRaycast.gameObject.GetComponent<InventorySlotUI>() == null)
        {
            rectTransform.anchoredPosition = originalPosition;
        }

        OnDragEnded?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;

        if (item != null && Tooltip.Instance != null)
        {
            Tooltip.Instance.ShowTooltip(item);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;

        if (Tooltip.Instance != null)
        {
            Tooltip.Instance.HideTooltip();
        }
    }

    public void RemoveItem()
    {
        if (item == null || PlayerInventory.Instance == null)
            return;

        // Find the item's position in the grid
        bool foundItem = false;
        int mainX = 0, mainY = 0;
        int gridColumns = PlayerInventory.Instance.Width;
        int gridRows = PlayerInventory.Instance.Height;

        for (int y = 0; y < gridRows && !foundItem; y++)
        {
            for (int x = 0; x < gridColumns && !foundItem; x++)
            {
                if (PlayerInventory.Instance.mainInventory.GetItemAt(x, y) == item)
                {
                    mainX = x;
                    mainY = y;
                    foundItem = true;
                    break;
                }
            }
        }

        if (foundItem)
        {
            // Remove the item from PlayerInventory.Instance
            PlayerInventory.Instance.RemoveItemAt(mainX, mainY);
            Debug.Log($"Removed item: {item.GetName()} from PlayerInventory.Instance at position ({mainX}, {mainY})");

            // The UI will be updated automatically through the PlayerInventory.Instance change event
        }
    }
}
