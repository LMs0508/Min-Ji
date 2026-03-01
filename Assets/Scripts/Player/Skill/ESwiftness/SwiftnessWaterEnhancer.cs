using UnityEngine;
using Game.Core;
using Game.Player;

// 1. 스킬 발동 시 명령을 내리는 클래스
public class SwiftnessWaterEnhancer : MonoBehaviour, ISkillElementEnhancer
{
    public ElementType TargetElement => ElementType.Water;

    public void OnStart(GameObject owner)
    {
        // 플레이어에게 스택 추적기가 있는지 확인하고 없으면 추가
        WaterStackTracker tracker = owner.GetComponent<WaterStackTracker>();
        if (tracker == null)
        {
            tracker = owner.AddComponent<WaterStackTracker>();
        }

        // 스택 추가 명령
        tracker.AddStack();
    }

    public void OnUpdate(GameObject owner) { }
    public void OnEnd(GameObject owner) { }
}

// 2. 플레이어 몸에 붙어서 실제 스택 수치를 기억하는 클래스 (같은 파일에 작성)
public class WaterStackTracker : MonoBehaviour
{
    private int currentStacks = 0;
    private const int MaxStacks = 3;
    private PlayerDebuffManager debuffManager;

    private void Awake()
    {
        debuffManager = GetComponent<PlayerDebuffManager>();
    }

    public void AddStack()
    {
        currentStacks++;
        Debug.Log($"<color=cyan>[물 정화]</color> 스택 추가! ({currentStacks}/{MaxStacks})");

        if (currentStacks >= MaxStacks)
        {
            Purify();
            currentStacks = 0; // 정화 후 스택 초기화
        }
    }

    private void Purify()
    {
        if (debuffManager != null)
        {
            // DebuffManager에 우리가 만든 정화 함수 호출
            debuffManager.ResetAllDebuffs();
            Debug.Log("<color=blue>[물 정화]</color> 모든 디버프가 제거되었습니다!");
        }
    }
}