using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    public GameObject panel;
    public TMP_Text nameText;
    public TMP_Text dialogueText;

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

    public void StartDialogue(NPCDialogue caller, string npcName, string[] newLines)
    {
        if (newLines == null || newLines.Length == 0) return;

        currentCaller = caller;   // <-- 여기 저장
        lines = newLines;
        index = 0;

        open = true;
        if (panel != null) panel.SetActive(true);

        if (nameText != null) nameText.text = npcName;
        ShowCurrentLine();

        Time.timeScale = 0f; // 게임 정지(너가 지금 쓰는 방식 유지)
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
        open = false;

        if (panel != null) panel.SetActive(false);

        Time.timeScale = 1f; // 게임 재개

        //  "대화가 닫힌 기준" 쿨다운 시작
        if (currentCaller != null)
        {
            currentCaller.NotifyDialogueClosed();
            currentCaller = null;
        }
    }
}