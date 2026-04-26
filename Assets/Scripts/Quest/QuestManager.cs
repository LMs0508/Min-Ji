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

            // [�߰�] ���� ����
            if (newQuest.startCutscene != null) 
                PlayTimeline(newQuest.startCutscene, newQuest.startTimelinePlayer);

            UpdateQuestUI();
            OnQuestListChanged?.Invoke();
        }
    }

    // [����] ���� ī���� ���� ���� + ���� �߰�
    public void CheckQuestCompletion(QuestData q)
    {
        bool allObjectivesDone = true;

        foreach (var obj in q.objectives)
        {
            // ������ ������ �ǽð� �κ��丮 Ȯ�� ���� (���� �״��)
            if (obj.type == QuestType.ItemCollect && obj.targetItem != null)
            {
                obj.currentAmount = InventoryManager.Instance.GetItemTotalCount(obj.targetItem);
            }

            // [�߰�] �߰� ���� ����
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

        // [�߰�] �Ϸ� ���� ����
        if (!wasCompleted && q.isCompleted && q.completeCutscene != null)
        {
            PlayTimeline(q.completeCutscene, q.completeTimelinePlayer);
        }

        if (wasCompleted != q.isCompleted)
        {
            OnQuestListChanged?.Invoke();
        }
    }

    // [����] ���� ����/��� ���� ���� ����
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

    // [����] UI ������Ʈ �� �ǽð� üũ ���� (���� �״��)
    public void UpdateQuestUI()
    {
        if (questListParent == null) return;

        foreach (Transform child in questListParent) Destroy(child.gameObject);

        foreach (var q in activeQuests)
        {
            // UI�� �׸� ������ ��ǥ�� �ٽ� üũ�ؼ� ī��Ʈ�� ������ (�̰� ��������)
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

        // NPC ������ ������Ʈ
        foreach (var npc in FindObjectsByType<NPCDialogue>(FindObjectsSortMode.None))
        {
            npc.UpdateQuestIcon();
        }
    }

    // [�߰�] Ÿ�Ӷ��� ��� ���� �Լ�
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
            OnQuestListChanged?.Invoke();
        }
    }
}