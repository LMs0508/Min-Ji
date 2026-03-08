using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; // Image 컴포넌트 사용을 위해 필요
using TMPro;


public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public List<QuestData> activeQuests = new List<QuestData>();

    [Header("Global NPC Icon Settings")]
    public GameObject iconPrefab;   // FloatingIcon이 붙어있는 프리팹
    public Sprite canStartIcon;     // 물음표
    public Sprite inProgressIcon;   // 펼쳐진 책
    public Sprite canCompleteIcon;  // 덮힌 책 

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
            // [추가] 수집형(ItemCollect) 퀘스트라면 인벤토리 실제 개수와 동기화
            if (q.type == QuestType.ItemCollect && q.targetItem != null)
            {
                q.currentAmount = InventoryManager.Instance.GetItemTotalCount(q.targetItem);

                // 개수가 부족해지면 다시 미완료 상태로 변경
                if (q.currentAmount < q.targetAmount)
                {
                    q.isCompleted = false;
                }
                else
                {
                    q.isCompleted = true;
                }
            }

            GameObject item = Instantiate(questPrefab, questListParent);
            TMP_Text title = item.GetComponentInChildren<TMP_Text>();

            // 실시간 개수 반영 (예: 2/3)
            if (title != null)
                title.text = $"{q.questTitle} ({q.currentAmount}/{q.targetAmount})";

            Image iconImage = item.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = q.isCompleted ? greenCheckIcon : grayCheckIcon;
            }
        }

        NPCDialogue[] allNPCs = Object.FindObjectsByType<NPCDialogue>(FindObjectsSortMode.None);
        foreach (var npc in allNPCs)
        {
            npc.UpdateQuestIcon();
        }
    }

    public void RemoveQuest(QuestData quest)
    {
        if (activeQuests.Contains(quest))
        {
            activeQuests.Remove(quest);
            UpdateQuestUI(); // UI 갱신해서 목록에서 삭제
        }
    }
}