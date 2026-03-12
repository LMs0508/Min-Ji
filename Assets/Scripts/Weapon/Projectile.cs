// 투사체 프리팹에 붙일 스크립트 예시
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 startPos;
    private float range;

    public void Setup(float speed, float range, Vector2 direction)
    {
        this.range = range;
        this.startPos = transform.position;
        GetComponent<Rigidbody2D>().linearVelocity = direction * speed;
    }

    private void Update()
    {
        // 시작 지점으로부터 이동한 거리 계산
        float distanceTraveled = Vector2.Distance(startPos, transform.position);

        if (distanceTraveled >= range)
        {
            Destroy(gameObject); // 설정된 사거리에 도달하면 소멸
        }
    }
}