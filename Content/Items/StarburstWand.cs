﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ImproveGame.Content.Items
{
    public class StarburstWand : MagickWand
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.width = 40;
            Item.height = 46;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(0, 5, 0, 0);

            killSizeMax = new(40, 20);
            killSize = new(7, 5);
            extraRange = new(16, 10);
        }

        public override void AddRecipes()
        {
            CreateRecipe().AddIngredient(ModContent.ItemType<MagickWand>())
                .AddIngredient(ItemID.SoulofLight, 5)
                .AddIngredient(ItemID.SoulofNight, 5)
                .AddIngredient(ItemID.DarkShard, 1)
                .AddIngredient(ItemID.LightShard, 1)
                .AddTile(TileID.CrystalBall).Register();
        }
    }
}
