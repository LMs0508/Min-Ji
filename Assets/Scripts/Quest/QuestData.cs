using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Playables;

public enum QuestType
{
    ItemConsume,    // 아이템 소비
    MonsterHunt,    // 몬스터 사냥
    ItemCollect,    // 아이템 수집
    ActivateObject, // 오브젝트 활성화
    TimeLimit       // 시간 제한
}

[System.Serializable]
public class QuestObjective // [추가] 개별 목표 클래스
{
    public QuestType type;
    public ItemData targetItem; // 수집형일 경우 필수
    public string targetID;
    public int targetAmount;
    public int currentAmount;
    public bool isReached;      // 이 목표를 달성했는지 여부
}

[System.Serializable]
public class QuestReward // [추가] 개별 보상 클래스
{
    public ItemData rewardItem;
    public int rewardAmount = 1;
}

[System.Serializable]
public class QuestData
{
    public string questTitle;
    [TextArea] public string questDescription;

    [Header("Quest Dialogues")] // [추가] 이 퀘스트 전용 대사들
    [TextArea(2, 4)] public string[] startLines;      // 퀘스트 수락 시
    [TextArea(2, 4)] public string[] processingLines; // 진행 중일 때
    [TextArea(2, 4)] public string[] completedLines;  // 완료 보고 시

    [Header("Quest Goals")]
    public QuestObjective[] objectives;

    [Header("Quest Options")]
    public bool StealItem = true;

    [Header("Status")]
    public bool isAccepted = false;
    public bool isCompleted = false;
    public bool isFinished = false;

    [Header("Rewards")]
    public QuestReward[] rewards;

    [Header("Timeline Settings")]
    // 시작 연출 설정
    public PlayableAsset startCutscene;
    public string startTimelinePlayer = "TimelinePlayer1"; 

    // 중간 연출 설정
    public PlayableAsset midCutscene;
    public int midTargetAmount = 3;
    public string midTimelinePlayer = "TimelinePlayer2"; 

    // 완료 연출 설정
    public PlayableAsset completeCutscene;
    public string completeTimelinePlayer = "TimelinePlayer3";

    [HideInInspector] public bool playedMid = false; // 연출 중복 방지용
}