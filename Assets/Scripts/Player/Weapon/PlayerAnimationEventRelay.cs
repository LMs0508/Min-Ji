using UnityEngine;

public class PlayerAnimationEventRelay : MonoBehaviour
{
    // 애니메이션 이벤트에서 'TriggerFire'를 호출하면 이 함수가 실행됩니다!
    public void TriggerFire()
    {
        WeaponManager wm = GetComponentInParent<WeaponManager>();
        
        if (wm != null)
        {
            // 하위의 모든 오브젝트에 방송(Broadcast)하지 않고 오직 현재 무기만 호출
            wm.FireCurrentWeapon();
        }
    }
}