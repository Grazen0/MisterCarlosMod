using Terraria;
using Terraria.ModLoader;

namespace MisterCarlosMod.Items
{
    public class GlobalWings : GlobalItem
    {
        public override void VerticalWingSpeeds(Item item, Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            MisterCarlosPlayer modPlayer = player.GetModPlayer<MisterCarlosPlayer>();
            if (!modPlayer.efeCurse) return;

            float reduceFactor = 0.75f;
            ascentWhenFalling *= reduceFactor + 0.1f;
            ascentWhenRising *= reduceFactor;
            maxCanAscendMultiplier *= reduceFactor;
            maxAscentMultiplier *= reduceFactor;
            constantAscend *= reduceFactor;
        }

        public override void HorizontalWingSpeeds(Item item, Player player, ref float speed, ref float acceleration)
        {
            MisterCarlosPlayer modPlayer = player.GetModPlayer<MisterCarlosPlayer>();
            if (!modPlayer.efeCurse) return;

            speed *= 0.8f;
        }
    }
}
