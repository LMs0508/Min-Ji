using UnityEngine;

/// <summary>
/// 플레이어의 모든 자식 스프라이트들의 소팅 레이어를 한꺼번에 동기화하는 스크립트입니다.
/// 계단 이용 시 방향 전환(비활성 오브젝트 활성화)에 따른 레이어 누락 문제를 해결합니다.
/// </summary>
public class PlayerLayerSync : MonoBehaviour
{
    /// <summary>
    /// 플레이어 본체 및 모든 자식(비활성화 포함)의 소팅 레이어 이름을 변경합니다.
    /// </summary>
    /// <param name="newLayerName">변경할 Sorting Layer의 이름</param>
    public void UpdateAllSortingLayers(string newLayerName)
    {
        // GetComponentsInChildren<T>(true) 의 true 인자가 중요합니다.
        // 현재 꺼져있는(비활성화된) walk_front, walk_back 등의 오브젝트도 모두 가져옵니다.
        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer sr in srs)
        {
            if (sr != null)
            {
                sr.sortingLayerName = newLayerName;
            }
        }

        // 선택 사항: 디버그 로그로 변경 확인 (개발 완료 후 주석 처리 가능)
        // Debug.Log($"[LayerSync] 모든 스프라이트의 Sorting Layer를 '{newLayerName}'으로 변경했습니다.");
    }
}