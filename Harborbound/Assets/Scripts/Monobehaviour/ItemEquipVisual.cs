using UnityEngine;

public class ItemEquipVisual : MonoBehaviour
{
    public Transform attachPoint; // Where on the player to attach
    public Vector3 positionOffset;
    public Vector3 rotationOffset;

    private SpriteRenderer spriteRenderer;
    private ItemDefinition itemDefinition;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetupVisual(ItemDefinition item, Transform attachment)
    {
        itemDefinition = item;

        // Set sprite
        if (spriteRenderer && item != null)
        {
            spriteRenderer.sprite = item.icon;

            // Adjust sprite parameters based on item type
            switch (item.type)
            {
                case ItemDefinition.ItemType.WEAPON:
                    // Weapon-specific adjustments
                    spriteRenderer.sortingOrder = 2; // Draw above player
                    break;

                case ItemDefinition.ItemType.FISHING_ROD:
                    // Fishing rod-specific adjustments
                    spriteRenderer.sortingOrder = 2; // Draw above player
                    break;

                default:
                    spriteRenderer.sortingOrder = 1;
                    break;
            }
        }

        // Attach to player
        if (attachment != null)
        {
            attachPoint = attachment;
            transform.SetParent(attachment);
            transform.localPosition = positionOffset;
            transform.localEulerAngles = rotationOffset;
        }
    }

    // For weapons, could include methods like:
    public void PlayUseAnimation()
    {
        // Simple rotation animation for using the item
        StartCoroutine(UseAnimationCoroutine());
    }

    private System.Collections.IEnumerator UseAnimationCoroutine()
    {
        // Store original rotation
        Quaternion startRotation = transform.localRotation;

        // Determine animation based on item type
        float animDuration = 0.3f;
        float startTime = Time.time;

        while (Time.time < startTime + animDuration)
        {
            float progress = (Time.time - startTime) / animDuration;

            if (itemDefinition != null)
            {
                if (itemDefinition.type == ItemDefinition.ItemType.WEAPON)
                {
                    // Swing animation for weapons
                    float swingAngle = Mathf.Sin(progress * Mathf.PI) * 45f;
                    transform.localRotation = startRotation * Quaternion.Euler(0, 0, swingAngle);
                }
                else if (itemDefinition.type == ItemDefinition.ItemType.FISHING_ROD)
                {
                    // Cast animation for fishing rods
                    float castAngle = Mathf.Sin(progress * Mathf.PI) * 30f;
                    transform.localRotation = startRotation * Quaternion.Euler(0, 0, -castAngle);
                }
            }

            yield return null;
        }

        // Return to original rotation
        transform.localRotation = startRotation;
    }

    // Add this method to your ItemEquipVisual class
    public void RotateTowardsMouse()
    {
        // Only apply to weapons
        if (itemDefinition == null || itemDefinition.type != ItemDefinition.ItemType.WEAPON)
            return;

        // Get mouse position and calculate angle
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Apply rotation
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // And update the existing Update method if you have one, or add it
    private void Update()
    {
        // Only rotate weapons
        if (itemDefinition != null && itemDefinition.type == ItemDefinition.ItemType.WEAPON)
        {
            RotateTowardsMouse();
        }
    }

    // Optional debug visualization
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }
}
