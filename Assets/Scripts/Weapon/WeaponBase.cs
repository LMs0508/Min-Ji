using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public WeaponData data; // 공격력, 사거리 등 정보

    public abstract void ExecuteAttack(Vector2 direction); // 발사 로직 (상속받아 구현)
}