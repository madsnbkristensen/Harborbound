using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Item item { get; private set; }
    private PlayerInventory inventory;
    private InventoryUIController controller;
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Image iconImage;

    public event System.Action OnDragStarted;
    public event System.Action OnDragEnded;

    public void Initialize(Item itemRef, PlayerInventory playerInventory, InventoryUIController uiController)
    {
        item = itemRef;
        inventory = playerInventory;
        controller = uiController;
        rectTransform = GetComponent<RectTransform>();

        // Ensure pivot is set to top-left
        rectTransform.pivot = new Vector2(0, 1);

        // Add canvas group for drag opacity
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Find canvas
        canvas = GetComponentInParent<Canvas>();
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
        if (item != null && Tooltip.Instance != null)
        {
            Tooltip.Instance.ShowTooltip(item);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Tooltip.Instance != null)
        {
            Tooltip.Instance.HideTooltip();
        }
    }
}
