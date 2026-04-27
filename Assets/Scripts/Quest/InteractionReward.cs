using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class InteractionRewardEntry
{
    public ItemData item;          // 지급할 아이템
    public int amount = 1;         // 지급할 개수
    [Range(0f, 100f)]
    public float chance;           // 이 아이템이 당첨될 확률 (0~100)
}

// DialogueManager의 ShowQuiz를 사용하기 위해 QuizNPC 인터페이스를 구현합니다.
public class InteractionReward : MonoBehaviour, IQuizHandler
{
    [Header("상호작용 설정")]
    public string interactionName = "상자"; // 다이얼로그 이름창에 뜰 이름

    [Header("UI Settings")]
    public bool showQuestIcon = true; // 아이콘 표시 여부 체크박스

    [TextArea(2, 4)]
    public string promptMessage = "이것을 열어보시겠습니까?"; // 출력될 대사

    [Header("피드백 설정")]
    public bool useDialogueFeedback = false; // 대사창으로 결과를 보여줄지 여부
    public bool useIconFeedback = true;      // 머리 위 아이콘으로 결과를 보여줄지 여부

    [Header("보상 설정")]
    public List<InteractionRewardEntry> rewards = new List<InteractionRewardEntry>(); // 여러 개의 보상 목록

    [Header("결과 메시지")]
    [TextArea(1, 2)]
    public string successMessage = "무언가 낚였습니다!"; // 성공 시 출력할 메시지
    [TextArea(1, 2)]
    public string failureMessage = "아무것도 낚이지 않았습니다."; // 실패 시 출력할 메시지

    [Header("이펙트 설정")]
    public GameObject successIconPrefab; // 성공 시 주인공 머리 위에 띄울 프리팹

    public GameObject FailedIconPrefab;

    public Vector3 iconOffset = new Vector3(0, 2.5f, 0); // 머리 위 위치 오프셋
    public float iconDuration = 0.5f; // 아이콘 유지 시간

    [Header("퀘스트 연동")]
    public string questTargetID; // 이 값이 있으면 성공 시 해당 ID의 퀘스트 카운트가 올라감
    public QuestType questType = QuestType.ActivateObject;

    [Header("조작 설정")]
    public KeyCode interactKey = KeyCode.Space;
    
    private bool isPlayerNear = false;
    private bool isProcessing = false;
    private SpriteRenderer iconRenderer;


    void Update()
    {
        // 플레이어가 근처에 있고, 스페이스바를 눌렀으며, 현재 진행 중이 아닐 때
        if (isPlayerNear && Input.GetKeyDown(interactKey) && !isProcessing)
        {
            StartInteraction();
        }
    }

    private void StartInteraction()
    {
        isProcessing = true;
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartQuizMode(this, promptMessage, interactionName);
        }
    }


    // [중요] DialogueManager의 O/X 버튼을 눌렀을 때 실행되는 함수
    public void OnAnswer(bool isO)
    {
        if (isO)
        {
            // 수락(O)을 눌렀을 때 리워드 계산
            CalculateReward();
        }
        else
        {
            // 거절(X)을 눌렀을 때
            Debug.Log("상호작용을 취소했습니다.");
            isProcessing = false;
        }
    }

    private void CalculateReward()
    {
        // 0.0 ~ 100.0 사이의 랜덤 숫자를 생성
        float randomValue = Random.Range(0f, 100f);
        float cumulativeChance = 0f;
        InteractionRewardEntry selectedReward = null;

        // 리스트를 돌며 누적 확률을 계산하여 당첨 여부 확인
        foreach (var reward in rewards)
        {
            cumulativeChance += reward.chance;
            if (randomValue < cumulativeChance)
            {
                selectedReward = reward;
                break;
            }
        }

        if (selectedReward != null && selectedReward.item != null)
        {
            // 확률 성공: 당첨된 아이템 지급
            InventoryManager.Instance?.AddItem(selectedReward.item, selectedReward.amount);
            
            // 아이콘 피드백
            if (useIconFeedback && successIconPrefab != null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    GameObject icon = Instantiate(successIconPrefab, player.transform);
                    icon.transform.localPosition = iconOffset;
                    Destroy(icon, iconDuration);
                }
            }

            // 대사창 피드백
            if (useDialogueFeedback && DialogueManager.Instance != null)
            {
                string finalSuccessMsg = successMessage.Replace("{item}", selectedReward.item.itemName);
                DialogueManager.Instance.StartDialogue(null, interactionName, new string[] { finalSuccessMsg }, false);
            }

            // [추가] 퀘스트 진행도 업데이트
            if (!string.IsNullOrEmpty(questTargetID))
            {
                QuestManager.Instance?.ProgressQuest(questType, questTargetID, 1);
            }
        }
        else
        {
            if (useIconFeedback && FailedIconPrefab != null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    GameObject icon = Instantiate(FailedIconPrefab, player.transform);
                    icon.transform.localPosition = iconOffset;
                    Destroy(icon, iconDuration);
                }
            }


            // 실패 시 대사창 피드백
            if (useDialogueFeedback && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(null, interactionName, new string[] { failureMessage }, false);
            }
        }

        // 상호작용 종료 (다시 실행 가능하게 변경)
        isProcessing = false;
    }

    // 플레이어 진입 감지
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
        }
    }

    // 플레이어 이탈 감지
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            isProcessing = false;
        }
    }
}