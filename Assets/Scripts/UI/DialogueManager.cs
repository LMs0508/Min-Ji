using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    public GameObject panel;
    public TMP_Text nameText;
    public TMP_Text dialogueText;

    [Header("Quest UI")]
    public GameObject selectionPanel; // 수락/거절 버튼 패널

    private bool hasQuest;

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

        // 선택지가 떠 있을 때는 스페이스바로 대화가 넘어가지 않게 방지
        if (selectionPanel != null && selectionPanel.activeSelf) return;

        if (Input.GetKeyDown(nextKey))
        {
            NextLine();
        }
    }

    public bool IsOpen() => open;

    public void StartDialogue(NPCDialogue caller, string npcName, string[] newLines, bool containsQuest = false)
    {
        if (newLines == null || newLines.Length == 0) return;

        currentCaller = caller;
        lines = newLines;
        index = 0;
        hasQuest = containsQuest;

        open = true;
        if (panel != null) panel.SetActive(true);
        if (selectionPanel != null) selectionPanel.SetActive(false);

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
    }

    void EndDialogue()
    {
        if (hasQuest)
        {
            // 퀘스트가 이미 수락된 상태인지 체크 (중복 수락 방지)
            if (currentCaller != null && currentCaller.quest.isAccepted)
            {
                CloseDialogue();
            }
            else
            {
                if (selectionPanel != null) selectionPanel.SetActive(true);
            }
            return;
        }

        CloseDialogue();
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
    }

    // [수정] 수락 버튼 클릭 시 실행
    public void OnAcceptQuest()
    {
        if (currentCaller != null)
        {
            // 수락하는 시점에 퀘스트 상태 초기화
            currentCaller.quest.isAccepted = true;
            currentCaller.quest.isCompleted = false; // 처음엔 무조건 미완료(회색 체크)

            QuestManager.Instance.AddQuest(currentCaller.quest);
        }
        CloseDialogue();
    }

    public void OnRejectQuest()
    {
        CloseDialogue();
    }
}