using System;

[Serializable]
public class InventorySlot
{
    public ItemData item;
    public int count;

    public InventorySlot(ItemData newItem, int amount)
    {
        item = newItem;
        count = amount;
    }

    public bool IsFull => count >= item.maxStack;
}