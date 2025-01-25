﻿using ImproveGame.Common.Conditions;
using ImproveGame.Common.ModSystems;
using ImproveGame.Content.Functions;
using ImproveGame.Packets.Weather;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;

namespace ImproveGame.Content.Items;

public class ShellShipInBottle : ModItem
{
    public override void SetStaticDefaults()
    {
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<ShellShipInBottle_Shimmered>();
        base.SetStaticDefaults();
    }

    public override void SetDefaults()
    {
        Item.maxStack = Item.CommonMaxStack;
        Item.rare = ItemRarityID.Blue;
        Item.Size = new Vector2(42, 20);
        Item.value = Item.sellPrice(silver: 20);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Bottle)
            .AddIngredient(ItemID.Seashell, 5)
            .AddIngredient(ItemID.Wood, 10)
            .AddIngredient(ItemID.FallenStar, 1)
            .AddTile(TileID.WorkBenches)
            .AddCondition(ConfigCondition.EnableQuickShimmerC)
            .Register();
    }
}

public class ShellShipInBottle_Shimmered : ModItem
{
    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.CombatBook);
        Item.Size = new Vector2(42, 20);
        Item.useStyle = ItemUseStyleID.RaiseLamp; // 用4的话物品会错位不在手上
        Item.value = Item.sellPrice(silver: 25);
    }

    public override bool? UseItem(Player player)
    {
        if (player.whoAmI != Main.myPlayer)
            return null;

        if (QuickShimmerSystem.Unlocked)
        {
            if (player.itemAnimation == player.itemAnimationMax)
                AddNotification(GetText("UI.QuickShimmer.AlreadyUnlocked"), Color.Pink);
            return null;
        }

        QuickShimmerUnlockPacket.Unlock();
        WorldGen.BroadcastText(NetworkText.FromKey("Mods.ImproveGame.UI.QuickShimmer.Unlocked"), Color.Pink);
        return true;
    }
}

public class QuickShimmerSystem : ModSystem
{
    public static bool Unlocked;
    public static bool Enabled => Unlocked && Config.QuickShimmer;

    public override void SaveWorldData(TagCompound tag)
    {
        if (Unlocked) tag.Add("unlocked", true);
        base.SaveWorldData(tag);
    }

    public override void LoadWorldData(TagCompound tag)
    {
        if (tag.ContainsKey("unlocked")) Unlocked = tag.GetBool("unlocked");

        base.LoadWorldData(tag);
    }

    public override void NetSend(BinaryWriter writer)
    {
        var states = new BitsByte(Unlocked);
        writer.Write(states);
    }

    public override void NetReceive(BinaryReader reader)
    {
        var states = (BitsByte) reader.ReadByte();
        Unlocked = states[0];
    }

    public override void ClearWorld()
    {
        Unlocked = false;
    }
}

public class QuickShimmerUnlockPacket : NetModule
{
    /// <summary>
    /// 这个包只要一发，就会解锁快捷微光功能，所以“解锁”就是发个包罢了
    /// </summary>
    public static void Unlock()
    {
        var module = NetModuleLoader.Get<QuickShimmerUnlockPacket>();
        module.Send(runLocally: true);
    }

    public override void Receive()
    {
        QuickShimmerSystem.Unlocked = true;

        if (Main.netMode is NetmodeID.Server)
            Send(-1, Sender);
    }
}