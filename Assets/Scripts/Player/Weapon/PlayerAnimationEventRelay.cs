using UnityEngine;

public class PlayerAnimationEventRelay : MonoBehaviour
{
    // 애니메이션 이벤트에서 'TriggerFire'를 호출하면 이 함수가 실행됩니다!
    public void TriggerFire()
    {
        WeaponManager wm = GetComponentInParent<WeaponManager>();
        
        if (wm != null)
        {
            // 무기에게 발사 명령 전달!
            wm.BroadcastMessage("FireBullet", SendMessageOptions.DontRequireReceiver);
        }
    }
}