using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("이동할 목적지 오브젝트")]
    public Transform destination;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 플레이어의 위치를 목적지의 위치로 즉시 변경
            collision.transform.position = destination.position;

            Debug.Log("같은 씬 내 텔레포트 완료!");
        }
    }
}