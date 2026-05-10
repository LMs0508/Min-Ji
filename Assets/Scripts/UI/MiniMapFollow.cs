using UnityEngine;

public class MiniMapFollow : MonoBehaviour
{
    public Transform player;
    public float z = -10f;

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void LateUpdate()
    {
        if (!player) return;
        transform.position = new Vector3(player.position.x, player.position.y, z);
    }
}
