using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    public string npcName = "NPC";
    [TextArea(2, 4)]
    public string[] lines;

    public KeyCode interactKey = KeyCode.Space;

    bool playerNear;

    void Update()
    {
        if (!playerNear) return;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen()) return;

        if (Input.GetKeyDown(interactKey))
        {
            DialogueManager.Instance.StartDialogue(npcName, lines);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNear = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNear = false;
    }
}