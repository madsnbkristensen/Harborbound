using UnityEngine;

public class EnemyWeaponController : MonoBehaviour
{
    private Transform playerTransform;
    private SpriteRenderer weaponSprite;

    private void Start()
    {
        // Find the player
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Get the sprite renderer for flipping
        weaponSprite = GetComponent<SpriteRenderer>();
        if (weaponSprite == null)
        {
            weaponSprite = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            // Try to find player if not assigned yet
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                return; // No player to aim at
            }
        }

        // Calculate direction to player
        Vector3 directionToPlayer = playerTransform.position - transform.position;

        // Calculate angle in degrees
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

        // Apply rotation to weapon
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Flip weapon sprite vertically if aiming left (optional - depends on your sprites)
        if (weaponSprite != null)
        {
            // Flip weapon sprite if pointing left
            bool flipY = (angle > 90 || angle < -90);
            weaponSprite.flipY = flipY;
        }
    }
}
