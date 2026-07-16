using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace ServerEssentials.Commands;

public static class Utils
{
    public static bool CheckPlayerInventoryForCommandCost(IServerPlayer player, string itemId, int quantity)
    {
        if (string.IsNullOrEmpty(itemId) || quantity <= 0) return true;

        AssetLocation targetCode = new(itemId);
        int totalFound = 0;

        string[] inventoryNames = [GlobalConstants.hotBarInvClassName, GlobalConstants.backpackInvClassName];
        foreach (string invName in inventoryNames)
        {
            IInventory inv = player.InventoryManager.GetOwnInventory(invName);
            if (inv == null) continue;
            foreach (ItemSlot slot in inv)
            {
                if (slot.Itemstack?.Collectible?.Code?.Equals(targetCode) == true)
                    totalFound += slot.Itemstack.StackSize;
            }
        }

        return totalFound >= quantity;
    }

    public static bool ConsumeItemsForCommandCost(IServerPlayer player, string itemId, int quantity)
    {
        if (string.IsNullOrEmpty(itemId) || quantity <= 0) return true;

        AssetLocation targetCode = new(itemId);

        if (!CheckPlayerInventoryForCommandCost(player, itemId, quantity))
        {
            player.SendMessage(0, new StringBuilder().AppendFormat(Configuration.translationNotEnoughItems, GetItemName(itemId), quantity).ToString(), EnumChatType.CommandError);
            return false;
        }

        int remaining = quantity;
        string[] inventoryNames = [GlobalConstants.hotBarInvClassName, GlobalConstants.backpackInvClassName];
        foreach (string invName in inventoryNames)
        {
            IInventory inv = player.InventoryManager.GetOwnInventory(invName);
            if (inv == null) continue;
            foreach (ItemSlot slot in inv)
            {
                if (slot.Itemstack?.Collectible?.Code?.Equals(targetCode) != true) continue;

                int take = System.Math.Min(remaining, slot.Itemstack.StackSize);
                slot.TakeOut(take);
                remaining -= take;

                if (remaining <= 0) return true;
            }
        }

        return true;
    }

    public static string GetItemName(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return itemId;

        AssetLocation code = new(itemId);
        string itemName = Lang.GetMatching($"{code.Domain}:item-{code.Path}");
        if (itemName != $"{code.Domain}:item-{code.Path}") return itemName;

        string blockName = Lang.GetMatching($"{code.Domain}:block-{code.Path}");
        if (blockName != $"{code.Domain}:block-{code.Path}") return blockName;

        return itemId;
    }
}
