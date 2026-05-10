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

                // [추가] 차징을 시작하는 순간 즉시 전투 태세(Combat Mode)로 전환합니다.
                PlayerVisualHandler visualHandler = transform.root.GetComponentInChildren<PlayerVisualHandler>();
                if (visualHandler != null)
                {
                    visualHandler.TriggerCombatMode();
                }
            }

            if (Input.GetKey(KeyCode.A)) {
                chargeTimer += Time.deltaTime;
                float ratio = Mathf.Clamp01(chargeTimer / 1.0f);
                OnChargeChanged?.Invoke(ratio);

                // [추가] 차징 중 마우스 커서 방향을 바라보게 만듭니다!
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 lookDir = (Vector2)(mouseWorldPos - transform.position).normalized;
                PlayerVisualHandler visualHandler = transform.root.GetComponentInChildren<PlayerVisualHandler>();
                
                if (visualHandler != null && visualHandler.bodyAnimator != null)
                {
                    visualHandler.bodyAnimator.SetFloat("MoveX", lookDir.x);
                    visualHandler.bodyAnimator.SetFloat("MoveY", lookDir.y);

                    // 마우스 방향에 맞춰 몸통 좌우 반전(플립) 처리
                    Vector3 scale = visualHandler.bodyAnimator.transform.localScale;
                    scale.x = Mathf.Abs(scale.x) * (lookDir.x < 0 ? -1f : 1f);
                    visualHandler.bodyAnimator.transform.localScale = scale;
                }
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