using UnityEngine;

public class SmartPortal : MonoBehaviour
{
    [Header("설정")]
    public Transform destination;

    [Tooltip("이 포탈을 이용할 수 있는 레이어들을 선택하세요.")]
    public LayerMask targetLayers; // 인스펙터에서 체크박스로 선택 가능

    private bool isLocked = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Tag 확인
        // 2. 비트 연산을 통해 충돌한 오브젝트의 레이어가 targetLayers에 포함되는지 확인
        // 3. 잠금 상태 확인
        if (collision.CompareTag("Player") && IsLayerInMask(collision.gameObject.layer, targetLayers) && !isLocked)
        {
            SmartPortal targetPortal = destination.GetComponentInParent<SmartPortal>();
            if (targetPortal != null)
            {
                targetPortal.LockPortal();
            }

            collision.transform.position = destination.position;
            Debug.Log($"{collision.gameObject.name} 이동 완료 (Layer: {collision.gameObject.layer})");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isLocked = false;
        }
    }

    public void LockPortal() => isLocked = true;

    // 레이어가 마스크에 포함되어 있는지 검사하는 함수
    private bool IsLayerInMask(int layer, LayerMask mask)
    {
        // 수학적 원리: $1 << layer$는 해당 레이어의 비트 위치를 나타냄
        return (mask.value & (1 << layer)) != 0;
    }
}