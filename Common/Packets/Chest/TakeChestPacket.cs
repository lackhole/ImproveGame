﻿using ImproveGame.Content.Items;

namespace ImproveGame.Common.Packets.Chest;

/// <summary>
/// 服务器向使用者发送Item信息
/// </summary>
public class TakeChestPacket : NetModule
{
    private string chestName;
    private ItemPosition itemID;
    private Item[] items;
    public static TakeChestPacket Get(ItemPosition itemID, Item[] items, string chestName)
    {
        var packet = ModContent.GetInstance<TakeChestPacket>();
        packet.itemID = itemID;
        packet.items = items;
        packet.chestName = chestName;
        return packet;

    }
    public override void Read(BinaryReader r)
    {
        itemID = new ItemPosition(r.ReadByte(), r.ReadInt32());
        items = r.ReadItemArray();
        chestName = r.ReadString();
    }

    public override void Receive()
    {
        Item item = itemID.slot >= 0 ? Main.player[itemID.player].inventory[itemID.slot] : null;
        if (item?.ModItem is not MoveChest chest)
        {
            Mod.Logger.Error("Unexpected Take Chest Packet Error");
            return;
        }
        chest.items = items;
        chest.chestName = chestName;
        items = null;
    }

    public override void Send(ModPacket p)
    {
        p.Write(itemID.player);
        p.Write(itemID.slot);
        p.Write(items);
        p.Write(chestName);
    }
}
