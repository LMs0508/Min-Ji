using UnityEngine;

public class MiniMapFollow : MonoBehaviour
{
    public Transform player;
    public float z = -10f;

    void LateUpdate()
    {
        if (!player) return;
        transform.position = new Vector3(player.position.x, player.position.y, z);
    }
}
