using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Playables;

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
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddQuest(QuestData newQuest)
    {
        if (!activeQuests.Contains(newQuest))
        {
            newQuest.isAccepted = true;
            activeQuests.Add(newQuest);

            // [추가] 시작 연출
            if (newQuest.startCutscene != null) 
                PlayTimeline(newQuest.startCutscene, newQuest.startTimelinePlayer);

            UpdateQuestUI();
            OnQuestListChanged?.Invoke();
        }
    }

    // [복구] 기존 카운팅 로직 유지 + 연출 추가
    public void CheckQuestCompletion(QuestData q)
    {
        bool allObjectivesDone = true;

        foreach (var obj in q.objectives)
        {
            // 수집형 아이템 실시간 인벤토리 확인 로직 (기존 그대로)
            if (obj.type == QuestType.ItemCollect && obj.targetItem != null)
            {
                obj.currentAmount = InventoryManager.Instance.GetItemTotalCount(obj.targetItem);
            }

            // [추가] 중간 연출 로직
            if (q.midCutscene != null && obj.currentAmount >= q.midTargetAmount && !q.playedMid)
            {
                PlayTimeline(q.midCutscene, q.midTimelinePlayer);
                q.playedMid = true;
            }

            if (obj.currentAmount < obj.targetAmount)
            {
                allObjectivesDone = false;
            }
        }

        bool wasCompleted = q.isCompleted;
        q.isCompleted = allObjectivesDone;

        // [추가] 완료 연출 로직
        if (!wasCompleted && q.isCompleted && q.completeCutscene != null)
        {
            PlayTimeline(q.completeCutscene, q.completeTimelinePlayer);
        }

        if (wasCompleted != q.isCompleted)
        {
            OnQuestListChanged?.Invoke();
        }
    }

    // [복구] 기존 수집/사냥 진행 로직 유지
    public void ProgressQuest(QuestType type, string id, int amount = 1)
    {
        foreach (var q in activeQuests)
        {
            if (q.isAccepted && !q.isCompleted)
            {
                bool progressMade = false;
                foreach (var obj in q.objectives)
                {
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

    // [복구] UI 업데이트 시 실시간 체크 로직 (기존 그대로)
    public void UpdateQuestUI()
    {
        if (questListParent == null) return;

        foreach (Transform child in questListParent) Destroy(child.gameObject);

        foreach (var q in activeQuests)
        {
            // UI를 그릴 때마다 목표를 다시 체크해서 카운트를 갱신함 (이게 빠졌었음)
            CheckQuestCompletion(q);

            GameObject item = Instantiate(questPrefab, questListParent);
            TMP_Text title = item.GetComponentInChildren<TMP_Text>();

            if (title != null)
            {
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
            if (iconImage != null) iconImage.sprite = q.isCompleted ? greenCheckIcon : grayCheckIcon;
        }

        // NPC 아이콘 업데이트
        foreach (var npc in FindObjectsByType<NPCDialogue>(FindObjectsSortMode.None))
        {
            npc.UpdateQuestIcon();
        }
    }

    // [추가] 타임라인 재생 공용 함수
    private void PlayTimeline(PlayableAsset asset, string playerName)
    {
        if (asset == null) return;
        GameObject playerObj = GameObject.Find(playerName);
        if (playerObj != null)
        {
            PlayableDirector director = playerObj.GetComponent<PlayableDirector>();
            if (director != null)
            {
                director.playableAsset = asset;
                director.Play();
            }
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