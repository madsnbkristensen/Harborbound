using System.Collections;
using UnityEngine;

public class Bobber : MonoBehaviour
{
    // Line renderer component
    private LineRenderer lineRenderer;
    
    // References
    private Transform sourceTransform; // Rod or player
    
    // Movement and animation
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float travelSpeed = 8f;
    private Vector3 basePosition;
    
    // Bobbing parameters
    private float bobAmplitude = 0.1f;
    private float bobFrequency = 2f;
    
    // Callback when fishing should start
    private System.Action<Vector3> onFishingStart;
    
    // This signature matches exactly what FishingManager is calling
    public void Initialize(Vector3 start, Vector3 target, float speed, System.Action<Vector3> callback, Transform source = null)
    {
        startPosition = start;
        targetPosition = target;
        travelSpeed = speed;
        onFishingStart = callback;
        sourceTransform = source;
        
        // Set up line renderer
        SetupLineRenderer();
        
        // Position and start movement
        transform.position = startPosition;
        StartCoroutine(MoveToDestination());
    }
    
    private void SetupLineRenderer()
    {
        // Get or add a line renderer
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        // Configure line renderer
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.01f;
        
        // Set material
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material.color = new Color(0.8f, 0.8f, 0.8f, 0.8f); // Light gray
    }
    
    private void UpdateLine()
    {
        if (lineRenderer == null) return;
        
        // Get source position (rod or player)
        Vector3 sourcePos;
        
        if (sourceTransform != null)
        {
            // Use source transform with a small offset to look better
            sourcePos = sourceTransform.position + new Vector3(0.1f, 0.2f, 0);
        }
        else
        {
            sourcePos = startPosition;
        }
        
        // Update line positions
        lineRenderer.SetPosition(0, sourcePos);
        lineRenderer.SetPosition(1, transform.position);
    }
    
    private IEnumerator MoveToDestination()
    {
        // Calculate journey parameters
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float arcHeight = journeyLength * 0.4f;
        
        float startTime = Time.time;
        float journeyTime = journeyLength / travelSpeed;
        float fractionOfJourney = 0f;
        
        while (fractionOfJourney < 1f)
        {
            // Update the line on each frame
            UpdateLine();
            
            // Calculate progress
            fractionOfJourney = (Time.time - startTime) / journeyTime;
            
            // Move along path
            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);
            
            // Add arc
            float parabola = 4 * fractionOfJourney * (1 - fractionOfJourney);
            currentPos.y += parabola * arcHeight;
            
            // Update position
            transform.position = currentPos;
            
            yield return null;
        }
        
        // Ensure we land at target position
        transform.position = targetPosition;
        basePosition = targetPosition;
        
        // Start bobbing in water
        StartCoroutine(BobbingEffect());
        
        // Invoke callback with position
        if (onFishingStart != null)
        {
            onFishingStart.Invoke(targetPosition);
        }
    }
    
    private IEnumerator BobbingEffect()
    {
        float startTime = Time.time;
        
        while (true)
        {
            // Update the line on each frame
            UpdateLine();
            
            // Calculate bobbing motion
            float bobbingOffset = Mathf.Sin((Time.time - startTime) * bobFrequency) * bobAmplitude;
            transform.position = basePosition + new Vector3(0, bobbingOffset, 0);
            
            yield return null;
        }
    }
    
    // Update is called once per frame
    private void Update()
    {
        // Update line in Update too, in case the source moves
        UpdateLine();
    }
    
    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
