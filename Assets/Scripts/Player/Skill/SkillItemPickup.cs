using UnityEngine;

public class SkillItemPickup : MonoBehaviour
{
    public GameObject skillPrefab;   // 설치될 스킬 프리팹
    public GameObject pickupPrefab;  // 다시 드롭될 때 생성될 아이템 프리팹

    private bool isPlayerInRange = false; // 플레이어가 범위 안에 있는지 확인
    private SkillSlotsPrefab playerSlots; // 감지된 플레이어의 슬롯 컴포넌트 저장

    private void Update()
    {
        // 1. 플레이어가 근처에 있고 + G 키를 눌렀을 때만 실행
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.G))
        {
            TryPickup();
        }
    }

    private void TryPickup()
    {
        var slotUI = FindObjectOfType<SlotSelectUI>(true);
        if (slotUI != null && playerSlots != null)
        {
            // UI를 열어 슬롯을 선택하게 함
            slotUI.Open(playerSlots, skillPrefab, pickupPrefab);

            // 아이템을 획득했으므로 필드에서 제거
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerSlots = other.GetComponent<SkillSlotsPrefab>();

            // 유저에게 안내 메시지 (콘솔창 확인용)
            Debug.Log("'G' 키를 눌러 스킬 획득");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerSlots = null;
        }
    }
}