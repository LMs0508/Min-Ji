using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData itemData;
    private bool isPlayerNearby;

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.G))
        {
            HandlePickup();
        }
    }

    private void HandlePickup()
    {
        switch (itemData.itemType)
        {
            case ItemType.Melee:
            case ItemType.Magic:
            case ItemType.Ranged:
                // 플레이어에게서 WeaponManager를 찾습니다.
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                var weaponManager = player?.GetComponent<WeaponManager>();

                if (weaponManager != null)
                {
                    // WeaponData로 형변환하여 전달 (아이템 데이터가 무기 데이터라면 가능)
                    if (itemData is WeaponData weaponData)
                    {
                        weaponManager.EquipWeapon(weaponData);
                        Destroy(gameObject); // 주운 무기 오브젝트 제거
                    }
                }
                break;

            case ItemType.Consumable:
            case ItemType.Quest:
                // 기존 인벤토리 로직 유지
                if (InventoryManager.Instance.AddItem(itemData))
                {
                    Destroy(gameObject);
                }
                break;
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            // 안내 문구를 띄우고 싶다면 여기에 추가
            Debug.Log("G 키를 눌러 아이템 획득");
        }
    }

    // 플레이어가 범위를 벗어났을 때
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }

}