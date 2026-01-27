using UnityEngine;

public class MiniMapFollow : MonoBehaviour
{
    public Transform player;

    void LateUpdate()
    {
        Vector3 pos = player.position;
        pos.y = transform.position.y; // 높이는 고정
        transform.position = pos;
    }
}
