using ImproveGame.Content.Tiles;
using Terraria.DataStructures;

namespace ImproveGame.Common.GlobalItems;

// 监听有没有额外生成的物品
public class AutofishItemListener : GlobalItem
{
    internal static TEAutofisher ListeningAutofisher;

    public override void OnSpawn(Item item, IEntitySource source)
    {
        if (Main.netMode is NetmodeID.MultiplayerClient || ListeningAutofisher is null)
            return;

        ListeningAutofisher.DirectAddItemToStorage(item);
        item.TurnToAir();
    }
}