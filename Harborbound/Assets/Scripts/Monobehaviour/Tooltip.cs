using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tooltip : MonoBehaviour
{
    public static Tooltip Instance;

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private LayoutElement layoutElement;
    [SerializeField] private int characterWrapLimit = 60;
    [SerializeField] private RectTransform canvasRectTransform;
    [SerializeField] private RectTransform rectTransform;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        gameObject.SetActive(false);

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        // Ensure tooltip stays active when panels change
        DontDestroyOnLoad(gameObject);
    }

    public void ShowTooltip(Item item)
    {
        if (item == null || item.definition == null)
            return;

        // Set up tooltip content
        titleText.text = item.GetName();
        descriptionText.text = item.definition.description;

        // Build stat text based on item type
        string statText = "";

        switch (item.definition.type)
        {
            case ItemDefinition.ItemType.WEAPON:
                statText = $"Damage: {item.definition.damage}\n" +
                           $"Range: {item.definition.range}\n" +
                           $"Attack Speed: {item.definition.attackSpeed}";
                break;

            case ItemDefinition.ItemType.FISH:
                Fish fish = item as Fish;
                statText = $"$ {item.GetValue()}\n" +
                           $"{(fish != null ? fish.size.ToString("F1") + " kgs" : "Unknown")}";
                break;

            case ItemDefinition.ItemType.FISHING_ROD:
                statText = $"Cast Range: {item.definition.castRange}";
                break;

            default:
                statText = $"Value: {item.definition.value}";
                break;
        }

        statsText.text = statText;

        // Adjust layout width based on content
        int titleLength = titleText.text.Length;
        int descriptionLength = descriptionText.text.Length;
        int statsLength = statsText.text.Length;

        layoutElement.enabled =
            titleLength > characterWrapLimit ||
            descriptionLength > characterWrapLimit ||
            statsLength > characterWrapLimit;

        gameObject.SetActive(true);

        UpdatePosition();
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }

    public void UpdatePosition()
    {
        Vector2 mousePosition = Input.mousePosition;

        // Check if tooltip would go beyond screen bounds
        float pivotX = mousePosition.x / Screen.width;
        float pivotY = mousePosition.y / Screen.height;

        // Invert pivot if tooltip would go outside screen
        if (pivotX > 0.8f)
            pivotX = 1;
        else
            pivotX = 0;

        if (pivotY < 0.2f)
            pivotY = 0;
        else
            pivotY = 1;

        rectTransform.pivot = new Vector2(pivotX, pivotY);
        transform.position = mousePosition;
    }

    private void Update()
    {
        if (gameObject.activeSelf)
        {
            UpdatePosition();
        }
    }
}