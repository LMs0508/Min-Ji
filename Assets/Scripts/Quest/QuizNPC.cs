using UnityEngine;

[System.Serializable]
public class QuizQuestion
{
    [TextArea(2, 3)] public string question;
    public bool isOCorrect;
    [TextArea(1, 2)] public string correctLine;
    [TextArea(1, 2)] public string wrongLine;
}

public class QuizNPC : MonoBehaviour
{
    public string npcName = "NPC";

    [Header("Quiz")]
    public QuizQuestion[] questions;
    [TextArea(2, 4)] public string[] introLines;
    [TextArea(2, 4)] public string[] allCorrectLines;

    [Header("Reward")]
    public ItemData rewardItem;
    public int rewardAmount = 1;

    [Header("Quest Integration")]
    public string questTitle;

    public KeyCode interactKey = KeyCode.Space;
    private float nextInteractTime = 0f;
    private bool playerNear = false;

    private int currentIndex = 0;
    private bool quizComplete = false;
    private bool introShown = false;

    void Update()
    {
        if (!playerNear || DialogueManager.Instance.IsOpen()) return;
        if (Time.unscaledTime < nextInteractTime) return;
        if (!Input.GetKeyDown(interactKey)) return;

        if (quizComplete)
        {
            if (allCorrectLines != null && allCorrectLines.Length > 0)
                DialogueManager.Instance.StartDialogue(null, npcName, allCorrectLines, false);
            return;
        }

        if (!introShown && introLines != null && introLines.Length > 0)
        {
            introShown = true;
            DialogueManager.Instance.StartDialogue(null, npcName, introLines, false);
            return;
        }

        AskCurrentQuestion();
    }

    void AskCurrentQuestion()
    {
        if (currentIndex >= questions.Length) return;
        DialogueManager.Instance.StartQuizMode(this, questions[currentIndex].question, npcName);
    }

    public void OnAnswer(bool isO)
    {
        nextInteractTime = Time.unscaledTime + 0.5f;

        QuizQuestion q = questions[currentIndex];
        bool correct = (isO == q.isOCorrect);

        if (correct)
        {
            currentIndex++;

            if (currentIndex >= questions.Length)
            {
                quizComplete = true;
                CompleteQuest();
                string[] lines = (allCorrectLines != null && allCorrectLines.Length > 0)
                    ? allCorrectLines
                    : new[] { "모든 문제를 맞췄어요!" };
                DialogueManager.Instance.StartDialogue(null, npcName, lines, false);
            }
            else
            {
                string line = !string.IsNullOrEmpty(q.correctLine) ? q.correctLine : "정답이에요!";
                DialogueManager.Instance.StartDialogue(null, npcName, new[] { line }, false);
            }
        }
        else
        {
            string line = !string.IsNullOrEmpty(q.wrongLine) ? q.wrongLine : "틀렸어요. 다시 생각해봐요.";
            DialogueManager.Instance.StartDialogue(null, npcName, new[] { line }, false);
        }
    }

    void CompleteQuest()
    {
        if (rewardItem != null && InventoryManager.Instance != null)
            InventoryManager.Instance.AddItem(rewardItem, rewardAmount);

        if (string.IsNullOrEmpty(questTitle) || QuestManager.Instance == null) return;
        QuestData quest = QuestManager.Instance.activeQuests.Find(q => q.questTitle == questTitle);
        if (quest == null) return;
        foreach (var obj in quest.objectives)
            obj.currentAmount = obj.targetAmount;
        QuestManager.Instance.CheckQuestCompletion(quest);
        QuestManager.Instance.UpdateQuestUI();
    }

    void OnTriggerEnter2D(Collider2D other) { if (other.CompareTag("Player")) playerNear = true; }
    void OnTriggerExit2D(Collider2D other) { if (other.CompareTag("Player")) playerNear = false; }
}
