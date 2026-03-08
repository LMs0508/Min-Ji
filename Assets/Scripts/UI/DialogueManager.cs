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

        // 대화 시작 시 첫 문장이 마지막이 아니라면 힌트 텍스트를 보여줍니다.
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

        // [추가] 마지막 대화 줄인지 확인하여 힌트 텍스트 제어
        if (hintText != null)
        {
            // 마지막 줄에 도달했거나 퀘스트가 포함된 마지막 문장이면 힌트를 숨깁니다.
            bool isLastLine = (index >= lines.Length - 1);
            hintText.SetActive(!isLastLine);
        }
    }

    void EndDialogue()
    {
        // 대화가 끝났으므로 힌트는 무조건 숨깁니다.
        if (hintText != null) hintText.SetActive(false);

        if (hasQuest)
        {
            if (selectionPanel != null) selectionPanel.SetActive(true);
            // 수락/거절 단계에서는 Space 입력이 대화를 닫지 않도록 
            // 이전 Update문에서 selectionPanel.activeSelf 체크를 유지해야 합니다.
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