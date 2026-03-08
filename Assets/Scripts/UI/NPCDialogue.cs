using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    private SpriteRenderer iconRenderer;
    public string npcName = "NPC";

    [Header("Quest Mode Dialogue")]
    [TextArea(2, 4)] public string[] lines;             // 처음 퀘스트 줄 때 대사
    [TextArea(2, 4)] public string[] processingLines;    // 진행 중일 때 대사
    [TextArea(2, 4)] public string[] completedLines;     // 목표 달성 후 보고 대사
    [TextArea(2, 4)] public string[] missingItemLines;   // 아이템 부족할 때 대사 (선택 사항)

    [Header("Normal Mode Dialogue")]
    [TextArea(2, 4)] public string[] normalLines;        // [추가] 퀘스트 완료 후 일반 NPC 대사

    public KeyCode interactKey = KeyCode.Space;

    [Header("Cooldown")]
    public float reInteractCooldown = 0.5f;
    private float nextInteractTime = 0f;
    private bool playerNear;

    [Header("Quest Settings")]
    public bool hasQuest;
    public QuestData quest;


    void Start()
    {
        if (QuestManager.Instance != null && QuestManager.Instance.iconPrefab != null)
        {
            GameObject iconObj = Instantiate(QuestManager.Instance.iconPrefab, transform);
            iconObj.transform.localPosition = new Vector3(0, 1.5f, 0);
            iconRenderer = iconObj.GetComponent<SpriteRenderer>();
        }
        UpdateQuestIcon();
    }

    void Update()
    {
        if (!playerNear || DialogueManager.Instance.IsOpen() || Time.unscaledTime < nextInteractTime) return;

        if (Input.GetKeyDown(interactKey))
        {
            // --- 1. 퀘스트 모드 (hasQuest가 true일 때만 진입) ---
            if (hasQuest)
            {
                if (quest.isAccepted && quest.isCompleted)
                {
                    // 목표 달성 상태: 완료 대사 후 보상 지급
                    DialogueManager.Instance.StartDialogue(this, npcName, completedLines, false);
                    GiveRewardAndFinish();
                }
                else if (quest.isAccepted && !quest.isCompleted)
                {
                    // 진행 중 상태
                    DialogueManager.Instance.StartDialogue(this, npcName, processingLines, false);
                }
                else
                {
                    // 수락 전 상태: 퀘스트 수락 창 포함하여 대화 시작
                    DialogueManager.Instance.StartDialogue(this, npcName, lines, true);
                }
            }
            // --- 2. 일반 모드 (퀘스트를 다 깼거나 처음부터 없을 때) ---
            else
            {
                if (normalLines != null && normalLines.Length > 0)
                {
                    DialogueManager.Instance.StartDialogue(this, npcName, normalLines, false);
                }
                else
                {
                    Debug.Log("일반 대화 내용이 비어있습니다.");
                }
            }
        }
    }

    void GiveRewardAndFinish()
    {
        // 아이템 회수 로직
        if (quest.StealItem && quest.targetItem != null)
        {
            InventoryManager.Instance.RemoveItem(quest.targetItem, quest.targetAmount);
        }

        // 보상 지급 로직
        if (quest.rewardItem != null)
        {
            InventoryManager.Instance.AddItem(quest.rewardItem, quest.rewardAmount);
        }

        QuestManager.Instance.RemoveQuest(quest);

        // 중요: hasQuest를 false로 만들어 다음 대화부터 일반 모드로 전환
        hasQuest = false;

        UpdateQuestIcon();
    }

    public void NotifyDialogueClosed()
    {
        nextInteractTime = Time.unscaledTime + reInteractCooldown;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNear = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNear = false;
    }


    public void UpdateQuestIcon()
    {
        // 1. 아이콘 렌더러가 없거나, NPC가 더 이상 줄 퀘스트가 없다면(hasQuest가 false면) 아이콘을 끈다.
        if (iconRenderer == null) return;

        if (!hasQuest)
        {
            iconRenderer.gameObject.SetActive(false);
            return;
        }

        // 2. 퀘스트가 있는 경우에만 아이콘을 활성화하고 상태를 체크한다.
        iconRenderer.gameObject.SetActive(true);

        if (quest.isAccepted && quest.isCompleted)
        {
            //  완료 보고 가능 (가방에 물건 다 있음)
            iconRenderer.sprite = QuestManager.Instance.canCompleteIcon;
        }
        else if (quest.isAccepted && !quest.isCompleted)
        {
            //  진행 중 (수락은 했으나 물건 부족)
            iconRenderer.sprite = QuestManager.Instance.inProgressIcon;
        }
        else
        {
            //  시작 가능 (아직 안 받음)
            iconRenderer.sprite = QuestManager.Instance.canStartIcon;
        }
    }
}
