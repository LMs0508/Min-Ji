using UnityEngine;
using Game.Core; // ElementType을 사용하기 위함

public interface ISkillElementEnhancer
{
    // 이 컴포넌트가 어떤 원소에 반응하는지 알려줍니다.
    ElementType TargetElement { get; }

    // 스킬이 시작될 때 실행할 로직
    void OnStart(GameObject owner);

    // 스킬 지속 시간 동안 매 프레임 실행할 로직 (필요 없으면 비워둠)
    void OnUpdate(GameObject owner);

    // 스킬이 끝날 때 실행할 로직
    void OnEnd(GameObject owner);
}