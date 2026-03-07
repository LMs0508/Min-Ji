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
        // 1. 기존에 표시되던 목록 오브젝트들을 모두 지웁니다.
        foreach (Transform child in questListParent) // QuestUI의 Transform
        {
            Destroy(child.gameObject);
        }

        // 2. 현재 진행 중인 퀘스트 개수만큼 프리팹을 생성합니다.
        foreach (var q in activeQuests)
        {
            GameObject item = Instantiate(questPrefab, questListParent);
            // 프리팹 내부의 텍스트 컴포넌트를 찾아 제목을 넣어줍니다.
            item.GetComponentInChildren<TMP_Text>().text = q.questTitle;
        }
    }
}