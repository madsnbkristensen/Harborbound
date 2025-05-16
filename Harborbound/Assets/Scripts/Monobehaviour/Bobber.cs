using System.Collections;
using UnityEngine;

public class Bobber : MonoBehaviour
{
    // Existing fields
    private LineRenderer lineRenderer;
    private Transform sourceTransform;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float travelSpeed = 8f;
    private Vector3 basePosition;
    private float bobAmplitude = 0.1f;
    private float bobFrequency = 2f;
    private System.Action<Vector3> onFishingStart;

    // New fields for fishing spot detection
    private FishingSpot currentFishingSpot;
    private bool isInFishingSpot = false;
    private float detectionRadius = 0.5f; // How close bobber needs to be to count as "in" a spot

    public void Initialize(Vector3 start, Vector3 target, float speed, System.Action<Vector3> callback, Transform source = null)
    {
        startPosition = start;
        targetPosition = target;
        travelSpeed = speed;
        onFishingStart = callback;
        sourceTransform = source;

        SetupLineRenderer();
        transform.position = startPosition;
        StartCoroutine(MoveToDestination());
    }

    private void SetupLineRenderer()
    {
        // Existing line renderer setup code...
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.01f;

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material.color = new Color(0.8f, 0.8f, 0.8f, 0.8f);
    }

    private void UpdateLine()
    {
        // Existing line update code...
        if (lineRenderer == null) return;

        Vector3 sourcePos;

        if (sourceTransform != null)
        {
            sourcePos = sourceTransform.position + new Vector3(0.52f, 0.27f, 0);
        }
        else
        {
            sourcePos = startPosition;
        }

        lineRenderer.SetPosition(0, sourcePos);
        lineRenderer.SetPosition(1, transform.position);
    }

    private IEnumerator MoveToDestination()
    {
        // Existing movement code...
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float arcHeight = journeyLength * 0.4f;

        float startTime = Time.time;
        float journeyTime = journeyLength / travelSpeed;
        float fractionOfJourney = 0f;

        while (fractionOfJourney < 1f)
        {
            UpdateLine();

            fractionOfJourney = (Time.time - startTime) / journeyTime;

            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

            float parabola = 4 * fractionOfJourney * (1 - fractionOfJourney);
            currentPos.y += parabola * arcHeight;

            transform.position = currentPos;

            yield return null;
        }

        transform.position = targetPosition;
        basePosition = targetPosition;

        // New code: Check if we landed in a fishing spot
        CheckForFishingSpot();

        StartCoroutine(BobbingEffect());

        // Pass fishing spot info when invoking callback
        if (onFishingStart != null)
        {
            onFishingStart.Invoke(targetPosition);
        }
    }

    // New method to check if we're in a fishing spot
    private void CheckForFishingSpot()
    {
        // Find all fishing spots in scene
        FishingSpot[] spots = FindObjectsByType<FishingSpot>(FindObjectsSortMode.None);

        float closestDistance = float.MaxValue;
        FishingSpot closestSpot = null;

        foreach (FishingSpot spot in spots)
        {
            float distance = Vector2.Distance(transform.position, spot.transform.position);

            // Use the fishing spot's collider radius if available
            CircleCollider2D collider = spot.GetComponent<CircleCollider2D>();
            float spotRadius = collider != null ? collider.radius * spot.transform.localScale.x : detectionRadius;

            if (distance <= spotRadius && distance < closestDistance)
            {
                closestDistance = distance;
                closestSpot = spot;
            }
        }

        if (closestSpot != null)
        {
            currentFishingSpot = closestSpot;
            isInFishingSpot = true;
            Debug.Log($"Bobber landed in fishing spot with {currentFishingSpot.numberOfFish} fish (Zone {currentFishingSpot.fishingSpotZone})");
        }
        else
        {
            isInFishingSpot = false;
            Debug.Log("Bobber didn't land in a fishing spot");
        }
    }

    // Getter methods for FishingManager to use
    public bool IsInFishingSpot() { return isInFishingSpot; }
    public FishingSpot GetCurrentFishingSpot() { return currentFishingSpot; }
    public int GetCurrentZone()
    {
        return isInFishingSpot ? currentFishingSpot.fishingSpotZone :
            GetZoneFromPosition(transform.position);
    }

    // Fallback method to determine zone from position when not in a fishing spot
    private int GetZoneFromPosition(Vector3 position)
    {
        if (ZoneManager.Instance == null || ZoneManager.Instance.zones.Count == 0)
            return 1; // Default to zone 1

        Vector2 centerPoint = ZoneManager.Instance.centerPoint;
        float distance = Vector2.Distance(position, centerPoint);

        // Check each zone
        for (int i = 0; i < ZoneManager.Instance.zones.Count; i++)
        {
            Zone zone = ZoneManager.Instance.zones[i];
            if (distance >= zone.innerRadius && distance <= zone.outerRadius)
            {
                return i + 1; // Zones are 1-indexed
            }
        }

        return 1; // Default fallback
    }

    private IEnumerator BobbingEffect()
    {
        // Existing bobbing code...
        float startTime = Time.time;

        while (true)
        {
            UpdateLine();

            float bobbingOffset = Mathf.Sin((Time.time - startTime) * bobFrequency) * bobAmplitude;
            transform.position = basePosition + new Vector3(0, bobbingOffset, 0);

            yield return null;
        }
    }

    private void Update()
    {
        UpdateLine();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
