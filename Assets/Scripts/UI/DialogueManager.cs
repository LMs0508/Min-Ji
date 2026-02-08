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
    public KeyCode interactKey = KeyCode.Space;

    string[] lines;
    int index;
    bool isOpen;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (panel) panel.SetActive(false);
    }

    void Update()
    {
        if (!isOpen) return;

        if (Input.GetKeyDown(interactKey))
        {
            Next();
        }
    }

    public void StartDialogue(string speakerName, string[] dialogueLines)
    {
        if (dialogueLines == null || dialogueLines.Length == 0) return;

        lines = dialogueLines;
        index = 0;
        isOpen = true;

        panel.SetActive(true);
        nameText.text = speakerName;
        dialogueText.text = lines[index];

        // (선택) 게임 멈추기
        // Time.timeScale = 0f;
    }

    void Next()
    {
        index++;

        if (index >= lines.Length)
        {
            EndDialogue();
            return;
        }

        dialogueText.text = lines[index];
    }

    public void EndDialogue()
    {
        isOpen = false;
        panel.SetActive(false);

        // (선택) 게임 재개
        // Time.timeScale = 1f;
    }

    public bool IsOpen() => isOpen;
}