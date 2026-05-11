using UnityEngine;
using System.Collections;

public class NPCPresenceController : MonoBehaviour
{
    // 어떤 조건일 때 반응할지 선택
    public enum TriggerCondition
    {
        OnQuestAccepted,    // 퀘스트 수락 시
        OnQuestCompleted,   // 퀘스트 완료(isFinished) 시
        OnQuestAcceptedOrCompleted // 수락 또는 완료 시 (기존 동작)
    }

    [Header("설정")]
    public string targetQuestTitle = "퀘스트이름";
    public bool shouldBeActiveWhenTriggered = false;

    [Header("트리거 조건")]
    [Tooltip("어떤 시점에 활성화 상태가 바뀔지 선택")]
    public TriggerCondition triggerCondition = TriggerCondition.OnQuestAcceptedOrCompleted;

    [Header("지연 설정")]
    public float delaySeconds = 0f;

    private void OnEnable()
    {
        UpdatePresence();
    }

    private void Start()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestListChanged += UpdatePresence;
        }
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestListChanged -= UpdatePresence;
        }
    }

    public void UpdatePresence()
    {
        if (QuestManager.Instance == null) return;

        // 1. 현재 진행 중인 퀘스트 목록에서 검색
        QuestData q = QuestManager.Instance.activeQuests.Find(x => x.questTitle == targetQuestTitle);

        // 2. 진행 중인 목록에 없다면, 모든 NPC의 퀘스트 목록 검색 (완료된 퀘스트 포함)
        if (q == null)
        {
            NPCDialogue[] allNPCs = FindObjectsByType<NPCDialogue>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var npc in allNPCs)
            {
                q = npc.questList.Find(x => x.questTitle == targetQuestTitle);
                if (q != null) break;
            }
        }

        // 트리거 조건에 따라 활성화 여부 판단
        bool isTriggered = false;
        if (q != null)
        {
            switch (triggerCondition)
            {
                case TriggerCondition.OnQuestAccepted:
                    // 수락은 됐지만 아직 완료되지 않은 상태
                    isTriggered = q.isAccepted && !q.isFinished;
                    break;
                case TriggerCondition.OnQuestCompleted:
                    // 완료된 상태
                    isTriggered = q.isFinished;
                    break;
                case TriggerCondition.OnQuestAcceptedOrCompleted:
                    // 수락되었거나 완료된 상태 (기존 동작)
                    isTriggered = q.isAccepted || q.isFinished;
                    break;
            }
        }

        // 이미 원하는 활성화 상태라면 중복 처리 방지
        bool targetActiveState = isTriggered ? shouldBeActiveWhenTriggered : !shouldBeActiveWhenTriggered;
        if (gameObject.activeSelf == targetActiveState) return;

        // 트리거 발동 시 사라져야 하는 경우이고, 지연 시간이 설정되어 있다면
        if (isTriggered && !shouldBeActiveWhenTriggered && delaySeconds > 0)
        {
            StartCoroutine(DelayedDisable());
        }
        // 트리거 발동 시 나타나야 하는 경우이고, 지연 시간이 설정되어 있다면
        else if (isTriggered && shouldBeActiveWhenTriggered && delaySeconds > 0)
        {
            StartCoroutine(DelayedEnable());
        }
        else
        {
            ApplyPresence(isTriggered);
        }
    }

    private IEnumerator DelayedDisable()
    {
        yield return new WaitForSeconds(delaySeconds);
        ApplyPresence(true);
    }

    private IEnumerator DelayedEnable()
    {
        yield return new WaitForSeconds(delaySeconds);
        ApplyPresence(true);
    }

    private void ApplyPresence(bool isTriggered)
    {
        if (isTriggered)
        {
            gameObject.SetActive(shouldBeActiveWhenTriggered);
        }
        else
        {
            gameObject.SetActive(!shouldBeActiveWhenTriggered);
        }
    }
}