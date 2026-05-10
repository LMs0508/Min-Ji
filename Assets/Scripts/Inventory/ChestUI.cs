using System.Collections.Generic;
using UnityEngine;

public class ChestUI : MonoBehaviour
{
    public static ChestUI Instance;

    [SerializeField] private GameObject panel;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private GameObject chestSlotPrefab;

    public bool IsOpen => panel != null && panel.activeSelf;

    private ChestInteraction currentChest;
    private readonly List<ChestSlotUI> spawnedSlots = new List<ChestSlotUI>();

    private void Awake()
    {
        Instance = this;
        if (panel != null) panel.SetActive(false);
    }

    public void Open(List<ChestItem> items, ChestInteraction chest)
    {
        currentChest = chest;

        foreach (var slot in spawnedSlots)
            if (slot != null) Destroy(slot.gameObject);
        spawnedSlots.Clear();

        foreach (var item in items)
        {
            var go = Instantiate(chestSlotPrefab, slotContainer);
            var slotUI = go.GetComponent<ChestSlotUI>();
            slotUI.SetItem(item, this);
            spawnedSlots.Add(slotUI);
        }

        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OnItemTaken(ChestSlotUI slot, ChestItem item)
    {
        currentChest?.OnItemTaken(item);
        spawnedSlots.Remove(slot);
        Destroy(slot.gameObject);

        if (spawnedSlots.Count == 0)
            Close();
    }

    public void Close()
    {
        panel.SetActive(false);
        Time.timeScale = 1f;
        currentChest = null;
    }
}
