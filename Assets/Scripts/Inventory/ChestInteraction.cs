using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChestItem
{
    public ItemData itemData;
    [Min(1)] public int amount = 1;
}

public class ChestInteraction : MonoBehaviour
{
    [SerializeField] private List<ChestItem> items = new List<ChestItem>();

    [Header("Debug (읽기 전용)")]
    [SerializeField] private bool isPlayerNearby = false;
    [SerializeField] private bool isOpened = false;

    private List<ChestItem> remainingItems;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        remainingItems = new List<ChestItem>(items);
    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.Space))
        {
            if (remainingItems.Count > 0 && !ChestUI.Instance.IsOpen)
                OpenChest();
        }
    }

    private void OpenChest()
    {
        if (!isOpened)
        {
            isOpened = true;
            if (animator != null)
                animator.SetTrigger("Open");
        }
        ChestUI.Instance.Open(remainingItems, this);
    }

    public void OnItemTaken(ChestItem item)
    {
        remainingItems.Remove(item);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            Debug.Log("스페이스바를 눌러 상자를 열 수 있습니다.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerNearby = false;
    }
}
