using System.IO;
using Terraria.ModLoader;

namespace MisterCarlosMod.NPCs
{
    public abstract class NPCAttack<NPC> where NPC : ModNPC
    {
        public readonly NPC modNPC;

        public abstract float Duration
        {
            get;
        }

        public NPCAttack(NPC modNPC)
        {
            this.modNPC = modNPC;
        }

        public abstract void AI();

        public virtual void Initialize() { }

        public virtual void ScaleExpertStats(int numPlayers, float bossLifeScale) { }

        public virtual void SendExtraAI(BinaryWriter writer) { }

        public virtual void ReceiveExtraAI(BinaryReader reader) { }
    }
}
