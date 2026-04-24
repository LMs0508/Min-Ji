using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public List<QuestData> activeQuests = new List<QuestData>();
    public event Action OnQuestListChanged;


    [Header("Global NPC Icon Settings")]
    public GameObject iconPrefab;
    public Sprite canStartIcon;
    public Sprite inProgressIcon;
    public Sprite canCompleteIcon;

    [Header("UI Settings")]
    public Transform questListParent;
    public GameObject questPrefab;

    [Header("Quest Icons")]
    public Sprite grayCheckIcon;
    public Sprite greenCheckIcon;

    [Header("Quest UI Colors")]
    public Color inProgressColor = Color.white;
    public Color completedColor = new Color(0.2f, 1f, 0.2f);

    void Awake()
    {
        Instance = this;
    }

    public void AddQuest(QuestData newQuest)
    {
        if (!activeQuests.Contains(newQuest))
        {
            newQuest.isAccepted = true;
            newQuest.isCompleted = false;
            activeQuests.Add(newQuest);
            UpdateQuestUI();
            OnQuestListChanged?.Invoke();
        }
    }

    // 모든 목표가 달성되었는지 체크하는 핵심 함수
    public void CheckQuestCompletion(QuestData q)
    {
        bool allObjectivesDone = true;

        foreach (var obj in q.objectives)
        {
            // 수집형 아이템은 실시간으로 인벤토리 개수 확인
            if (obj.type == QuestType.ItemCollect && obj.targetItem != null)
            {
                obj.currentAmount = InventoryManager.Instance.GetItemTotalCount(obj.targetItem);
            }

            if (obj.currentAmount < obj.targetAmount)
            {
                allObjectivesDone = false;
            }
        }

        bool wasCompleted = q.isCompleted;
        q.isCompleted = allObjectivesDone;
        if (wasCompleted != q.isCompleted)
        {
            OnQuestListChanged?.Invoke();
        }
    }

    public void ProgressQuest(QuestType type, string id, int amount = 1)
    {
        foreach (var q in activeQuests)
        {
            if (q.isAccepted && !q.isCompleted)
            {
                bool progressMade = false;
                foreach (var obj in q.objectives)
                {
                    // 타입과 ID가 일치하는 목표 수치 증가 (수집형 제외)
                    if (obj.type != QuestType.ItemCollect && obj.type == type && obj.targetID == id)
                    {
                        obj.currentAmount += amount;
                        if (obj.currentAmount > obj.targetAmount) obj.currentAmount = obj.targetAmount;
                        progressMade = true;
                    }
                }

                if (progressMade)
                {
                    CheckQuestCompletion(q);
                    UpdateQuestUI();
                    OnQuestListChanged?.Invoke();
                }
            }
        }
    }

    public void UpdateQuestUI()
    {
        // 기존 UI 항목 삭제
        foreach (Transform child in questListParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var q in activeQuests)
        {
            // UI 생성 전 상태 업데이트
            CheckQuestCompletion(q);

            GameObject item = Instantiate(questPrefab, questListParent);
            TMP_Text title = item.GetComponentInChildren<TMP_Text>();

            if (title != null)
            {
                // 여러 목표를 텍스트로 합치기
                string statusText = $"<b>{q.questTitle}</b>";
                foreach (var obj in q.objectives)
                {
                    string colorHex = (obj.currentAmount >= obj.targetAmount) ? "#32CD32" : "#FFFFFF";
                    statusText += $"\n<color={colorHex}>- {obj.targetID} ({obj.currentAmount}/{obj.targetAmount})</color>";
                }

                title.text = statusText;
                title.color = q.isCompleted ? completedColor : inProgressColor;
            }

            Image iconImage = item.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = q.isCompleted ? greenCheckIcon : grayCheckIcon;
            }
        }

        // 모든 NPC 아이콘 갱신
        NPCDialogue[] allNPCs =UnityEngine.Object.FindObjectsByType<NPCDialogue>(FindObjectsSortMode.None);
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
            UpdateQuestUI();
        }
    }
}