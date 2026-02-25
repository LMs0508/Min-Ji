using Game.Core;
using UnityEngine;

public class SwiftnessEarthEnhancer : MonoBehaviour, ISkillElementEnhancer
{
    public ElementType TargetElement => ElementType.Earth;
    public int bonusArmor = 10;

    public void OnStart(GameObject owner)
    {
        // 방어력 스탯 증가
    }
    public void OnUpdate(GameObject owner) { }
    public void OnEnd(GameObject owner)
    {
        // 방어력 스탯 원복
    }
}