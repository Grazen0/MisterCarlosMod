using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MisterCarlosMod.Projectiles
{
    public class StardustBulllet : ModProjectile
    {
        public override string Texture => "Terraria/Projectile_" + ProjectileID.StardustCellMinionShot;

        public override void SetStaticDefaults()
        {
            Main.projFrames[projectile.type] = Main.projFrames[ProjectileID.StardustCellMinionShot];
        }

        public override void SetDefaults()
        {
            projectile.scale = 1.5f;
            projectile.width = projectile.height = 12;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = 300;

            drawOriginOffsetY = 0;
        }

        public override void AI()
        {
            // Animate frames
            if (++projectile.frameCounter > 7)
            {
                projectile.frameCounter = 0;
                projectile.frame = (projectile.frame + 1) % Main.projFrames[projectile.type];
            }
        }
    }
}
