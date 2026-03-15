using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    private ShotgunWeapon parentWeapon;

    private void Awake()
    {
        // 부모 오브젝트에 있는 ShotgunWeapon 스크립트를 찾아옵니다.
        parentWeapon = GetComponentInParent<ShotgunWeapon>();
    }

    // 애니메이션 창에서 'Add Event'로 등록할 함수!
    public void TriggerFire()
    {
        if (parentWeapon != null)
        {
            // 부모의 실제 발사 함수를 실행하라고 찔러줍니다.
            parentWeapon.FireBullet();
        }
    }
}