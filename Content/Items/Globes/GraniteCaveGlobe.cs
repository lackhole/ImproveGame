﻿using ImproveGame.Common.Conditions;
using ImproveGame.Content.Items.Globes.Core;
using ImproveGame.Content.Projectiles;

namespace ImproveGame.Content.Items.Globes;

public class GraniteCaveGlobe () : GlobePlentyTooltip(ItemRarityID.Quest, Item.sellPrice(silver: 30))
{
    public class GraniteCaveGlobeProj : GlobeProjBase
    {
        public override ModItem GetModItemDummy() => ModContent.GetInstance<GraniteCaveGlobe>();
    }

    public override bool RevealOperation(Projectile projectile)
    {
        if (StructureDatas.AllGraniteCavePositions.Count is 0)
        {
            if (projectile.owner == Main.myPlayer)
                AddNotification(GetLocalizedText("NotFound").ToString(), Color.PaleVioletRed * 1.4f);
            return false;
        }

        if (StructureDatas.AllGraniteCavePositions.Count <= StructureDatas.GraniteCavePositions.Count)
        {
            if (projectile.owner == Main.myPlayer)
                AddNotification(this.GetLocalizedValue("NotFound"), Color.PaleVioletRed * 1.4f);
            return false;
        }

        StructureDatas.GraniteCavePositions.Add(StructureDatas.AllGraniteCavePositions
            .Except(StructureDatas.GraniteCavePositions)
            .MinBy(position => projectile.Center.Distance(position.ToVector2() * 16)));

        var text = Language.GetText("Mods.ImproveGame.Items.GlobeBase.Reveal")
            .WithFormatArgs(this.GetLocalizedValue("BiomeName"), Main.player[projectile.owner].name);
        AddNotification(text.Value, Color.Pink);
        return true;
    }

    protected override Recipe AddCraftingMaterials(Recipe recipe) =>
        recipe.AddIngredient(ItemID.Glass, 8)
            .AddRecipeGroup(RecipeGroupID.IronBar, 4)
            .AddIngredient(ItemID.Ruby)
            .AddTile(TileID.WorkBenches)
            .AddCondition(ConfigCondition.EnableMinimapMarkC);
}