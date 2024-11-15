using ImproveGame.Content.Functions.AutoPiggyBank;
using ImproveGame.Core;
using ImproveGame.UI.OpenBag;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.Graphics2D;
using ImproveGame.UIFramework.SUIElements;
using ReLogic.Graphics;
using System.Diagnostics;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.UI;

namespace ImproveGame.Content.NPCs.Dummy;
[AutoSync]
public class RemoveDummyModule : NetModule
{
    int whoami;
    int from;
    public static RemoveDummyModule Get(int whoami, int from)
    {
        var packet = NetModuleLoader.Get<RemoveDummyModule>();
        packet.whoami = whoami;
        packet.from = from;
        return packet;
    }
    public override void Receive()
    {
        if (Main.npc[whoami].ModNPC is DummyNPC dummy)
            dummy.Disappear();
        if (Main.netMode == NetmodeID.Server)
        {
            Get(whoami, from).Send(-1, from);
            //Console.WriteLine(whoami);
        }
        //else
        //    Main.NewText(whoami);
    }
}

public class SyncDummyModule : NetModule
{
    Vector2? position;
    int owner;
    DummyConfig config;
    public override void Read(BinaryReader r)
    {
        if (r.ReadBoolean())
            position = r.ReadVector2();
        else position = null;
        owner = r.ReadByte();

        config.LockHP = r.ReadBoolean();
        config.LifeMax = r.ReadInt32();
        config.Defense = r.ReadInt32();
        config.Damage = r.ReadInt32();
        config.ShowBox = r.ReadBoolean();
        config.ShowDamageData = r.ReadBoolean();
        config.ShowNameOnHover = r.ReadBoolean();
        config.Immortal = r.ReadBoolean();
        config.NoGravity = r.ReadBoolean();
        config.NoTileCollide = r.ReadBoolean();
        config.KnockBackResist = r.ReadSingle();
        base.Read(r);
    }
    public override void Send(ModPacket p)
    {
        p.Write(position != null);
        if (position.HasValue)
            p.WriteVector2(position.Value);
        p.Write((byte)owner);

        p.Write(config.LockHP);
        p.Write(config.LifeMax);
        p.Write(config.Defense);
        p.Write(config.Damage);
        p.Write(config.ShowBox);
        p.Write(config.ShowDamageData);
        p.Write(config.ShowNameOnHover);
        p.Write(config.Immortal);
        p.Write(config.NoGravity);
        p.Write(config.NoTileCollide);
        p.Write(config.KnockBackResist);
        base.Send(p);
    }
    public static SyncDummyModule Get(Vector2? position, int owner, DummyConfig dummyConfig)
    {
        var packet = NetModuleLoader.Get<SyncDummyModule>();
        packet.position = position;
        packet.owner = owner;
        packet.config = dummyConfig;
        return packet;
    }
    public override void Receive()
    {
        if (position == null)
        {
            foreach (var n in Main.npc)
            {
                if (n.ModNPC is DummyNPC dummy && dummy.Owner == owner)
                {
                    dummy.Config = config;
                    dummy.SetDefaults();
                }
            }
        }
        else
        {
            NPC npc = NPC.NewNPCDirect(null, 0, 0, ModContent.NPCType<DummyNPC>(), target: owner);
            npc.Center = position.Value;
            (npc.ModNPC as DummyNPC).Owner = owner;
            (npc.ModNPC as DummyNPC).Config = config;

            for (int i = 0; i < 20; i++)
            {
                int dust = Dust.NewDust(npc.position, npc.width, npc.height, DustID.Torch, Scale: 2f);
                Main.dust[dust].velocity.Y = -1f;
                Main.dust[dust].noGravity = true;
            }
        }

        if (Main.netMode == NetmodeID.Server)
        {
            Get(position, owner, config).Send(-1, owner);
            //Console.WriteLine(position?.ToString() ?? "Null");

        }
        //else
        //{
        //    Main.NewText(position?.ToString() ?? "Null");
        //}
    }
}
public class DummyNPC : ModNPC
{
    public static DummyConfig LocalConfig = new();
    public DummyConfig Config = new();
    public DummyDPS DummyDPS = new();
    public int Owner;
    public override bool CheckDead() => false;
    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => false;

    public override void SetStaticDefaults()
    {
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers
        {
            Hide = true
        });
    }

    public override void SetDefaults()
    {
        NPC npc = NPC;
        Config = LocalConfig;
        npc.SetBaseValues(56, 74, Config.LifeMax, false,
            value: 0, damage: Config.Damage, defense: Config.Defense);
        npc.HitSound = SoundID.NPCHit1;
        npc.aiStyle = -1;
        npc.life = npc.lifeMax = Config.LifeMax;
        DummyDPS.Parent = this;
    }

    private float HitScale { get => NPC.ai[0]; set => NPC.ai[0] = value; }

    public void Reset()
    {
        NPC npc = NPC;

        npc.immortal = Config.Immortal;
        npc.ShowNameOnHover = Config.ShowNameOnHover;
        npc.damage = Config.Damage;
        npc.lifeMax = Config.LifeMax;
        if (Config.LockHP)
        {
            npc.life = npc.lifeMax;
        }

        npc.defense = Config.Defense;
        npc.noGravity = Config.NoGravity;
        npc.noTileCollide = Config.NoTileCollide;
        npc.knockBackResist = Config.KnockBackResist;

        DummyDPS.Update();
    }
    public void Disappear()
    {
        NPC npc = NPC;
        npc.active = false;
        //(npc.ModNPC as DummyNPC).DummyDPS.Reset();

        for (int i = 0; i < 20; i++)
        {
            int dust = Dust.NewDust(npc.position, npc.width, npc.height, DustID.Torch, Scale: 2f);
            Main.dust[dust].velocity.Y = -1f;
            Main.dust[dust].noGravity = true;
        }
    }
    public override void AI()
    {
        Reset();
        if (!Main.player[Owner].active || Vector2.Distance(Main.player[Owner].Center, NPC.Center) > 4096)
            RemoveDummyModule.Get(NPC.whoAmI, Main.myPlayer).Send(runLocally: true);
        NPC npc = NPC;
        npc.dontTakeDamage = CurrentFrameProperties.AnyActiveBoss;
        if (npc.HasPlayerTarget)
        {
            Player player = Main.player[npc.target];

            if (player.position.X < npc.position.X)
            {
                npc.spriteDirection = 1;
            }
            else
            {
                npc.spriteDirection = -1;
            }
        }

        UpdateTimer();

        NPC.frame = new Rectangle(0, NPC.frameCounter > 0 ? 76 : 0, 56, 74);
        npc.scale = 1f + HitScale;
        npc.velocity = Vector2.Zero;
    }

    public void ClearBuffs()
    {
        if (!NPC.active) return;

        for (int i = 0; i < NPC.maxBuffs; i++)
        {
            NPC.buffTime[i] = 0;
            NPC.buffType[i] = 0;
        }

        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendData(MessageID.NPCBuffs, -1, -1, null, NPC.whoAmI);
    }

    public void UpdateTimer()
    {
        HitScale -= 0.01f;
        HitScale = Math.Max(0, HitScale);
        NPC.frameCounter -= 1f;
        NPC.frameCounter = Math.Max(0, NPC.frameCounter);
    }

    public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        DummyDPS.Hurt(damageDone);

        HitScale = 0.1f;
        NPC.frameCounter = 60f;
    }

    public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
    {
        DummyDPS.Hurt(damageDone);

        HitScale = 0.1f;
        NPC.frameCounter = 60f;
    }

    public override bool PreDraw(SpriteBatch sb, Vector2 screenPos, Color drawColor)
    {
        NPC npc = NPC;
        Texture2D texture2D = TextureAssets.Npc[Type].Value;
        Vector2 fSize = npc.frame.Size();

        Vector2 position = npc.position - screenPos;

        if (Config.ShowBox)
        {
            SDFRectangle.HasBorder(position, npc.Size,
                new(0f), Color.Transparent, 2f, Color.White, false);
        }

        sb.Draw(texture2D, position + fSize / 2f,
            npc.frame, drawColor, npc.rotation, fSize / 2f, npc.scale,
            npc.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);

        if (Config.ShowDamageData)
        {
            DummyDPS.DrawString(position + new Vector2(fSize.X + 10f, 0f), new Vector2(0f, 0f));
        }
        /*sb.DrawString(FontAssets.MouseText.Value, $"{npc.whoAmI}\n{Config.LockHP}\n{(npc.lifeMax, Config.LifeMax)}\n{(npc.defense, Config.Defense)}\n" +
            $"{(npc.damage, Config.Damage)}\n{Config.ShowBox}\n{Config.ShowDamageData}\n{(npc.ShowNameOnHover, Config.ShowNameOnHover)}\n{(npc.immortal, Config.Immortal)}\n" +
            $"{(npc.noGravity, Config.NoGravity)}\n{(npc.noTileCollide, Config.NoTileCollide)}\n{(npc.knockBackResist, Config.KnockBackResist)}", npc.Center - Main.screenPosition + new Vector2(0, 80), Color.White);*/
        return false;
    }
}

[AutoCreateGUI(LayerName.Vanilla.RadialHotbars, "Dummy Configuration GUI")]
public class DummyConfigurationUI : BaseBody
{
    public static DummyConfigurationUI Instance { get; private set; }

    public DummyConfigurationUI() => Instance = this;

    public override bool IsNotSelectable => StartTimer.AnyClose;

    public override bool Enabled
    {
        get => StartTimer.Closing || _enabled;
        set => _enabled = value;
    }

    private bool _enabled;

    public override bool CanSetFocusTarget(UIElement target)
        => (target != this && MainPanel.IsMouseHovering) || MainPanel.IsLeftMousePressed;

    /// <summary>
    /// 启动关闭动画计时器
    /// </summary>
    public AnimationTimer StartTimer = new(3);

    // 主面板
    public SUIPanel MainPanel;

    // 标题面板
    private View TitlePanel;

    public SUIScrollBar Scrollbar;
    public UIText TipText;

    public override void OnInitialize()
    {
        // 主面板
        MainPanel = new SUIPanel(UIStyle.PanelBorder, UIStyle.PanelBg)
        {
            Shaded = true,
            Draggable = true,
            IsAdaptiveHeight = true
        };
        MainPanel.SetPadding(0f);
        MainPanel.SetPosPixels(410, 360)
            .SetSizePixels(404, 0)
            .JoinParent(this);

        TitlePanel = ViewHelper.CreateHead(Color.Black * 0.25f, 45f, 10f);
        TitlePanel.SetPadding(0f);
        TitlePanel.JoinParent(MainPanel);

        // 标题
        var title = new SUIText
        {
            IsLarge = true,
            UseKey = true,
            TextOrKey = "Mods.ImproveGame.UI.DummyConfiguration.Title",
            TextAlign = new Vector2(0f, 0.5f),
            TextScale = 0.45f,
            Height = StyleDimension.Fill,
            Width = StyleDimension.Fill,
            DragIgnore = true,
            Left = new StyleDimension(16f, 0f)
        };
        title.JoinParent(TitlePanel);

        var cross = new SUICross
        {
            HAlign = 1f,
            Rounded = new Vector4(0f, 10f, 0f, 0f),
            CrossSize = 20f,
            CrossRounded = 4.5f * 0.85f,
            Border = 0f,
            BorderColor = Color.Transparent,
            BgColor = Color.Transparent,
        };
        cross.CrossOffset.X = 1f;
        cross.Width.Pixels = 46f;
        cross.Height.Set(0f, 1f);
        cross.OnUpdate += _ =>
        {
            cross.BgColor = cross.HoverTimer.Lerp(Color.Transparent, Color.Black * 0.25f);
        };
        cross.OnLeftMouseDown += (_, _) => Close();
        cross.JoinParent(TitlePanel);

        var bagPanel = new View
        {
            DragIgnore = true,
            RelativeMode = RelativeMode.Vertical,
            IsAdaptiveHeight = true,
        };
        bagPanel.SetPadding(6, 6, 6, 6);
        bagPanel.SetSize(0f, 640, 1f, 0f);
        bagPanel.JoinParent(MainPanel);


        //void MakeSeparator()
        //{
        //    View searchArea = new()
        //    {
        //        Height = new StyleDimension(10f, 0f),
        //        Width = new StyleDimension(-16f, 1f),
        //        HAlign = 0.5f,
        //        DragIgnore = true,
        //        RelativeMode = RelativeMode.Vertical,
        //        Spacing = new Vector2(0, 6)
        //    };
        //    searchArea.JoinParent(bagPanel);
        //    searchArea.Append(new UIHorizontalSeparator
        //    {
        //        Width = StyleDimension.FromPercent(1f),
        //        Color = Color.Lerp(Color.White, new Color(63, 65, 151, 255), 0.85f) * 0.9f
        //    });
        //}
        var fieldInfos = typeof(DummyConfig).GetFields();
        foreach (var fInfo in fieldInfos)
        {
            var fPanel = new View
            {
                DragIgnore = true,
                RelativeMode = RelativeMode.Vertical,
                Rounded = new Vector4(6),
                Spacing = new Vector2(0, 8)
            };
            fPanel.SetPadding(6, 6, 6, 6);
            fPanel.SetSize(0f, 40, 1f, 0f);
            fPanel.JoinParent(bagPanel);
            fPanel.BgColor = Color.White * .25f;
            var fieldName = new SUIText
            {
                IsLarge = true,
                UseKey = true,
                TextOrKey = $"Mods.ImproveGame.NPC.fieldName.{fInfo.Name}",
                TextAlign = new Vector2(0f, 0f),
                TextScale = 0.35f,
                Height = new StyleDimension(40, 0),
                Width = StyleDimension.Fill,
                DragIgnore = true,
                Left = new StyleDimension(16f, 0f)

            };
            fieldName.JoinParent(fPanel);
            var fType = fInfo.FieldType;
            if (fType == typeof(bool))
            {
                SUISwitch uiSwitch = new SUISwitch(
                    () => (bool)fInfo.GetValue(DummyNPC.LocalConfig),
                    flag =>
                    {
                        fInfo.SetValueDirect(__makeref(DummyNPC.LocalConfig), flag);
                        SyncDummyModule.Get(null, Main.myPlayer, DummyNPC.LocalConfig).Send(runLocally: true);
                    }, ""
                    )
                {
                    HAlign = 1f,
                    Width = new StyleDimension(60, 0),
                    Height = StyleDimension.Fill
                };
                uiSwitch.JoinParent(fPanel);
            }
            else if (fType == typeof(int))
            {
                SUISlider<int> sUISlider = new SUISlider<int>(
                    () => (int)fInfo.GetValue(DummyNPC.LocalConfig),
                    obj =>
                    {
                        fInfo.SetValueDirect(__makeref(DummyNPC.LocalConfig), obj);
                        SyncDummyModule.Get(null, Main.myPlayer, DummyNPC.LocalConfig).Send(runLocally: true);
                    }, "",
                    fInfo.Name == "LifeMax" ? 1 : 0, fInfo.Name == "LifeMax" ? 400000 : 1000, fInfo.Name == "LifeMax" ? 200000 : 0
                    )
                {
                    HAlign = 1f,
                    Width = new StyleDimension(180, 0),
                    Height = StyleDimension.Fill
                };
                sUISlider.JoinParent(fPanel);

            }
            else 
            {
                SUISlider<float> sUISlider = new SUISlider<float>(
                () => (float)fInfo.GetValue(DummyNPC.LocalConfig),
                obj =>
                {
                    fInfo.SetValueDirect(__makeref(DummyNPC.LocalConfig), obj);
                    SyncDummyModule.Get(null, Main.myPlayer, DummyNPC.LocalConfig).Send(runLocally: true);
                }, "",
                0, 100, 0
                )
                {
                    HAlign = 1f,
                    Width = new StyleDimension(180, 0),
                    Height = StyleDimension.Fill
                };
                sUISlider.JoinParent(fPanel);
            }
            //MakeSeparator();

        }
    }



    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        StartTimer.Update();
        if (!MainPanel.IsMouseHovering)
            return;

        PlayerInput.LockVanillaMouseScroll("ImproveGame: Dummy Configuration GUI");
        Main.LocalPlayer.mouseInterface = true;
    }

    public void Open()
    {
        SoundEngine.PlaySound(SoundID.MenuOpen);
        Enabled = true;
        StartTimer.Open();
        MainPanel.SetPosPixels(Main.MouseScreen.X, Main.MouseScreen.Y);
        MainPanel.Recalculate();
    }

    public void Close()
    {
        SoundEngine.PlaySound(SoundID.MenuClose);
        Enabled = false;
        StartTimer.Close();
    }

    public override bool RenderTarget2DDraw => !StartTimer.Opened;
    public override float RenderTarget2DOpacity => StartTimer.Schedule;
    public override Vector2 RenderTarget2DOrigin => MainPanel.GetDimensionsCenter();
    public override Vector2 RenderTarget2DPosition => MainPanel.GetDimensionsCenter();
    public override Vector2 RenderTarget2DScale => new Vector2(0.95f + StartTimer.Lerp(0, 0.05f));
}