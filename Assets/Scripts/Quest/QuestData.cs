using UnityEngine;

[System.Serializable]
public class QuestData
{
    public string questTitle;       // 퀘스트 제목
    [TextArea]
    public string questDescription; // 퀘스트 설명
    public bool isAccepted = false; // 수락 여부
    public bool isCompleted = false; // 완료 여부
}