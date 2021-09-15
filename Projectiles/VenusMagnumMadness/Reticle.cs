using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MisterCarlosMod.Projectiles.VenusMagnumMadness
{
    public class Reticle : ModProjectile
    {
        private const float InitialRotation = MathHelper.Pi;
        private const float InitialScale = 2f;

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 32;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.rotation = InitialRotation;
            projectile.alpha = 255;
            projectile.scale = InitialScale;
            projectile.timeLeft = 90;
        }

        public override bool CanDamage()
        {
            return false;
        }

        public override void AI()
        {
            int appearDuration = 30;
            int fadeSpeed = (int)Math.Ceiling(255f / appearDuration);
            float rotationSpeed = InitialRotation / appearDuration;
            float scaleSpeed = (InitialScale - 1f) / appearDuration;

            if (projectile.timeLeft < appearDuration)
            {
                projectile.alpha = Math.Min(projectile.alpha + fadeSpeed, 255);
            } else
            {
                projectile.alpha = Math.Max(projectile.alpha - fadeSpeed, 0);
            }

            if (projectile.rotation > 0f)
            {
                projectile.rotation = Math.Max(projectile.rotation - rotationSpeed, 0f);
            }

            if (projectile.scale > 1f)
            {
                projectile.scale = Math.Max(projectile.scale - scaleSpeed, 1f);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            Vector2 origin = texture.Size() / 2f;

            spriteBatch.Draw(
                texture,
                projectile.Center - Main.screenPosition,
                null,
                Color.White,
                projectile.rotation,
                origin,
                projectile.scale,
                SpriteEffects.None,
                0f);

            return false;
        }
    }
}
