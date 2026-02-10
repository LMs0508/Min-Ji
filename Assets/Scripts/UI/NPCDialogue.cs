using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    public string npcName = "NPC";
    [TextArea(2, 4)]
    public string[] lines;

    public KeyCode interactKey = KeyCode.Space;

    [Header("Cooldown (after dialogue closes)")]
    public float reInteractCooldown = 0.5f;

    private float nextInteractTime = 0f;
    private bool playerNear;

    void Update()
    {
        if (!playerNear) return;
        if (DialogueManager.Instance == null) return;

        // 대화 중이면 NPC가 다시 시작 못하게
        if (DialogueManager.Instance.IsOpen()) return;

        // 대화가 끝난 직후 쿨다운(같은 NPC만 적용)
        if (Time.unscaledTime < nextInteractTime) return;

        if (Input.GetKeyDown(interactKey))
        {
            DialogueManager.Instance.StartDialogue(this, npcName, lines);
        }
    }

    // DialogueManager가 "대화 종료"할 때 호출해줌
    public void NotifyDialogueClosed()
    {
        nextInteractTime = Time.unscaledTime + reInteractCooldown;
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