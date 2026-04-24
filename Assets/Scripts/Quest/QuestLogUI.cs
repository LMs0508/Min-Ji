using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class QuestLogUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject windowRoot;
    public Transform listParent;
    public GameObject questSlotPrefab;

    [Header("Detail Display")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI objectiveText;
    public Transform rewardParent;
    public GameObject rewardPrefab;
    
    [Header("UI Buttons")]
    public Button closeButton;

    private QuestData selectedQuest; // 현재 보고 있는 퀘스트를 기억

    void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(() => windowRoot.SetActive(false));
    }

    void Start()
    {
        // 퀘스트 매니저의 이벤트 구독
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestListChanged += RefreshQuestList;
    }

    void OnDestroy()
    {
        // 구독 해제
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestListChanged -= RefreshQuestList;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) ToggleWindow();
    }

    public void ToggleWindow()
    {
        bool isActive = !windowRoot.activeSelf;
        windowRoot.SetActive(isActive);

        if (isActive)
        {
            RefreshQuestList();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void RefreshQuestList()
    {
        foreach (Transform child in listParent) Destroy(child.gameObject);

        var activeQuests = QuestManager.Instance.activeQuests;

        foreach (var quest in activeQuests)
        {
            GameObject slot = Instantiate(questSlotPrefab, listParent);
            slot.GetComponentInChildren<TextMeshProUGUI>().text = quest.questTitle;
            
            slot.GetComponent<Button>().onClick.AddListener(() => {
                selectedQuest = quest;
                DisplayQuestDetail(quest);
            });
        }

        // 실시간 갱신 시 보던 내용 유지
        if (selectedQuest != null && activeQuests.Contains(selectedQuest))
            DisplayQuestDetail(selectedQuest);
        else if (activeQuests.Count > 0)
        {
            selectedQuest = activeQuests[0];
            DisplayQuestDetail(selectedQuest);
        }
    }

    public void DisplayQuestDetail(QuestData data)
    {
        if (data == null) return;

        titleText.text = data.questTitle ?? "제목 없음";
        descriptionText.text = data.questDescription ?? "설명이 없습니다.";

        string objInfo = "<color=#FFD700>[목표 정보]</color>\n";
        if (data.objectives != null)
        {
            foreach (var obj in data.objectives)
            {
                if (obj == null) continue;
                string color = (obj.currentAmount >= obj.targetAmount) ? "#00FF00" : "#FFFFFF";
                objInfo += $"<color={color}>- {obj.targetID} ({obj.currentAmount}/{obj.targetAmount})</color>\n";
            }
        }
        objectiveText.text = objInfo;

        if (rewardParent != null)
        {
            foreach (Transform child in rewardParent) Destroy(child.gameObject);
            if (data.rewards != null)
            {
                foreach (var reward in data.rewards)
                {
                    if (reward == null || reward.rewardItem == null) continue; 
                    GameObject rObj = Instantiate(rewardPrefab, rewardParent);
                    rObj.GetComponentInChildren<Image>().sprite = reward.rewardItem.icon;
                    rObj.GetComponentInChildren<TextMeshProUGUI>().text = $"x{reward.rewardAmount}";
                }
            }
        }
    }
}