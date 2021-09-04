using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MisterCarlosMod.Projectiles
{
    public class SpiralStarlightLight : ModProjectile
    {
        public override string Texture => "MisterCarlosMod/Projectiles/StarlightLight";

        private Color color;
        private bool init = true;

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 20;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 0f;
            projectile.timeLeft = 180;
        }

        public override void AI()
        {
            if (init)
            {
                color = Utils.HsvToColor(projectile.ai[0], 0.4f, 1f);
                Main.PlaySound(SoundID.Item1, projectile.Center);
                init = false;
            }

            projectile.scale = MathHelper.Min(projectile.scale + 0.08f, 1f);
            projectile.rotation = projectile.velocity.ToRotation();

            projectile.velocity *= 1.02f;

            Lighting.AddLight(projectile.Center, color.ToVector3());
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            // Manually draw projectile
            Texture2D texture = Main.projectileTexture[projectile.type];

            Color drawColor = GetAlpha(lightColor) ?? lightColor;
            Vector2 origin = texture.Size() / 2f;

            spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, null, drawColor, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);

            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return color * (1f - (projectile.alpha / 255f));
        }
    }
}
