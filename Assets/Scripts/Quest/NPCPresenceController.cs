using UnityEngine;
using System.Collections; // 지연 시간을 위해 추가

public class NPCPresenceController : MonoBehaviour
{
    [Header("설정")]
    public string targetQuestTitle = "퀘스트이름";
    public bool shouldBeActiveWhenAccepted = false;

    [Header("지연 설정")]
    public float delaySeconds = 0f; // 몇 초 뒤에 사라지게 할지 인스펙터에서 설정

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

        // 2. 진행 중인 목록에 없다면, 이미 완료되어 리스트에서 나갔는지 확인하기 위해 모든 NPC의 퀘스트 목록 검색
        // (비활성화된 객체도 포함하여 찾아야 완료된 퀘스트 데이터를 확인할 수 있습니다)
        if (q == null)
        {
            NPCDialogue[] allNPCs = FindObjectsByType<NPCDialogue>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var npc in allNPCs)
            {
                q = npc.questList.Find(x => x.questTitle == targetQuestTitle);
                if (q != null) break;
            }
        }

        // 수락되었거나 이미 종료(Finished)된 상태인지 체크
        bool isStartedOrDone = (q != null && (q.isAccepted || q.isFinished));

        // 이미 원하는 활성화 상태라면 중복 처리를 방지 (비활성화 상태에서 코루틴 시작 시 오류 방지)
        bool targetActiveState = isStartedOrDone ? shouldBeActiveWhenAccepted : !shouldBeActiveWhenAccepted;
        if (gameObject.activeSelf == targetActiveState) return;

        // 퀘스트 수락 시 사라져야 하는 경우(마을 할머니)이고, 지연 시간이 설정되어 있다면
        if (isStartedOrDone && !shouldBeActiveWhenAccepted && delaySeconds > 0)
        {
            StartCoroutine(DelayedDisable());
        }
        else
        {
            // 그 외의 경우(즉시 나타나야 하는 집 안 할머니 등)는 기존처럼 즉시 처리
            ApplyPresence(isStartedOrDone);
        }
    }

    private IEnumerator DelayedDisable()
    {
        yield return new WaitForSeconds(delaySeconds);
        ApplyPresence(true); // 수락된 상태(isAccepted = true)로 적용
    }

    private void ApplyPresence(bool isAccepted)
    {
        if (isAccepted)
        {
            gameObject.SetActive(shouldBeActiveWhenAccepted);
        }
        else
        {
            gameObject.SetActive(!shouldBeActiveWhenAccepted);
        }
    }
}