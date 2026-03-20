using System;
using UnityEngine;

public class PlayerAttackInput : MonoBehaviour
{
    private WeaponManager weaponManager;
    private float chargeTimer = 0f;
    public event Action<float> OnChargeChanged;
    private void Awake()
    {
        // ���� ������Ʈ�� �پ��ִ� WeaponManager�� �����ɴϴ�.
        weaponManager = GetComponent<WeaponManager>();
    }

    private void Update()
    {
        if (weaponManager.currentWeapon == null) return;

        if (weaponManager.currentWeapon.canCharge)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                // A키를 누르는 즉시 1프레임 딜레이 없이 차징 시작 신호를 보냅니다!
                OnChargeChanged?.Invoke(0.01f);
            }

            if (Input.GetKey(KeyCode.A)) {
                chargeTimer += Time.deltaTime;
                float ratio = Mathf.Clamp01(chargeTimer / 1.0f);
                OnChargeChanged?.Invoke(ratio);
            }

            if (Input.GetKeyUp(KeyCode.A))
            {
                float ratio = Mathf.Clamp01(chargeTimer / 1.0f);
                ExecuteAttack(1.0f + (ratio * 0.5f)); // ��¡ ���� ����
                chargeTimer = 0f;
                OnChargeChanged?.Invoke(0f);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                ExecuteAttack(1.0f); // �Ϲ� ������ ���� 1.0 ����
            }
        }
    }

    private void ExecuteAttack(float multiplier)
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 attackDirection = (Vector2)(mouseWorldPos - transform.position).normalized;

        // WeaponManager�� OnAttack�� ���ڸ� �� �� �޵��� �����ؾ� �մϴ�.
        weaponManager.OnAttack(attackDirection, multiplier);
    }
}