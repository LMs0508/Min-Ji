using UnityEngine;

public class PerfectPortal : MonoBehaviour
{
    [Header("이 포탈을 탔을 때 도착할 반대편 포탈")]
    public PerfectPortal destinationPortal;

    [Header("이 포탈 '위에' 서 있을 때 캐릭터의 설정")]
    public int myLayer = 0; // 이 층의 레이어 번호
    public string mySortingLayer = "Default"; // 이 층의 소팅 레이어 이름

    private static bool isProcessing = false; // 전역 잠금 (순간적인 중복 실행 방지)

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어이고, 다른 포탈 처리 중이 아닐 때만 실행
        if (collision.CompareTag("Player") && !isProcessing)
        {
            if (destinationPortal != null)
            {
                TeleportPlayer(collision.gameObject);
            }
        }
    }

    private void TeleportPlayer(GameObject player)
    {
        isProcessing = true; // 이동 시작! (잠금)

        // 1. 위치 이동: 반대편 포탈 위치로 뿅!
        player.transform.position = destinationPortal.transform.position;

        // 2. 레이어 변경: 반대편 포탈이 가진 '레이어 설정'을 나에게 적용
        player.layer = destinationPortal.myLayer;

        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = destinationPortal.mySortingLayer;
        }

        Debug.Log($"이동 완료: 레이어 {destinationPortal.myLayer}번 적용");

        // 3. 잠시 후 잠금 해제 (아주 짧은 시간 뒤에 다시 이동 가능하게)
        Invoke("ReleaseLock", 0.5f);
    }

    private void ReleaseLock()
    {
        isProcessing = false;
    }
}