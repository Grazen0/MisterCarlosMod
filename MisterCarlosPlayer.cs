using Terraria.ModLoader;

namespace MisterCarlosMod
{
    public class MisterCarlosPlayer : ModPlayer
    {
        public bool efeCurse = false;

        public override void ResetEffects()
        {
            efeCurse = false;
        }
    }
}
