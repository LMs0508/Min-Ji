using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData itemData;
    private bool isPlayerNearby;

    private void Update()
    {

        if (isPlayerNearby && Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("G 키 눌림!"); // 키 입력이 먹히는지 확인
            HandlePickup();
        }
        //if (isPlayerNearby && Input.GetKeyDown(KeyCode.G))
        //{
        //    HandlePickup();
        //}
    }

    private void HandlePickup()
    {
        switch (itemData.itemType)
        {
            case ItemType.Melee:
            case ItemType.Magic:
            case ItemType.Ranged:
                var weaponManager = FindFirstObjectByType<WeaponManager>();

                if (weaponManager != null)
                {
                    if (itemData is WeaponData weaponData)
                    {
                        weaponManager.EquipWeapon(weaponData);
                        Destroy(gameObject);
                    }
                }
                else
                {
                    Debug.LogError("플레이어에게 WeaponManager가 붙어있는지 확인하세요!");
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