using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public List<QuestData> activeQuests = new List<QuestData>();
    public TMP_Text questListText; // 좌측 상단 UI 텍스트

    void Awake()
    {
        Instance = this;
    }

    public void AddQuest(QuestData newQuest)
    {
        if (!activeQuests.Contains(newQuest))
        {
            newQuest.isAccepted = true;
            activeQuests.Add(newQuest);
            UpdateQuestUI();
        }
    }

    public void UpdateQuestUI()
    {
        string list = "<size=120%><b>[진행 중인 퀘스트]</b></size>\n";
        foreach (var q in activeQuests)
        {
            list += $"- {q.questTitle}\n";
        }
        questListText.text = list;
    }
}