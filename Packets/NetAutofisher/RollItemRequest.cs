using ImproveGame.Common.GlobalItems;
using ImproveGame.Common.ModPlayers;
using ImproveGame.Common.ModSystems;
using ImproveGame.Content.Tiles;

namespace ImproveGame.Packets.NetAutofisher;

/// <summary>
/// 服务器发送给任意连接的客户端，询问钓鱼机“假人”所处环境
/// Player.UpdateBiomes本应是客户端执行的代码，不应该放到服务器运行
/// </summary>
[AutoSync]
public class RollItemRequest : NetModule
{
    private int _tileEntityID;
    private int _rolledItemDrop;

    public static void SendTo(TEAutofisher fisher, int toClient)
    {
        var module = NetModuleLoader.Get<RollItemRequest>();
        module._tileEntityID = fisher.ID;
        module._rolledItemDrop = 0;
        module.Send(toClient);
    }

    public override void Receive()
    {
        if (!TryGetTileEntityAs<TEAutofisher>(_tileEntityID, out var autofisher) ||
            TileLoader.GetTile(Main.tile[autofisher.Position.ToPoint()].TileType) is not Autofisher)
        {
            return;
        }

        switch (Main.netMode)
        {
            case NetmodeID.MultiplayerClient:
                bool accAvailable = ModIntegrationsSystem.FishingStatLookup.TryGetValue(autofisher.accessory.type,
                    out FishingStat stat);
                autofisher.LavaFishing = false;
                autofisher.TackleBox = false;
                autofisher.FishingSkill = 0;
                if (accAvailable)
                    autofisher.ApplyAccessories(stat);
                var fisher = autofisher.GetFisher(out _);

                autofisher.RollItemDrop(ref fisher);

                if (fisher.rolledItemDrop != 0)
                {
                    _rolledItemDrop = fisher.rolledItemDrop;
                    Send(); // 发送回服务器
                    // Main.NewText($"[i:{fisher.rolledItemDrop}]");
                }

                break;
            case NetmodeID.Server:
                if (_rolledItemDrop != 0)
                {
                    AutofishItemListener.ListeningAutofisher = autofisher;

                    try
                    {
                        var player = TEAutofisher.GetClosestPlayer(autofisher.Position);
                        autofisher.GiveCatchToStorage(player, _rolledItemDrop);
                    }
                    catch
                    {
                        // ignored
                    } finally
                    {
                        AutofishItemListener.ListeningAutofisher = null;
                    }
                }

                break;
            default:
                Console.WriteLine("What the fuck?");
                break;
        }
    }
}