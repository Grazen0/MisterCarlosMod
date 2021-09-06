using Terraria;
using Terraria.ModLoader;
using MisterCarlosMod.NPCs.MisterCarlos;

namespace MisterCarlosMod.Buffs
{
    public class EfeCurse : ModBuff
    {
        public override void SetDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<MisterCarlosPlayer>().efeCurse = true;
            player.moveSpeed *= 0.75f;
            

            int carlosID = NPC.FindFirstNPC(ModContent.NPCType<MisterCarlos>());
            if (carlosID == -1)
            {
                DelBuff(player, ref buffIndex);
            } else
            {
                NPC carlos = Main.npc[carlosID];

                if (!carlos.active || carlos.Distance(player.Center) > MisterCarlos.CurseRange)
                {
                    DelBuff(player, ref buffIndex);
                }
                else
                {
                    player.buffTime[buffIndex] = 18000;
                }
            }
        }

        private void DelBuff(Player player, ref int buffIndex)
        {
            player.DelBuff(buffIndex);
            buffIndex--;
        }
    }
}
