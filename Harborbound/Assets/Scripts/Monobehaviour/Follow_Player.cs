using UnityEngine;

public class Follow_player : MonoBehaviour
{
    public Transform player;
    // find player by object and add
    private void Start()
    {
        if (player == null)
            player = FindFirstObjectByType<Player>().transform;
    }


    void Update()
    {
        transform.position = player.transform.position + new Vector3(0, 1, -5);
    }
}
