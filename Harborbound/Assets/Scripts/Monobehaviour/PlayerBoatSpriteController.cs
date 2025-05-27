using UnityEngine;

public class PlayerBoatSpriteController : MonoBehaviour
{
    [Header("Basic Direction Sprites")]
    public Sprite spriteUp;
    public Sprite spriteDown;
    public Sprite spriteRight; // Will be flipped for left

    [Header("Diagonal Sprites")]
    public Sprite spriteRightUp;   // Will be flipped for leftUp
    public Sprite spriteLeftDown;  // Already facing left-down, will be flipped for rightDown

    [Header("Settings")]
    [Range(0.1f, 0.9f)]
    public float diagonalThreshold = 0.4f; // When to use diagonal sprites

    private SpriteRenderer spriteRenderer;
    private Vector2 lastDirection = Vector2.zero;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found on this GameObject");
        }
    }

    public void UpdateDirection(Vector2 direction)
    {
        // Skip if no significant movement
        if (direction.magnitude < 0.1f)
        {
            // Keep last direction when stopping
            direction = lastDirection;
            if (direction.magnitude < 0.1f) // If there's still no valid direction
                return;
        }
        else
        {
            // Store current direction for when we stop
            lastDirection = direction;
        }

        // Normalize direction to get consistent results
        direction = direction.normalized;

        // Calculate absolute values for cleaner comparisons
        float absX = Mathf.Abs(direction.x);
        float absY = Mathf.Abs(direction.y);

        // Determine if we're facing horizontally, vertically, or diagonally
        if (absY > absX && absY > diagonalThreshold)
        {
            // Primarily vertical movement
            if (direction.y > 0)
            {
                // Up
                spriteRenderer.sprite = spriteUp;
                spriteRenderer.flipX = false;
            }
            else
            {
                // Down
                spriteRenderer.sprite = spriteDown;
                spriteRenderer.flipX = false;
            }
        }
        else if (absX > absY && absX > diagonalThreshold)
        {
            // Primarily horizontal movement
            spriteRenderer.sprite = spriteRight;
            spriteRenderer.flipX = direction.x < 0; // Flip for left movement
        }
        else
        {
            // Diagonal movement
            if (direction.y > 0)
            {
                // Upper diagonal
                spriteRenderer.sprite = spriteRightUp;
                spriteRenderer.flipX = direction.x < 0; // Flip for left-up
            }
            else
            {
                // Lower diagonal
                spriteRenderer.sprite = spriteLeftDown;
                spriteRenderer.flipX = direction.x > 0; // Flip for right-down (opposite of other flips)
            }
        }
    }
}
