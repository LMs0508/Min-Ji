using UnityEngine;

public class PlayerAttackInput : MonoBehaviour
{
    private WeaponManager weaponManager;
    private float chargeTimer = 0f;
    private void Awake()
    {
        // 같은 오브젝트에 붙어있는 WeaponManager를 가져옵니다.
        weaponManager = GetComponent<WeaponManager>();
    }

    private void Update()
    {
        if (weaponManager.currentWeapon == null) return;

        if (weaponManager.currentWeapon.canCharge)
        {
            if (Input.GetKey(KeyCode.A)) chargeTimer += Time.deltaTime;
            if (Input.GetKeyUp(KeyCode.A))
            {
                float ratio = Mathf.Clamp01(chargeTimer / 1.0f);
                ExecuteAttack(1.0f + (ratio * 0.5f)); // 차징 배율 전달
                chargeTimer = 0f;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                ExecuteAttack(1.0f); // 일반 무기는 배율 1.0 전달
            }
        }
    }

    private void ExecuteAttack(float multiplier)
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 attackDirection = (Vector2)(mouseWorldPos - transform.position).normalized;

        // WeaponManager의 OnAttack도 인자를 두 개 받도록 수정해야 합니다.
        weaponManager.OnAttack(attackDirection, multiplier);
    }
}