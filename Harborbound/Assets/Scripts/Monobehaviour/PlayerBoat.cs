using UnityEngine;

public class PlayerBoat : Boat
{
    [Header("Player Boat")]
    [SerializeField]
    private Player player;

    [SerializeField]
    private Transform _wheelPosition;
    private PlayerBoatSpriteController spriteController;

    [SerializeField]
    private EdgeCollider2D edgeCollider;

    // Add these as class variables
    private bool isCollidingWithRock = false;
    private Vector2 collisionNormal;

    // Add this as a class variable
    [SerializeField]
    private Vector2 colliderPivotOffset = Vector2.zero;

    private void Start()
    {
        if (player == null)
            player = FindFirstObjectByType<Player>();
        spriteController = GetComponent<PlayerBoatSpriteController>();

        // Set up proper collision
        SetupBoatCollision();
    }

    private void SetupBoatCollision()
    {
        // Ensure edge collider exists and is not a trigger
        if (edgeCollider == null)
        {
            edgeCollider = GetComponent<EdgeCollider2D>();
            if (edgeCollider == null)
            {
                edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
                Debug.Log("Added EdgeCollider2D to PlayerBoat");
            }
        }
        edgeCollider.isTrigger = false;

        // Add a Rigidbody2D with Kinematic type if it doesn't exist
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            Debug.Log("Added Rigidbody2D to PlayerBoat");
        }

        // Configure the Rigidbody2D as Kinematic so it doesn't fall
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true; // Ensure physics simulation is enabled
        rb.useFullKinematicContacts = true; // Important for kinematic-to-kinematic collisions
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Set the boat's layer (create "PlayerBoat" layer in Unity if needed)
        if (LayerMask.NameToLayer("Player") != -1)
            gameObject.layer = LayerMask.NameToLayer("Player");
    }

    public override void Move(Vector2 inputDirection)
    {
        // Only process input if there's actual input
        if (inputDirection.magnitude > 0.1f)
        {
            // Get the rigidbody component
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null && rb.simulated)
            {
                // If colliding with a rock, handle sliding behavior
                if (isCollidingWithRock)
                {
                    // Rather than reflecting, project the movement onto the surface
                    // This creates a sliding motion without jittering
                    Vector2 slideDirection = Vector2.Perpendicular(collisionNormal);

                    // Make sure we're sliding in the correct direction based on input
                    if (Vector2.Dot(slideDirection, inputDirection) < 0)
                        slideDirection *= -1;

                    // Calculate the slide component of the input
                    float slideAmount = Vector2.Dot(inputDirection, slideDirection);

                    // Create movement based on the slide component only
                    Vector2 movement = slideDirection * slideAmount * speed * 0.4f * Time.deltaTime;

                    // Apply a small push away from the rock to prevent sticking
                    movement += collisionNormal * 0.01f;

                    // Move the boat
                    rb.MovePosition(rb.position + movement);

                    // Debug visualization for sliding
                    Debug.DrawRay(transform.position, slideDirection * 2f, Color.blue, 0.1f);
                    Debug.DrawRay(transform.position, collisionNormal * 2f, Color.red, 0.1f);
                }
                else
                {
                    // Normal movement when not colliding
                    Vector2 movement = inputDirection * speed * Time.deltaTime;
                    rb.MovePosition(rb.position + movement);
                }

                // Update sprite direction and adjust collider
                if (spriteController != null)
                {
                    spriteController.UpdateDirection(inputDirection);

                    // Update collider orientation based on movement direction
                    UpdateColliderOrientation(inputDirection);
                }
            }
        }
    }

    // Public method to stop the engine sound (called from Player.StopDriving())
    public void StopEngine()
    {
        AudioManager.Instance.StopPlay(AudioManager.SoundType.Boat_Engine);
    }

    public Transform wheelPosition
    {
        get
        {
            // If wheel position isn't assigned, try to find it
            if (_wheelPosition == null)
            {
                // First try to find a child named "BoatWheel"
                Transform wheelChild = transform.Find("BoatWheel");
                if (wheelChild != null)
                {
                    _wheelPosition = wheelChild;
                    Debug.Log("Found BoatWheel child object");
                }
            }
            return _wheelPosition;
        }
    }

    void OnDrawGizmos()
    {
        if (wheelPosition != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(wheelPosition.position, 0.3f);
        }
    }

    // Cleanup when boat is destroyed or disabled
    private void OnDestroy()
    {
        StopEngine();
    }

    private void OnDisable()
    {
        StopEngine();
    }

    // draws gizmos for the edge collider
    private void OnDrawGizmosSelected()
    {
        if (edgeCollider != null && edgeCollider.points.Length > 0)
        {
            Gizmos.color = Color.red;
            Vector2[] points = edgeCollider.points;
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 worldPoint = transform.TransformPoint(points[i]);
                Gizmos.DrawSphere(worldPoint, 0.1f);
                if (i > 0)
                {
                    Vector2 prevWorldPoint = transform.TransformPoint(points[i - 1]);
                    Gizmos.DrawLine(prevWorldPoint, worldPoint);
                }
            }
        }
    }

    // Modify your collision methods to track collision state
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check for both rock layer and island tag
        if (
            collision.gameObject.layer == LayerMask.NameToLayer("Rocks")
            || collision.gameObject.CompareTag("Island")
        )
        {
            isCollidingWithRock = true;

            // Initialize the smoothed normal
            _smoothedNormal = collision.GetContact(0).normal;
            collisionNormal = _smoothedNormal;

            // Optional: Play collision sound with slight randomization to prevent sound repetition
            // if (AudioManager.Instance != null)
            //     AudioManager.Instance.PlayWithPitch(AudioManager.SoundType.Boat_Hit_Rock, Random.Range(0.95f, 1.05f));
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (
            collision.gameObject.layer == LayerMask.NameToLayer("Rocks")
            || collision.gameObject.CompareTag("Island")
        )
        {
            isCollidingWithRock = false;
        }
    }

    private Vector2 _smoothedNormal = Vector2.zero;
    private float _normalSmoothTime = 0.1f;

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (
            collision.gameObject.layer == LayerMask.NameToLayer("Rocks")
            || collision.gameObject.CompareTag("Island")
        )
        {
            // Smoothly update the collision normal to prevent jittering
            Vector2 targetNormal = collision.GetContact(0).normal;
            _smoothedNormal = Vector2.Lerp(
                _smoothedNormal,
                targetNormal,
                Time.deltaTime / _normalSmoothTime
            );
            collisionNormal = _smoothedNormal.normalized;
        }
    }

    // Add this method to rotate the collider based on movement direction
    private void UpdateColliderOrientation(Vector2 direction)
    {
        if (edgeCollider != null && direction.magnitude > 0.1f)
        {
            // Get the current points from the edge collider
            Vector2[] currentPoints = edgeCollider.points;

            // Create a reference to original points if we haven't stored them yet
            if (
                _originalColliderPoints == null
                || _originalColliderPoints.Length != currentPoints.Length
            )
            {
                _originalColliderPoints = new Vector2[currentPoints.Length];
                for (int i = 0; i < currentPoints.Length; i++)
                {
                    _originalColliderPoints[i] = currentPoints[i];
                }
            }

            // Only rotate for vertical movement (W or S keys)
            bool isVertical = Mathf.Abs(direction.y) > Mathf.Abs(direction.x);

            if (isVertical)
            {
                // Rotate collider for up movement
                RotateCollider(-90);
            }
            else
            {
                // For horizontal movement (A or D), keep original orientation
                RotateCollider(0);
            }

            // Debug visualization
            Debug.DrawRay(transform.position, direction.normalized * 2f, Color.green, 0.1f);
        }
    }

    // Store original collider points
    private Vector2[] _originalColliderPoints;

    // Helper method to rotate collider points by a specific angle
    private void RotateCollider(float angleDegrees)
    {
        if (_originalColliderPoints != null && edgeCollider != null)
        {
            // Create a new array to hold rotated points
            Vector2[] rotatedPoints = new Vector2[_originalColliderPoints.Length];

            // Find the center point of the collider and apply the manual offset
            Vector2 pivot = CalculateColliderCenter(_originalColliderPoints) + colliderPivotOffset;

            // Convert angle to radians
            float angleRad = angleDegrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angleRad);
            float sin = Mathf.Sin(angleRad);

            // Rotate each point around the pivot
            for (int i = 0; i < _originalColliderPoints.Length; i++)
            {
                // Translate point to origin (relative to pivot)
                Vector2 pointRelativeToPivot = _originalColliderPoints[i] - pivot;

                // Rotate around origin
                Vector2 rotated = new Vector2(
                    pointRelativeToPivot.x * cos - pointRelativeToPivot.y * sin,
                    pointRelativeToPivot.x * sin + pointRelativeToPivot.y * cos
                );

                // Translate back to original position
                rotatedPoints[i] = rotated + pivot;
            }

            // Apply rotated points to the edge collider
            edgeCollider.points = rotatedPoints;

            // Debug visualization of pivot point
            Debug.DrawLine(
                transform.position,
                transform.position + (Vector3)pivot,
                Color.yellow,
                0.1f
            );
        }
    }

    // Calculate the center of the collider points
    private Vector2 CalculateColliderCenter(Vector2[] points)
    {
        if (points == null || points.Length == 0)
            return Vector2.zero;

        Vector2 sum = Vector2.zero;
        foreach (Vector2 point in points)
        {
            sum += point;
        }

        return sum / points.Length;
    }
}
