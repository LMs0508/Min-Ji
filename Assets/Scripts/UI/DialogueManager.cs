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
    public GameObject selectionPanel; // 수락/거절 버튼이 있는 패널

    private bool hasQuest; // 현재 대화에 퀘스트가 포함되어 있는지

    [Header("Input")]
    public KeyCode nextKey = KeyCode.Space; // "Next"도 Space

    private string[] lines;
    private int index;
    private bool open;

    private NPCDialogue currentCaller; // <-- 누가 시작했는지 저장

    void Start()
    {
        if (panel != null) panel.SetActive(false);
        open = false;
    }


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject); // 필요하면 사용
    }

    void Update()
    {
        if (!open) return;

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
        hasQuest = containsQuest; // 퀘스트 포함 여부 저장

        open = true;
        if (panel != null) panel.SetActive(true);
        if (selectionPanel != null) selectionPanel.SetActive(false); // 처음엔 버튼 숨김

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
        // 퀘스트가 있는 대화였다면 버튼 패널을 띄우고 종료하지 않음
        if (hasQuest)
        {
            if (selectionPanel != null) selectionPanel.SetActive(true);
            return;
        }

        CloseDialogue();
    }

    public void CloseDialogue() // 실제 창을 닫는 로직 분리
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

    // 버튼에 연결할 함수들
    public void OnAcceptQuest()
    {
        if (currentCaller != null)
        {
            QuestManager.Instance.AddQuest(currentCaller.quest); // 퀘스트 추가
        }
        CloseDialogue();
    }

    public void OnRejectQuest()
    {
        CloseDialogue();
    }
}