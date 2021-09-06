using Terraria.ModLoader;

namespace MisterCarlosMod
{
    public class MisterCarlosPlayer : ModPlayer
    {
        public bool efeCurse = false;
        public bool stardustInfection = false;

        public override void ResetEffects()
        {
            efeCurse = false;
            stardustInfection = false;
        }

        public override void UpdateBadLifeRegen()
        {
            if (stardustInfection)
            {
                if (player.lifeRegen > 0)
                    player.lifeRegen = 0;

                player.lifeRegenTime = 0;
                player.lifeRegen -= 200;
            }
        }
    }
}
