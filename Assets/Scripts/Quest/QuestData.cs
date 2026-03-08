using UnityEngine;

// 1. 퀘스트 종류를 정의하는 열거형 (Enum)
public enum QuestType
{
    ItemConsume,    // 아이템 소비
    MonsterHunt,    // 몬스터 사냥
    ItemCollect,    // 아이템 수집
    ActivateObject, // 오브젝트 활성화
    TimeLimit       // 시간 제한
}

[System.Serializable]
public class QuestData
{
    public string questTitle;       // 퀘스트 제목
    [TextArea]
    public string questDescription; // 퀘스트 설명

    [Header("Quest Goal")]
    public QuestType type;          // 퀘스트 타입 (Enum)
    public ItemData targetItem;     // 목표 아이템 데이터 (인벤토리 확인용)
    public string targetID;         // 목표 ID (문자열 이름표)
    public int targetAmount;        // 목표 수량
    public int currentAmount;       // 현재 수량

    [Header("Quest Options")]
    public bool StealItem = true; // true면 가져가고, false면 확인만 함

    [Header("Status")]
    public bool isAccepted = false;  // 수락 여부
    public bool isCompleted = false; // 수치 달성 여부 (3/3 등)
    public bool isFinished = false;  // 최종 보상 수령 여부

    [Header("Reward")]
    public ItemData rewardItem;      // 보상 아이템
    public int rewardAmount = 1;     // 보상 개수
}