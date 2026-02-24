using UnityEngine;

public class SkillItemPickup : MonoBehaviour
{
    public GameObject skillPrefab;   // 설치될 스킬 프리팹
    public GameObject pickupPrefab;  // 다시 드롭될 때 생성될 아이템 프리팹 (자기 자신 프리팹)

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var slots = other.GetComponent<SkillSlotsPrefab>();
        if (slots == null) return;

        var slotUI = FindObjectOfType<SlotSelectUI>(true);
        if (slotUI != null)
        {
            // 여기서 세 번째 인자로 'pickupPrefab'을 넘겨줍니다.
            slotUI.Open(slots, skillPrefab, pickupPrefab);

            // 이제 파괴되어도 pickupPrefab은 프로젝트의 프리팹이므로 사라지지 않습니다.
            Destroy(gameObject);
        }
    }
}