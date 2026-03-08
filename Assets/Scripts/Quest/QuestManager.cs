using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; // Image 컴포넌트 사용을 위해 필요
using TMPro;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public List<QuestData> activeQuests = new List<QuestData>();

    [Header("UI Settings")]
    public Transform questListParent;
    public GameObject questPrefab;

    [Header("Quest Icons")]
    public Sprite grayCheckIcon;   // 회색 체크 (진행 중)
    public Sprite greenCheckIcon;  // 초록 체크 (완료)

    void Awake()
    {
        Instance = this;
    }

    public void AddQuest(QuestData newQuest)
    {
        if (!activeQuests.Contains(newQuest))
        {
            newQuest.isAccepted = true;
            newQuest.isCompleted = false; // 새로 받은 퀘스트는 미완료 상태
            activeQuests.Add(newQuest);
            UpdateQuestUI();
        }
    }

    // 퀘스트 완료 처리용 함수 (예: 몬스터를 다 잡았을 때 호출)
    public void CompleteQuest(string title)
    {
        foreach (var q in activeQuests)
        {
            if (q.questTitle == title)
            {
                q.isCompleted = true;
                UpdateQuestUI(); // 상태가 변했으니 UI 갱신
                break;
            }
        }
    }

    public void UpdateQuestUI()
    {
        foreach (Transform child in questListParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var q in activeQuests)
        {
            GameObject item = Instantiate(questPrefab, questListParent);

            // 1. 제목 설정
            TMP_Text title = item.GetComponentInChildren<TMP_Text>();
            if (title != null) title.text = q.questTitle;

            // 2. 아이콘 설정 (프리팹 안의 Image 컴포넌트를 찾음)
            Image iconImage = item.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImage != null)
            {
                // 완료 여부에 따라 아이콘 교체
                iconImage.sprite = q.isCompleted ? greenCheckIcon : grayCheckIcon;
            }
        }
    }
}