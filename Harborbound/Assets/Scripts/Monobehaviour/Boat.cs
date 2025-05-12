using UnityEngine;

public class Boat : MonoBehaviour
{
    public float speed = 3f;
    public float rotationSpeed = 100f;
    [Header("Boat Components")]
    [SerializeField] private Transform _wheelPosition;

    public void Move(Vector2 inputDirection)
    {
        // Only process input if there's actual input
        if (inputDirection.magnitude > 0.1f)
        {
            // Set the boat's rotation based on input direction
            // This makes the boat point in the direction of movement
            float angle = Mathf.Atan2(inputDirection.y, inputDirection.x) * Mathf.Rad2Deg;

            // Move in the input direction
            Vector2 movement = inputDirection * speed * Time.deltaTime;
            transform.position += new Vector3(movement.x, movement.y, 0);
        }
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
}
