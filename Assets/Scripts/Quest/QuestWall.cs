using UnityEngine;
using System.Collections;

public class QuestWall : MonoBehaviour
{
    [Header("Quest Settings")]
    public string questTitle;

    [Header("Wall Settings")]
    public GameObject[] wallObjects;    // 사라질 벽들 (비워두면 이 GameObject 자체가 벽)
    public float disappearDelay = 0.3f; // 대사 닫힌 후 벽 사라지기까지 대기 시간

    [Header("Camera Shake")]
    public float shakeIntensity = 0.25f;
    public float shakeDuration = 0.6f;

    [Header("After Wall Dialogue")]
    public NPCDialogue linkedNPC;             // 추가 대사를 할 NPC
    [TextArea(2, 4)]
    public string[] afterWallLines;           // 벽 사라진 후 자동 출력할 대사

    private bool triggered = false;
    private bool wasQuestAccepted = false;

    void Start()
    {
        if (wallObjects == null || wallObjects.Length == 0)
            wallObjects = new GameObject[] { gameObject };

        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestListChanged += OnQuestChanged;
    }

    void OnDestroy()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestListChanged -= OnQuestChanged;
    }

    void OnQuestChanged()
    {
        if (triggered || QuestManager.Instance == null) return;

        QuestData quest = QuestManager.Instance.activeQuests.Find(q => q.questTitle == questTitle);

        if (quest != null)
        {
            wasQuestAccepted = true;
        }
        else if (wasQuestAccepted)
        {
            triggered = true;
            StartCoroutine(DisappearRoutine());
        }
    }

    IEnumerator DisappearRoutine()
    {
        // 첫 번째 대사가 닫힐 때까지 대기
        while (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen())
            yield return null;

        yield return new WaitForSecondsRealtime(disappearDelay);

        // 벽 사라짐 + 카메라 흔들림
        foreach (var wall in wallObjects)
        {
            if (wall != null)
                wall.SetActive(false);
        }

        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(shakeIntensity, shakeDuration);

        // 흔들림이 끝날 때까지 실시간으로 대기 후 추가 대사 시작
        if (linkedNPC != null && afterWallLines != null && afterWallLines.Length > 0)
        {
            yield return new WaitForSecondsRealtime(shakeDuration);
            DialogueManager.Instance.StartDialogue(linkedNPC, linkedNPC.npcName, afterWallLines, false);
        }
    }
}
