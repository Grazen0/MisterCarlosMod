using Terraria;
using Terraria.ModLoader;

namespace MisterCarlosMod.Buffs
{
    public class StardustInfection : ModBuff
    {
        public override void SetDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<MisterCarlosPlayer>().stardustInfection = true;
        }
    }
}
