using ImproveGame.Content.Items.Globes;
using ImproveGame.Content.Items.Globes.Core;
using Terraria.ID;

namespace ImproveGame.Content.Projectiles;

public abstract class GlobeProjBase : ModProjectile
{
    public override string Texture => GetModItemDummy().Texture;

    public override LocalizedText DisplayName => GetModItemDummy().DisplayName;

    public override void SetStaticDefaults() => Globe.GlobeLookup[GetModItemDummy().Type] = Type;

    public override void SetDefaults()
    {
        Projectile.CloneDefaults(ProjectileID.MoonGlobe);
        AIType = ProjectileID.MoonGlobe;
    }

    public override void OnKill(int timeLeft)
    {
        base.OnKill(timeLeft);

        SoundEngine.PlaySound(SoundID.Item107, Projectile.Center);
        for (int i = 0; i < 15; i++)
        {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Glass, 0f, -2f, 0, default,
                1.2f);
        }

        // RevealOperation可能在所有端执行，但物品只能在服务器/单人掉落
        if (!RevealOperation() && Main.netMode is not NetmodeID.MultiplayerClient)
            Item.NewItem(Projectile.GetItemSource_DropAsItem(), Projectile.Center, GetModItemDummy().Type,
                noGrabDelay: true);
    }

    public abstract ModItem GetModItemDummy();

    public virtual bool RevealOperation()
    {
        var modItem = GetModItemDummy();
        return modItem switch
        {
            OnceForAllGlobe onceForAllGlobe => onceForAllGlobe.RevealOperation(Projectile),
            Globe basicGlobe => basicGlobe.RevealOperation(Projectile),
            _ => false
        };
    }
}