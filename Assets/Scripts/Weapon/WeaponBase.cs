using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public WeaponData data; // 공격력, 사거리 등 정보

    public abstract void ExecuteAttack(Vector2 direction, float multiplier); // 발사 함수 (방향받아 처리)
    public virtual Transform GetFirePoint(Vector2 direction)
    {
        return transform; // 기본적으로 무기 자신의 Transform을 반환
    }
}