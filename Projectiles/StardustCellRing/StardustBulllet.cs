using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MisterCarlosMod.Projectiles.StardustCellRing
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
            projectile.width = projectile.height = 12;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = 300;
        }

        public override void AI()
        {
            // Animate frames
            if (++projectile.frameCounter > 7)
            {
                projectile.frameCounter = 0;
                projectile.frame = (projectile.frame + 1) % Main.projFrames[projectile.type];
            }

            int scaleDuration = 30;
            float scaleSpeed = 1f / scaleDuration;

            if (projectile.timeLeft < scaleDuration)
            {
                projectile.scale = MathHelper.Max(projectile.scale - scaleSpeed, 0f);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            // Draw manually
            Texture2D texture = Main.projectileTexture[projectile.type];

            int frameHeight = texture.Height / Main.projFrames[projectile.type];

            Rectangle sourceRectangle = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
            Vector2 origin = new Vector2(texture.Width, frameHeight - 2f) / 2f;

            spriteBatch.Draw(
                texture,
                projectile.Center - Main.screenPosition,
                sourceRectangle,
                lightColor,
                projectile.rotation,
                origin,
                projectile.scale * 1.5f,
                SpriteEffects.None,
                0f);

            return false;
        }

        public override bool CanHitPlayer(Player target)
        {
            return projectile.scale > 0.5f;
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            // TODO - Fix shit here
            drawCacheProjsBehindProjectiles.Add(index);
        }

        public override void Kill(int timeLeft)
        {
            // Create dust
            Color dustColor = new Color(136, 226, 255);
            for (int d = 0; d < 10; d++)
            {
                int dustID = Dust.NewDust(projectile.Center, 0, 0, DustID.Snow, 0, 0, 0, dustColor);
                Main.dust[dustID].velocity *= 1.5f;
                Main.dust[dustID].noGravity = true;
            }
        }
    }
}
