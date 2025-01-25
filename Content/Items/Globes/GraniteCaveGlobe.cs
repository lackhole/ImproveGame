using ImproveGame.Common.Conditions;

namespace ImproveGame.Content.Items.Globes;

public class GraniteCaveGlobe : ModItem
{
    private LocalizedText GetLocalizedText(string suffix) =>
        Language.GetText($"Mods.ImproveGame.Items.GlobeBase.{suffix}")
            .WithFormatArgs(Language.GetTextValue("Mods.ImproveGame.Items.GraniteCaveGlobe.BiomeName"));

    public override LocalizedText DisplayName => GetLocalizedText(nameof(DisplayName));

    public override LocalizedText Tooltip => GetLocalizedText(nameof(Tooltip) + "_1");

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 30;
        Item.useAnimation = 30;
        Item.consumable = true;
        Item.maxStack = Item.CommonMaxStack;
        Item.rare = ItemRarityID.Quest;
        Item.value = Item.sellPrice(silver: 30);
    }

    public override void AddRecipes() =>
        CreateRecipe()
            .AddIngredient(ItemID.Glass, 8)
            .AddRecipeGroup(RecipeGroupID.IronBar, 4)
            .AddIngredient(ItemID.Ruby)
            .AddTile(TileID.WorkBenches)
            .AddCondition(ConfigCondition.EnableMinimapMarkC)
            .Register();
    
    public override bool CanUseItem(Player player)
    {
        if (StructureDatas.AllGraniteCavePositions.Count is 0)
        {
            if (player.whoAmI == Main.myPlayer)
                AddNotification(GetLocalizedText("NotFound").ToString(), Color.PaleVioletRed * 1.4f);
            return false;
        }

        if (StructureDatas.AllGraniteCavePositions.Count > StructureDatas.GraniteCavePositions.Count)
            return true;

        if (player.whoAmI == Main.myPlayer)
            AddNotification(this.GetLocalizedValue("NotFound"), Color.PaleVioletRed * 1.4f);

        return false;
    }

    public override bool? UseItem(Player player)
    {
        if (StructureDatas.AllGraniteCavePositions.Count <= StructureDatas.GraniteCavePositions.Count)
            return null;

        StructureDatas.GraniteCavePositions.Add(StructureDatas.AllGraniteCavePositions
            .Except(StructureDatas.GraniteCavePositions)
            .MinBy(position => player.Center.Distance(position.ToVector2() * 16)));

        var text = Language.GetText("Mods.ImproveGame.Items.GlobeBase.Reveal")
            .WithFormatArgs(this.GetLocalizedValue("BiomeName"), player.name);
        AddNotification(text.Value, Color.Pink);
        return true;
    }
}