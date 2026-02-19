using UnityEngine;

public interface ISkill
{
    //void Use(GameObject owner);
    Sprite Icon { get; }
    bool TryUse(GameObject owner);   // 성공하면 true
    float Cooldown { get; }          // 쿨타임 총 시간
    float CooldownRemaining { get; } // 남은 시간
}