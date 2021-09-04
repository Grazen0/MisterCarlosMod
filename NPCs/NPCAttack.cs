using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace MisterCarlosMod.NPCs
{
    public abstract class NPCAttack<NPC> where NPC : ModNPC
    {
        public readonly Mod mod;
        public readonly NPC modNPC;

        public abstract float Duration
        {
            get;
        }

        public NPCAttack(NPC modNPC)
        {
            this.modNPC = modNPC;
            mod = modNPC.mod;
        }

        public abstract void AI();

        public virtual void Initialize() { }

        public virtual void ScaleExpertStats(int numPlayers, float bossLifeScale) { }

        public virtual void SendExtraAI(BinaryWriter writer) { }

        public virtual void ReceiveExtraAI(BinaryReader reader) { }

        public virtual bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            return true;
        }

        public virtual void PostDraw(SpriteBatch spriteBatch, Color drawColor) { }
    }
}
