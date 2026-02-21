using UnityEngine;

public class StairLayerFollower : MonoBehaviour
{
    [Header("추적할 대상 (Player)")]
    public GameObject targetPlayer;

    private int lastLayer;

    void Start()
    {
        if (targetPlayer != null)
        {
            // 시작할 때의 플레이어 레이어를 기록
            lastLayer = targetPlayer.layer;
            this.gameObject.layer = lastLayer;
        }
    }

    void Update()
    {
        if (targetPlayer == null) return;

        // 플레이어의 레이어가 바뀌었는지 매 프레임 확인
        if (targetPlayer.layer != lastLayer)
        {
            // 바뀌었다면 계단(본인)의 레이어도 플레이어와 동일하게 변경
            lastLayer = targetPlayer.layer;
            this.gameObject.layer = lastLayer;

            Debug.Log($"계단 레이어가 플레이어를 따라 {lastLayer}번으로 변경되었습니다.");
        }
    }
}