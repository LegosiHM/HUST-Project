public static class InventoryItemCheck
{
    public static bool HasItem(this InventoryManager inventory, string itemName, int minQuantity = 1)
    {
        if (inventory == null || inventory.itemSlot == null)
            return false;

        int total = 0;

        foreach (var slot in inventory.itemSlot)
        {
            if (slot == null) continue;

            if (slot.itemName == itemName)
            {
                total += slot.quantity;
                if (total >= minQuantity)
                    return true;
            }
        }

        return false;
    }
}
