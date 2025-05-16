using UnityEngine;

public class ItemEquipVisual : MonoBehaviour
{
    public Transform attachPoint; // Where on the player to attach
    public Vector3 positionOffset;
    public Vector3 rotationOffset;

    private SpriteRenderer spriteRenderer;
    private ItemDefinition itemDefinition;

    // Add these fields to track animation state
    private bool isAnimationPlaying = false;
    private Coroutine currentAnimationCoroutine = null;
    private Weapon attachedWeapon; // Reference to the attached weapon

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

                    // Store reference to the weapon component
                    attachedWeapon = transform.parent?.GetComponentInParent<Weapon>();
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

    // Modified PlayUseAnimation method
    public void PlayUseAnimation()
    {
        // For automatic weapons, only play animation if not already playing
        if (isAnimationPlaying)
        {
            return;
        }

        // For non-automatic weapons, check fire rate
        if (attachedWeapon != null && attachedWeapon.weaponType != Weapon.WeaponType.AUTOMATIC)
        {
            float timeSinceLastFire = Time.time - attachedWeapon.LastFireTime;
            float cooldownTime = 1f / attachedWeapon.fireRate;

            // If trying to fire too soon, don't play animation
            if (timeSinceLastFire < cooldownTime)
            {
                return;
            }
        }

        // Start animation
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
        }

        currentAnimationCoroutine = StartCoroutine(UseAnimationCoroutine());
    }

    private System.Collections.IEnumerator UseAnimationCoroutine()
    {
        isAnimationPlaying = true;

        // Store original rotation
        Quaternion startRotation = transform.localRotation;

        // Determine animation based on item type
        float animDuration = 0.3f;

        // For automatic weapons, sync animation duration to fire rate
        if (attachedWeapon != null && attachedWeapon.weaponType == Weapon.WeaponType.AUTOMATIC)
        {
            // Make animation slightly shorter than fire rate to ensure smooth transitions
            animDuration = Mathf.Min(0.3f, (0.9f / attachedWeapon.fireRate));
        }

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
        isAnimationPlaying = false;
        currentAnimationCoroutine = null;
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
