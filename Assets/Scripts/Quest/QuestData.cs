using UnityEngine;

public enum QuestType { ItemConsume, MonsterHunt, ItemCollect, ActivateObject, TimeLimit }

[System.Serializable]
public class QuestData
{
    public string questTitle;
    [TextArea]
    public string questDescription;

    [Header("Quest Type & Target")]
    public QuestType type;          // 퀘스트 종류
    public string targetID;         // 목표 ID (아이템 이름, 몬스터 이름, 제단 이름 등)
    public int targetAmount;        // 목표 수량
    public int currentAmount;       // 현재 수량

    [Header("Status")]
    public bool isAccepted = false;
    public bool isCompleted = false;
}