using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MisterCarlosMod.Projectiles.Celebration
{
    public class CelebrationSparkle : ModProjectile
    {
        private float scale = 0f;

        public int ColorType
        {
            get => (int)projectile.ai[0];
            set => projectile.ai[0] = value;
        }

        public override void SetDefaults()
        {
            projectile.hostile = true;
            projectile.width = projectile.height = 20;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        public override void AI()
        {
            scale = MathHelper.Min(scale + 0.05f, 1f);

            projectile.velocity *= 1.02f;
            projectile.rotation = projectile.velocity.ToRotation();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            Vector2 origin = texture.Size() / 2f;

            spriteBatch.Draw(
                texture,
                projectile.Center - Main.screenPosition,
                null,
                GetAlpha(lightColor) ?? lightColor,
                projectile.rotation,
                origin,
                scale,
                SpriteEffects.None,
                0f);


            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            float hue = 0f; // Red
            switch (ColorType)
            {
                case 1:
                    hue = 120f; // Green
                    break;
                case 2:
                    hue = 240f; // Blue
                    break;
                case 3:
                    hue = 60f; // Yellow
                    break;
            }

            Color color = Utils.HsvToColor(hue, 0.8f, 1f);
            return Color.Lerp(color, lightColor, 0.3f);
        }
    }
}
