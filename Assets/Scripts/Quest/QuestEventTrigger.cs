using UnityEngine;
using System.Collections;

public class QuestEventTrigger : MonoBehaviour
{
    [Header("대상 퀘스트 설정")]
    public string targetQuestTitle = "할머니의 부탁"; // 감시할 퀘스트 이름

    [Header("이벤트 설정")]
    public float delaySeconds = 5f; // 완료 후 몇 초 뒤에 대사를 띄울지
    [TextArea(3, 5)]
    public string[] linesToShow;    // 출력할 대사 내용
    public string speakerName = "할머니"; // 이름표에 뜰 이름

    private bool hasTriggered = false;

    private void Start()
    {
        // 퀘스트 상태가 변할 때마다 체크하기 위해 이벤트 구독
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestListChanged += CheckQuestStatus;
        }
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestListChanged -= CheckQuestStatus;
        }
    }

    private void CheckQuestStatus()
    {
        // 이미 작동했거나 매니저가 없으면 무시
        if (hasTriggered || QuestManager.Instance == null) return;

        // 1. 현재 진행 중인 퀘스트 목록에 있는지 확인 (목록에 있으면 아직 안 끝난 것)
        bool isActive = QuestManager.Instance.activeQuests.Exists(q => q.questTitle == targetQuestTitle);
        if (isActive) return;

        // 2. 목록에 없다면, 실제로 완료(isFinished)되어 삭제된 것인지 확인하기 위해 전수 조사
        NPCDialogue[] allNPCs = FindObjectsByType<NPCDialogue>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        QuestData targetQuest = null;

        foreach (var npc in allNPCs)
        {
            targetQuest = npc.questList.Find(x => x.questTitle == targetQuestTitle);
            if (targetQuest != null) break;
        }

        // 퀘스트를 찾았고, 그 퀘스트가 최종 완료(isFinished) 상태라면 트리거 작동
        if (targetQuest != null && targetQuest.isFinished)
        {
            hasTriggered = true;
            StartCoroutine(DelayedActionRoutine());
        }
    }

    private IEnumerator DelayedActionRoutine()
    {
        yield return new WaitForSeconds(delaySeconds);

        if (DialogueManager.Instance != null && linesToShow != null && linesToShow.Length > 0)
        {
            // 대사창 띄우기 (caller를 null로 주면 특정 NPC를 비추지 않고 창만 띄웁니다)
            DialogueManager.Instance.StartDialogue(null, speakerName, linesToShow, false);
        }
    }
}