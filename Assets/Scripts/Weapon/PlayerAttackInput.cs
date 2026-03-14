using UnityEngine;

public class PlayerAttackInput : MonoBehaviour
{
    private WeaponManager weaponManager;

    private void Awake()
    {
        // 같은 오브젝트에 붙어있는 WeaponManager를 가져옵니다.
        weaponManager = GetComponent<WeaponManager>();
    }

    private void Update()
    {
        // 1. A키 입력을 감지
        if (Input.GetKeyDown(KeyCode.A))
        {
            ExecuteAttack();
        }
    }

    private void ExecuteAttack()
    {
        if (weaponManager == null) return;

        // 2. 마우스 위치를 월드 좌표로 변환
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 3. 플레이어 위치에서 마우스 방향 계산 (2D이므로 Z축은 무시)
        Vector2 attackDirection = (Vector2)(mouseWorldPos - transform.position).normalized;

        // 4. WeaponManager에게 공격 명령 전달!
        weaponManager.OnAttack(attackDirection);
    }
}