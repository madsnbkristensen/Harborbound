using UnityEngine;

public class SeagullMover : MonoBehaviour
{
    public Vector3 startPos = new Vector3(-10f, 4f, 0f);
    public Vector3 endPos = new Vector3(10f, 4f, 0f);
    public float waveFrequency = 2f;
    public float waveAmplitude = 0.5f;

    private float flightDuration = 10f;
    private float timer = 0f;
    private float delayBeforeNextFlight = 0f;
    private float baseY;
    private bool flying = false;

    private AudioSource audioSource; // 

    void Start()
    {
        transform.position = startPos;
        baseY = startPos.y;
        audioSource = GetComponent<AudioSource>(); // Get the AudioSource
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
                transform.position = startPos;
                timer = 0f;
                StartNextFlightDelay();
            }
            else
            {
                float x = Mathf.Lerp(startPos.x, endPos.x, t);
                float bobbing = Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;
                transform.position = new Vector3(x, baseY + bobbing, transform.position.z);
            }
        }
        else
        {
            if (timer >= delayBeforeNextFlight)
            {
                flying = true;
                timer = 0f;
                
                // 🔊 Play sound when flight begins
                if (audioSource != null)
                {
                    audioSource.Play();
                }
            }
        }
    }

    void StartNextFlightDelay()
    {
        delayBeforeNextFlight = Random.Range(50f, 100f);
    }
}