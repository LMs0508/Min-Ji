using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    [Header("대사 설정")]
    public string doorName = "문";
    [TextArea(1, 3)] public string[] lockedLines = { "꼼짝도 하지 않는다." };

    public KeyCode interactKey = KeyCode.Space;
    private bool playerNear = false;
    private float nextInteractTime = 0f;

    void Update()
    {
        if (!playerNear || DialogueManager.Instance.IsOpen()) return;
        if (Time.unscaledTime < nextInteractTime) return;
        if (!Input.GetKeyDown(interactKey)) return;

        DialogueManager.Instance.StartDialogue(null, doorName, lockedLines, false);
        nextInteractTime = Time.unscaledTime + 0.5f;
    }

    void OnTriggerEnter2D(Collider2D other) { if (other.CompareTag("Player")) playerNear = true; }
    void OnTriggerExit2D(Collider2D other) { if (other.CompareTag("Player")) playerNear = false; }
}
