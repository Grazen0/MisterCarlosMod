using Microsoft.Xna.Framework;
using Terraria;

namespace MisterCarlosMod.NPCs.MisterCarlos.Attacks
{
    public class TestAttack : NPCAttack<MisterCarlos>
    {
        public override float Duration => 180f;

        public TestAttack(MisterCarlos carlos) : base(carlos)
        {

        }

        public override void AI()
        {
            NPC npc = modNPC.npc;

            float speed = 6f + (2f * modNPC.Phase);
            float inertia = 40f;
            Vector2 velocity = npc.DirectionTo(modNPC.Target.Center) * speed;
            
            npc.velocity = (npc.velocity * (inertia - 1) + velocity) / inertia;
        }
    }
}
