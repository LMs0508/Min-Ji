using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    public GameObject panel;
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public GameObject hintText;

    [Header("Quest UI")]
    public GameObject selectionPanel; // 수락/거절 버튼 패널

    private bool hasQuest;
    private QuestData pendingQuest; // [추가] 수락을 기다리는 현재 퀘스트 데이터

    [Header("Input")]
    public KeyCode nextKey = KeyCode.Space;

    private string[] lines;
    private int index;
    private bool open;

    private NPCDialogue currentCaller;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (panel != null) panel.SetActive(false);
        if (selectionPanel != null) selectionPanel.SetActive(false);
        open = false;
    }

    void Update()
    {
        if (!open) return;

        // 수락창이 떠 있을 때는 스페이스바 입력을 무시
        if (hasQuest && selectionPanel != null && selectionPanel.activeSelf) return;

        if (Input.GetKeyDown(nextKey))
        {
            NextLine();
        }
    }

    public bool IsOpen() => open;

    // [수정] 대화 시작 시 QuestData를 매개변수로 직접 받도록 변경
    public void StartDialogue(NPCDialogue caller, string npcName, string[] newLines, bool containsQuest = false)
    {
        if (newLines == null || newLines.Length == 0) return;

        currentCaller = caller;
        lines = newLines;
        index = 0;
        hasQuest = containsQuest;

        // [추가] NPC가 가진 현재 퀘스트 정보를 가져옴
        if (hasQuest && caller != null)
        {
            pendingQuest = caller.GetCurrentQuest();
        }
        else
        {
            pendingQuest = null;
        }

        open = true;
        if (panel != null) panel.SetActive(true);
        if (selectionPanel != null) selectionPanel.SetActive(false);

        if (hintText != null) hintText.SetActive(newLines.Length > 1);

        if (nameText != null) nameText.text = npcName;
        ShowCurrentLine();

        Time.timeScale = 0f;
    }

    void NextLine()
    {
        index++;

        if (index >= lines.Length)
        {
            EndDialogue();
            return;
        }

        ShowCurrentLine();
    }

    void ShowCurrentLine()
    {
        if (dialogueText != null) dialogueText.text = lines[index];

        bool isLastLine = (index >= lines.Length - 1);

        if (hasQuest && isLastLine)
        {
            if (selectionPanel != null) selectionPanel.SetActive(true);
        }

        if (hintText != null)
        {
            hintText.SetActive(!isLastLine);
        }
    }

    void EndDialogue()
    {
        if (hintText != null) hintText.SetActive(false);

        if (hasQuest)
        {
            if (selectionPanel != null) selectionPanel.SetActive(true);
        }
        else
        {
            CloseDialogue();
        }
    }

    public void CloseDialogue()
    {
        open = false;
        if (panel != null) panel.SetActive(false);
        if (selectionPanel != null) selectionPanel.SetActive(false);
        Time.timeScale = 1f;

        if (currentCaller != null)
        {
            currentCaller.NotifyDialogueClosed();
            currentCaller = null;
        }
        pendingQuest = null; // 초기화
    }

    // [핵심 수정] 수락 버튼 클릭 시 리스트 방식에 맞춰 동작
    public void OnAcceptQuest()
    {
        // NPC의 GetCurrentQuest()를 통해 받아온 pendingQuest를 사용
        if (pendingQuest != null)
        {
            // 수락하는 시점에 상태 초기화
            pendingQuest.isAccepted = true;
            pendingQuest.isCompleted = false;

            // 퀘스트 매니저에 추가
            QuestManager.Instance.AddQuest(pendingQuest);

            // NPC 아이콘 갱신 (수락했으므로 책 아이콘 등으로 변경)
            if (currentCaller != null)
            {
                currentCaller.UpdateQuestIcon();
            }
        }

        CloseDialogue();
    }

    public void OnRejectQuest()
    {
        CloseDialogue();
    }
}