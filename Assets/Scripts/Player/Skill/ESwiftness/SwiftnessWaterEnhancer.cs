using Game.Core;
using UnityEngine;

public class SwiftnessWaterEnhancer : MonoBehaviour, ISkillElementEnhancer
{
    public ElementType TargetElement => ElementType.Water;

    public void OnStart(GameObject owner)
    {
        // 현재 걸린 슬로우나 화상 등 디버프 제거
        Debug.Log("물 정화: 모든 디버프를 씻어냅니다.");
    }
    public void OnUpdate(GameObject owner) { }
    public void OnEnd(GameObject owner) { }
}