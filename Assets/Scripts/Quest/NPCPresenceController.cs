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

        QuestData q = QuestManager.Instance.activeQuests.Find(x => x.questTitle == targetQuestTitle);
        bool isAccepted = (q != null && q.isAccepted);

        // 퀘스트 수락 시 사라져야 하는 경우(마을 할머니)이고, 지연 시간이 설정되어 있다면
        if (isAccepted && !shouldBeActiveWhenAccepted && delaySeconds > 0)
        {
            StartCoroutine(DelayedDisable());
        }
        else
        {
            // 그 외의 경우(즉시 나타나야 하는 집 안 할머니 등)는 기존처럼 즉시 처리
            ApplyPresence(isAccepted);
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