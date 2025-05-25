using UnityEngine;

public class PlayerBoat : Boat
{
    [Header("Player Boat")]
    [SerializeField] private Player player;
    [SerializeField] private Transform _wheelPosition;
    private PlayerBoatSpriteController spriteController;

    private void Start()
    {
        if (player == null)
            player = FindFirstObjectByType<Player>();
        spriteController = GetComponent<PlayerBoatSpriteController>();
    }

    public override void Move(Vector2 inputDirection)
    {
        // Call the base class implementation first
        base.Move(inputDirection);

        // Update sprite direction
        if (spriteController != null && inputDirection.magnitude > 0.1f)
        {
            spriteController.UpdateDirection(inputDirection);
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
}