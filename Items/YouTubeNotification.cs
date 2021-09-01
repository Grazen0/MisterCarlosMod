using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MisterCarlosMod.NPCs.MisterCarlos;

namespace MisterCarlosMod.Items
{
    public class YouTubeNotification : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityBossSpawns[item.type] = 20;
        }

        public override void SetDefaults()
        {
            item.width = 30;
            item.height = 22;
            item.maxStack = 1;
            item.rare = ItemRarityID.Red;
            item.useAnimation = item.useTime = 45;
            item.useStyle = ItemUseStyleID.HoldingOut;
        }

        public override bool CanUseItem(Player player)
        {
            return !NPC.AnyNPCs(ModContent.NPCType<MisterCarlos>());
        }

        public override bool UseItem(Player player)
        {
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<MisterCarlos>());

            int wenaCabros = Main.rand.Next(6);
            Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Item, "Sounds/Item/Wena_Cabros_" + wenaCabros));

            return true;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);

            recipe.AddIngredient(ItemID.RedPressurePlate);
            recipe.AddIngredient(ItemID.LunarBar, 15);
            recipe.AddIngredient(ItemID.BrokenHeroSword);
            recipe.AddIngredient(ItemID.HerosHat);
            recipe.AddIngredient(ItemID.HerosShirt);
            recipe.AddIngredient(ItemID.HerosPants);

            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.SetResult(this);

            recipe.AddRecipe();
        }
    }
}
