using UnityEngine;

public class Follow_player : MonoBehaviour
{
    public Transform player;
    public bool shouldFollow = true;

    // find player by object and add
    private void Start()
    {
        if (player == null)
            player = FindFirstObjectByType<Player>().transform;
    }


    void LateUpdate()
    {
        if (player != null)
        {
            transform.position = player.transform.position + new Vector3(0, 1, -5);
        }
        else
        {
            Debug.LogWarning("Player not found! Please assign the player object in the inspector or ensure it exists in the scene.");
        }
    }
}
