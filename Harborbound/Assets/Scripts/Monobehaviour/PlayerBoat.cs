using UnityEngine;

public class PlayerBoat : Boat
{
    [Header("Player Boat")]
    [SerializeField] private Player player;

    private void Start()
    {
        if (player == null)
            player = FindFirstObjectByType<Player>();
    }

}
