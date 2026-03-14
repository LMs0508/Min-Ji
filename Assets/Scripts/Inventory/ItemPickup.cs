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
                    // [체크] itemData가 진짜 WeaponData인지 강제로 로그를 찍어봅니다.
                    WeaponData wData = itemData as WeaponData;

                    if (wData != null)
                    {
                        weaponManager.EquipWeapon(wData);
                        Debug.Log($"<color=cyan>{wData.itemName}</color> 장착 성공!");
                        Destroy(gameObject);
                    }
                    else
                    {
                        // 만약 이 로그가 뜬다면, 롱소드 에셋이 WeaponData 스크립트 기반이 아니라는 뜻입니다.
                        Debug.LogError($"{itemData.itemName}은 ItemType은 무기지만, 실제 데이터는 WeaponData가 아닙니다!");
                    }
                }
                else
                {
                    Debug.LogError("플레이어에게 WeaponManager가 없습니다!");
                }
                break;

            case ItemType.Consumable:
            case ItemType.Quest:
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