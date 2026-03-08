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

    void Update()
    {
        // 수락된 퀘스트 중 시간 제한이 있는 것들 체크
        foreach (var q in activeQuests)
        {
            if (q.isAccepted && !q.isCompleted && q.type == QuestType.TimeLimit)
            {
                // targetAmount를 초(Second) 단위로 사용 (예: 60초)
                // 시간 안에 목표를 달성해야 하는 로직은 별도로 짜야 하지만, 
                // 여기서는 단순 생존 시간 퀘스트 예시입니다.
            }
        }
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
    //public void ProgressQuest(QuestType type, string id, int amount = 1)
    //{
    //    foreach (var q in activeQuests)
    //    {
    //        // 1. 수락 상태고 2. 아직 미완료며 3. 퀘스트 타입과 ID가 일치하는지 확인
    //        if (q.isAccepted && !q.isCompleted && q.type == type && q.targetID == id)
    //        {
    //            q.currentAmount += amount;

    //            // 목표 달성 시
    //            if (q.currentAmount >= q.targetAmount)
    //            {
    //                q.currentAmount = q.targetAmount; // 초과 방지
    //                q.isCompleted = true;
    //                Debug.Log($"퀘스트 완료: {q.questTitle}");
    //            }
    //            UpdateQuestUI();
    //        }
    //    }
    //}

    public void ProgressQuest(QuestType type, string id, int amount = 1)
    {
        Debug.Log($"퀘스트 체크 중: {type} / {id}"); // 호출이 되는지 확인
        foreach (var q in activeQuests)
        {
            if (q.isAccepted && !q.isCompleted && q.type == type && q.targetID == id)
            {
                q.currentAmount += amount;
                Debug.Log($"{q.questTitle} 진행도 상승! : {q.currentAmount}/{q.targetAmount}");
                UpdateQuestUI();
            }

            if (q.currentAmount >= q.targetAmount)
            {
                q.isCompleted = true;

                // 여기에 성공 시 실행하고 싶은 코드를 추가하세요!
                Debug.Log("퀘스트 성공! 보상을 지급합니다.");
                // 예: GoldManager.Instance.AddGold(100); 
                // 이제 데이터가 true가 되었으므로 UpdateUI를 부르면 초록 체크가 뜹니다.
                UpdateQuestUI();
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
            item.GetComponentInChildren<TMP_Text>().text = $"{q.questTitle} ({q.currentAmount}/{q.targetAmount})";
            if (iconImage != null)
            {
                // 완료 여부에 따라 아이콘 교체
                iconImage.sprite = q.isCompleted ? greenCheckIcon : grayCheckIcon;
            }
        }
    }
}