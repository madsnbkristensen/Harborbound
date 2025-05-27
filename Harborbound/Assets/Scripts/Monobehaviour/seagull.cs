using UnityEngine;

public class SeagullMover : MonoBehaviour
{
    public Vector3 startOffset = new Vector3(-10f, 4f, 5f);
    public Vector3 endOffset = new Vector3(10f, 4f, 5f);
    public float waveFrequency = 2f;
    public float waveAmplitude = 0.5f;
    [Range(0f, 1f)]
    public float volume = 0.5f; // Volume control
    private float flightDuration = 10f;
    private float timer = 0f;
    private float delayBeforeNextFlight = 0f;
    private bool flying = false;
    private AudioSource audioSource;
    private Camera mainCamera;

    // Store the actual positions for the current flight (smooth interpolation)
    private Vector3 flightStartPos;
    private Vector3 flightEndPos;
    private float flightBaseY;

    void Start()
    {
        mainCamera = Camera.main;
        audioSource = GetComponent<AudioSource>();

        // Set initial volume
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }

        StartNextFlightDelay();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (flying)
        {
            float t = timer / flightDuration;
            if (t >= 1f)
            {
                flying = false;
                timer = 0f;
                StartNextFlightDelay();
            }
            else
            {
                // Use the fixed flight path calculated when flight started
                float x = Mathf.Lerp(flightStartPos.x, flightEndPos.x, t);
                float z = Mathf.Lerp(flightStartPos.z, flightEndPos.z, t);
                float bobbing = Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;
                transform.position = new Vector3(x, flightBaseY + bobbing, z);
            }
        }
        else
        {
            // When not flying, stay at start position relative to camera
            if (mainCamera != null)
            {
                transform.position = mainCamera.transform.position + mainCamera.transform.TransformDirection(startOffset);
            }

            if (timer >= delayBeforeNextFlight)
            {
                StartFlight();
            }
        }
    }

    void StartFlight()
    {
        // Calculate the flight path once at the start of flight based on current camera position
        if (mainCamera != null)
        {
            flightStartPos = mainCamera.transform.position + mainCamera.transform.TransformDirection(startOffset);
            flightEndPos = mainCamera.transform.position + mainCamera.transform.TransformDirection(endOffset);
        }
        else
        {
            flightStartPos = startOffset;
            flightEndPos = endOffset;
        }

        flightBaseY = flightStartPos.y;

        flying = true;
        timer = 0f;

        // Play sound when flight begins
        if (audioSource != null)
        {
            audioSource.volume = volume; // Update volume each time it plays
            audioSource.Play();
        }
    }

    void StartNextFlightDelay()
    {
        delayBeforeNextFlight = Random.Range(50f, 100f);
    }
}
