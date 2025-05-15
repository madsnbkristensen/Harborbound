using System.Collections;
using UnityEngine;

public class CatchAnimation : MonoBehaviour
{
    public float animationDuration = 1.5f;
    public float floatHeight = 1.0f;
    public float wobbleAmount = 0.2f;
    public ParticleSystem splashEffect;

    private SpriteRenderer spriteRenderer;
    private ItemDefinition fishDefinition;
    private System.Action onAnimationComplete;
    private Vector3 startLocalPosition;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetupAnimation(ItemDefinition fish, System.Action callback)
    {
        fishDefinition = fish;
        onAnimationComplete = callback;

        // Record the starting local position
        startLocalPosition = transform.localPosition;

        // Set sprite from definition
        if (spriteRenderer && fish != null)
        {
            spriteRenderer.sprite = fish.icon;

            // Make sure it's visible
            spriteRenderer.sortingOrder = 100;
        }

        // Start animation
        StartCoroutine(AnimateCapture());
    }

    private IEnumerator AnimateCapture()
    {
        // Play splash effect
        if (splashEffect)
            splashEffect.Play();

        float startTime = Time.time;

        while (Time.time < startTime + animationDuration)
        {
            // Calculate how far through the animation we are (0-1)
            float progress = (Time.time - startTime) / animationDuration;

            // Arc movement upward then down
            float height = Mathf.Sin(progress * Mathf.PI) * floatHeight;

            // Side-to-side wobble
            float wobble = Mathf.Sin(progress * Mathf.PI * 8) * wobbleAmount;

            // Update position - use local position since we're a child of the camera
            transform.localPosition = startLocalPosition + new Vector3(wobble, height, 0);

            // Fade out near the end
            if (progress > 0.7f)
            {
                Color c = spriteRenderer.color;
                c.a = 1 - ((progress - 0.7f) / 0.3f);
                spriteRenderer.color = c;
            }

            yield return null;
        }

        // Animation complete, invoke callback
        onAnimationComplete?.Invoke();

        // Destroy the animation object
        Destroy(gameObject);
    }
}
