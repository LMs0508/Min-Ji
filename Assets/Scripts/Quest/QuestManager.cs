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

            if (newQuest.startCutscene != null) 
                PlayTimeline(newQuest.startCutscene, newQuest.startTimelinePlayer);

            UpdateQuestUI();
            OnQuestListChanged?.Invoke();
        }
    }

    public void CheckQuestCompletion(QuestData q)
    {
        bool allObjectivesDone = true;
        foreach (var obj in q.objectives)
        {
            if (obj.type == QuestType.ItemCollect && obj.targetItem != null)
                obj.currentAmount = InventoryManager.Instance.GetItemTotalCount(obj.targetItem);

            if (q.midCutscene != null && obj.currentAmount >= q.midTargetAmount && !q.playedMid)
            {
                PlayTimeline(q.midCutscene, q.midTimelinePlayer);
                q.playedMid = true;
            }
            if (obj.currentAmount < obj.targetAmount) allObjectivesDone = false;
        }

        bool wasCompleted = q.isCompleted;
        q.isCompleted = allObjectivesDone;

        if (!wasCompleted && q.isCompleted && q.completeCutscene != null)
            PlayTimeline(q.completeCutscene, q.completeTimelinePlayer);

        // [추가] 자동 완료 설정이 되어있고 이제 막 목표를 달성했다면 즉시 완료 처리
        if (!wasCompleted && q.isCompleted && q.autoComplete)
            CompleteQuest(q);

        if (wasCompleted != q.isCompleted) OnQuestListChanged?.Invoke();
    }

    // [추가] 퀘스트를 완료 처리하고 보상을 지급하는 공용 함수
    public void CompleteQuest(QuestData q)
    {
        if (q == null || q.isFinished) return;

        // 아이템 회수
        if (q.StealItem)
        {
            foreach (var obj in q.objectives)
            {
                if (obj.type == QuestType.ItemCollect && obj.targetItem != null)
                    InventoryManager.Instance?.RemoveItem(obj.targetItem, obj.targetAmount);
            }
        }

        // 보상 지급
        if (q.rewards != null)
        {
            foreach (var reward in q.rewards)
            {
                if (reward.rewardItem != null)
                    InventoryManager.Instance?.AddItem(reward.rewardItem, reward.rewardAmount);
            }
        }

        q.isFinished = true;
        RemoveQuest(q);
    }

    public void ProgressQuest(QuestType type, string id, int amount = 1)
    {
        // 리스트 수정(삭제)에 대비하여 역순으로 순회
        for (int i = activeQuests.Count - 1; i >= 0; i--)
        {
            QuestData q = activeQuests[i];
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

    public void UpdateQuestUI()
    {
        if (questListParent == null) return;

        // 1. 상태 체크 (자동 완료 시 리스트가 변하므로 역순 체크)
        for (int i = activeQuests.Count - 1; i >= 0; i--)
        {
            CheckQuestCompletion(activeQuests[i]);
        }

        // 2. UI 갱신
        foreach (Transform child in questListParent) Destroy(child.gameObject);
        foreach (var q in activeQuests)
        {
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

        foreach (var npc in FindObjectsByType<NPCDialogue>(FindObjectsSortMode.None))
            npc.UpdateQuestIcon();
    }

    // [최종 수정] 컷신 매니저를 호출하여 중복 기능을 통합
    private void PlayTimeline(PlayableAsset asset, string playerName)
    {
        if (asset == null) return;
        GameObject playerObj = GameObject.Find(playerName);
        if (playerObj == null) return;

        PlayableDirector director = playerObj.GetComponent<PlayableDirector>();
        if (director == null) return;

        // 시네머신 트랙 바인딩 로직만 처리
        foreach (var output in asset.outputs)
        {
            if (output.sourceObject != null && output.sourceObject.GetType().Name == "CinemachineTrack")
            {
                GameObject mainCam = GameObject.FindWithTag("MainCamera");
                if (mainCam != null) director.SetGenericBinding(output.sourceObject, mainCam);
            }
        }

        // 실제 재생 및 조작 차단 관리는 CutsceneManager에게 일임 (ID는 빈값 처리)
        if (CutsceneManager.Instance != null)
        {
            CutsceneManager.Instance.PlayCutscene(director, asset, "");
        }
    }

    public void RemoveQuest(QuestData quest)
    {
        if (activeQuests.Contains(quest))
        {
            activeQuests.Remove(quest);
            UpdateQuestUI();
            OnQuestListChanged?.Invoke();
        }
    }
}