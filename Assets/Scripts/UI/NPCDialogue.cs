using UnityEngine;
using System.Collections.Generic;

public class NPCDialogue : MonoBehaviour
{
    private SpriteRenderer iconRenderer;
    public string npcName = "NPC";

    [Header("Quest System")]
    // 이제 단일 'quest' 변수는 삭제되었습니다. 인스펙터가 깨끗해질 거예요!
    public List<QuestData> questList = new List<QuestData>();
    private int currentQuestIndex = 0;

    [Header("Normal Dialogue")]
    [TextArea(2, 4)] public string[] normalLines; // 모든 퀘스트가 끝난 후 대사

    public KeyCode interactKey = KeyCode.Space;
    public float reInteractCooldown = 0.5f;
    private float nextInteractTime = 0f;
    private bool playerNear;

    private QuestData CurrentQuest => (currentQuestIndex < questList.Count) ? questList[currentQuestIndex] : null;

    void Start()
    {
        if (QuestManager.Instance != null && QuestManager.Instance.iconPrefab != null)
        {
            GameObject iconObj = Instantiate(QuestManager.Instance.iconPrefab, transform);
            iconObj.transform.localPosition = new Vector3(0, 1.5f, 0);
            iconRenderer = iconObj.GetComponent<SpriteRenderer>();
        }
        
        if (QuestManager.Instance != null)
        {
            // 내 퀘스트 리스트를 돌면서 가방(QuestManager)에 이미 있는지 확인
            for (int i = 0; i < questList.Count; i++)
            {
                // QuestManager의 activeQuests에 이 퀘스트가 포함되어 있다면
                if (QuestManager.Instance.activeQuests.Contains(questList[i]))
                {
                    // 이미 수락된 퀘스트이므로 인덱스를 여기에 맞추고 탈출
                    currentQuestIndex = i;
                    questList[i].isAccepted = true; // 수락 상태 강제 동기화
                    break;
                }
            }
        }
        UpdateQuestIcon();
    }

    void Update()
    {
        if (!playerNear || DialogueManager.Instance.IsOpen() || Time.unscaledTime < nextInteractTime) return;

        if (Input.GetKeyDown(interactKey))
        {
            QuestData q = CurrentQuest;

            if (q != null) // 진행할 퀘스트가 남아있다면
            {
                if (q.isAccepted && q.isCompleted)
                {
                    // 퀘스트 데이터에 들어있는 '완료 대사' 사용
                    DialogueManager.Instance.StartDialogue(this, npcName, q.completedLines, false);
                    GiveRewardAndNextQuest();
                }
                else if (q.isAccepted && !q.isCompleted)
                {
                    // 퀘스트 데이터에 들어있는 '진행 중 대사' 사용
                    DialogueManager.Instance.StartDialogue(this, npcName, q.processingLines, false);
                }
                else
                {
                    // 퀘스트 데이터에 들어있는 '시작 대사' 사용
                    DialogueManager.Instance.StartDialogue(this, npcName, q.startLines, true);
                }
            }
            else // 모든 퀘스트 완료 시
            {
                DialogueManager.Instance.StartDialogue(this, npcName, normalLines, false);
            }
        }
    }

    void GiveRewardAndNextQuest()
    {
        QuestData q = CurrentQuest;
        if (q == null) return;

        // 아이템 회수
        if (q.StealItem)
        {
            foreach (var obj in q.objectives)
            {
                if (obj.type == QuestType.ItemCollect && obj.targetItem != null)
                    InventoryManager.Instance.RemoveItem(obj.targetItem, obj.targetAmount);
            }
        }

        // 보상 지급
        if (q.rewards != null)
        {
            foreach (var reward in q.rewards)
            {
                if (reward.rewardItem != null)
                    InventoryManager.Instance.AddItem(reward.rewardItem, reward.rewardAmount);
            }
        }

        QuestManager.Instance.RemoveQuest(q);
        currentQuestIndex++;
        UpdateQuestIcon();
    }

    // 아이콘 상태 업데이트
    public void UpdateQuestIcon()
    {
        if (iconRenderer == null) return;
        QuestData q = CurrentQuest;

        if (q == null)
        {
            iconRenderer.gameObject.SetActive(false);
            return;
        }

        iconRenderer.gameObject.SetActive(true);

        if (q.isAccepted && q.isCompleted)
            iconRenderer.sprite = QuestManager.Instance.canCompleteIcon;
        else if (q.isAccepted && !q.isCompleted)
            iconRenderer.sprite = QuestManager.Instance.inProgressIcon;
        else
            iconRenderer.sprite = QuestManager.Instance.canStartIcon;
    }

    public void NotifyDialogueClosed() => nextInteractTime = Time.unscaledTime + reInteractCooldown;
    public QuestData GetCurrentQuest() => CurrentQuest;

    void OnTriggerEnter2D(Collider2D other) { if (other.CompareTag("Player")) playerNear = true; }
    void OnTriggerExit2D(Collider2D other) { if (other.CompareTag("Player")) playerNear = false; }
}