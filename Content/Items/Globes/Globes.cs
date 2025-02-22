﻿using ImproveGame.Common.Conditions;
using ImproveGame.Common.ModSystems;
using ImproveGame.Content.Items.Globes.Core;
using ImproveGame.Content.Projectiles;

namespace ImproveGame.Content.Items.Globes;

public class AetherGlobe : OnceForAllGlobe
{
    public class AetherGlobeProj : GlobeProjBase
    {
        public override ModItem GetModItemDummy() => ModContent.GetInstance<AetherGlobe>();
    }

    public override StructureDatas.UnlockID StructureType => StructureDatas.UnlockID.Shimmer;

    public override bool NotFoundCheck() => StructureDatas.ShimmerPosition == default;

    protected override Recipe AddCraftingMaterials(Recipe recipe) =>
        recipe.AddIngredient(ItemID.Glass, 10)
            .AddIngredient(ItemID.StoneBlock, 80)
            .AddRecipeGroup(RecipeSystem.AnyGem, 6)
            .AddTile(TileID.WorkBenches)
            .AddCondition(ConfigCondition.EnableMinimapMarkC);
}

public class DungeonGlobe : OnceForAllGlobe
{
    public class DungeonGlobeProj : GlobeProjBase
    {
        public override ModItem GetModItemDummy() => ModContent.GetInstance<DungeonGlobe>();
    }

    public override StructureDatas.UnlockID StructureType => StructureDatas.UnlockID.Dungeon;

    protected override Recipe AddCraftingMaterials(Recipe recipe) =>
        recipe.AddRecipeGroup(RecipeGroupID.Sand, 10)
            .AddIngredient(ItemID.DirtBlock, 30)
            .AddIngredient(ItemID.StoneBlock, 50)
            .AddTile(TileID.WorkBenches)
            .AddCondition(ConfigCondition.EnableMinimapMarkC);
}

public class TempleGlobe : OnceForAllGlobe
{
    public class TempleGlobeProj : GlobeProjBase
    {
        public override ModItem GetModItemDummy() => ModContent.GetInstance<TempleGlobe>();
    }

    public override StructureDatas.UnlockID StructureType => StructureDatas.UnlockID.Temple;

    public override bool NotFoundCheck() => StructureDatas.TemplePosition == default;

    protected override Recipe AddCraftingMaterials(Recipe recipe) =>
        recipe.AddIngredient(ItemID.Glass, 10)
            .AddIngredient(ItemID.MudBlock, 100)
            .AddIngredient(ItemID.JungleSpores, 3)
            .AddIngredient(ItemID.Stinger, 3)
            .AddTile(TileID.WorkBenches)
            .AddCondition(ConfigCondition.EnableMinimapMarkC);
}

public class PyramidGlobe : OnceForAllGlobe
{
    public class PyramidGlobeProj : GlobeProjBase
    {
        public override ModItem GetModItemDummy() => ModContent.GetInstance<PyramidGlobe>();
    }

    public override StructureDatas.UnlockID StructureType => StructureDatas.UnlockID.Pyramids;

    protected override Recipe AddCraftingMaterials(Recipe recipe) =>
        recipe.AddRecipeGroup(RecipeGroupID.Sand, 50)
            .AddTile(TileID.WorkBenches)
            .AddCondition(ConfigCondition.EnableMinimapMarkC);
}

public class FloatingIslandGlobe : OnceForAllGlobe
{
    public class FloatingIslandGlobeProj : GlobeProjBase
    {
        public override ModItem GetModItemDummy() => ModContent.GetInstance<FloatingIslandGlobe>();
    }

    public override StructureDatas.UnlockID StructureType => StructureDatas.UnlockID.FloatingIslands;

    public override bool NotFoundCheck() =>
        StructureDatas.SkyHousePositions.Count is 0 && StructureDatas.SkyLakePositions.Count is 0;

    protected override Recipe AddCraftingMaterials(Recipe recipe) =>
        recipe.AddIngredient(ItemID.Glass, 18)
            .AddRecipeGroup(RecipeGroupID.Wood, 100)
            .AddIngredient(ItemID.Rope, 100)
            .AddTile(TileID.WorkBenches)
            .AddCondition(ConfigCondition.EnableMinimapMarkC);
}