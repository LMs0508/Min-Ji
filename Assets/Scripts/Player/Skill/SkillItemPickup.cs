using UnityEngine;

public class SkillItemPickup : MonoBehaviour
{
    public GameObject skillPrefab;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var slots = other.GetComponent<SkillSlotsPrefab>();
        if (slots == null) return;

        var slotUI = FindObjectOfType<SlotSelectUI>(true); // 
        if (slotUI == null)
        {
            Debug.LogWarning("¾À¿¡ SlotSelectUI°¡ ¾ø¾î!");
            return;
        }

        slotUI.Open(slots, skillPrefab);
        Destroy(gameObject);
    }
}