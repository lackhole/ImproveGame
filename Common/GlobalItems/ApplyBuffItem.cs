﻿using ImproveGame.Common.Players;
using ImproveGame.Common.Systems;
using ImproveGame.Interface.GUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria.Localization;

namespace ImproveGame.Common.GlobalItems
{
    public class ApplyBuffItem : GlobalItem
    {
        // 特殊药水
        public static readonly List<int> SpecialPotions = new() { 2350, 2351, ItemID.WormholePotion, ItemID.PotionOfReturn };
        // 增益 Tile 巴斯特雕像，篝火，红心灯笼，星星瓶，向日葵，弹药箱，施法桌，水晶球，蛋糕块，利器站，水蜡烛，和平蜡烛
        public static readonly List<List<int>> BUFFTiles = new() { new() { 506, -1, 215 }, new() { 215, -1, 87 }, new() { 42, 9, 89 }, new() { 42, 7, 158 }, new() { 27, -1, 146 }, new() { 287, -1, 93 }, new() { 354, -1, 150 }, new() { 125, -1, 29 }, new() { 621, -1, 192 }, new() { 377, -1, 159 }, new() { 49, -1, 86 }, new() { 372, -1, 157 } };

        public static void UpdateInventoryGlow(Item item) {
            bool globalItemNotNull = item.TryGetGlobalItem<GlobalItemData>(out var globalItem);
            if (globalItemNotNull)
                globalItem.InventoryGlow = false;

            int buffType = GetItemBuffType(item);
            if (buffType is not -1) {
                HideBuffSystem.BuffTypesShouldHide[buffType] = true;
                if (globalItemNotNull)
                    globalItem.InventoryGlow = true;
            }
            // 非增益药剂
            if (MyUtils.Config.NoConsume_Potion && item.stack >= 30 && SpecialPotions.Contains(item.type) && globalItemNotNull) {
                globalItem.InventoryGlow = true;
            }
            // 随身增益站：旗帜
            if (MyUtils.Config.NoPlace_BUFFTile_Banner) {
                if (item.createTile == TileID.Banners) {
                    int style = item.placeStyle;
                    int frameX = style * 18;
                    int frameY = 0;
                    if (style >= 90) {
                        frameX -= 1620;
                        frameY += 54;
                    }
                    if (globalItemNotNull && (frameX >= 396 || frameY >= 54)) {
                        globalItem.InventoryGlow = true;
                    }
                }
            }
            // 弹药
            if (MyUtils.Config.NoConsume_Ammo && item.stack >= 3996 && item.ammo > 0 && globalItemNotNull) {
                globalItem.InventoryGlow = true;
            }
        }

        public static int GetItemBuffType(Item item) {
            if (MyUtils.Config.NoConsume_Potion) {
                // 普通药水
                if (item.stack >= 30 && item.active) {
                    if (item.buffType > 0)
                        return item.buffType;
                    // 其他Mod的，自行添加了引用
                    if (ModIntegrationsSystem.ModdedPotionBuffs.ContainsKey(item.type))
                        return ModIntegrationsSystem.ModdedPotionBuffs[item.type];
                }
            }
            // 随身增益站：普通
            if (MyUtils.Config.NoPlace_BUFFTile) {
                IsBuffTileItem(item, out int buffType);
                if (buffType is not -1)
                    return buffType;

                if (item.type == ItemID.HoneyBucket) {
                    return BuffID.Honey;
                }
            }
            return -1;
        }

        public static bool IsBuffTileItem(Item item, out int buffType) {
            // 会给玩家buff的雕像
            for (int i = 0; i < BUFFTiles.Count; i++) {
                if (item.createTile == BUFFTiles[i][0] && (item.placeStyle == BUFFTiles[i][1] || BUFFTiles[i][1] == -1)) {
                    buffType = BUFFTiles[i][2];
                    return true;
                }
            }
            // 其他Mod的，自行添加了引用
            foreach (var moddedBuff in ModIntegrationsSystem.ModdedPlaceableItemBuffs) {
                if (item.type == moddedBuff.Key) {
                    buffType = moddedBuff.Value;
                    return true;
                }
            }
            buffType = -1;
            return false;
        }

        // 物品消耗
        public override bool ConsumeItem(Item item, Player player) {
            if (MyUtils.Config.NoConsume_Potion && item.stack >= 30 && (item.buffType > 0 || SpecialPotions.Contains(item.type))) {
                return false;
            }
            return base.ConsumeItem(item, player);
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
            if (!item.TryGetGlobalItem<GlobalItemData>(out var global) || !global.InventoryGlow)
                return;

            if (IsBuffTileItem(item, out _) || item.type == ItemID.HoneyBucket ||
                (item.stack >= 30 && item.buffType > 0 && item.active)) {
                int buffType = GetItemBuffType(item);

                if (buffType is -1) return;

                if (Main.mouseMiddle && Main.mouseMiddleRelease) {
                    if (BuffTrackerGUI.Visible)
                        UISystem.Instance.BuffTrackerGUI.Close();
                    else
                        UISystem.Instance.BuffTrackerGUI.Open();
                }

                TagItem.ModifyBuffTooltips(Mod, item.type, buffType, tooltips);
            }
        }

        public override bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y) {
            if (!item.TryGetGlobalItem<GlobalItemData>(out var global) || !global.InventoryGlow)
                return base.PreDrawTooltip(item, lines, ref x, ref y);

            if (IsBuffTileItem(item, out _) || item.type == ItemID.HoneyBucket ||
                (item.stack >= 30 && item.buffType > 0 && item.active)) {
                int buffType = GetItemBuffType(item);

                if (buffType is -1)
                    return base.PreDrawTooltip(item, lines, ref x, ref y);

                object arg = new {
                    BuffName = Lang.GetBuffName(buffType),
                    MaxSpawn = MyUtils.Config.SpawnRateMaxValue
                };
                if (ItemSlot.ShiftInUse)
                    TagItem.DrawTagTooltips(lines, TagItem.GenerateDetailedTags(Mod, lines, arg), x, y);
            }

            return base.PreDrawTooltip(item, lines, ref x, ref y);
        }
    }
}
