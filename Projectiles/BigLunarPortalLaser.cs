using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MisterCarlosMod.Projectiles
{
    public class BigLunarPortalLaser : ModProjectile
    {
        public override string Texture => "Terraria/Projectile_" + ProjectileID.MoonlordTurretLaser;

        private const int maxSegments = 100;
        private Projectile Owner => Main.projectile[(int)projectile.ai[0]];
        private float ScaleFactor => 1.2f * Owner.scale * projectile.scale;
        private float Length => maxSegments * ScaleFactor * 20;

        private bool playSound = true;

        public override void SetDefaults()
        {
            projectile.penetrate = -1;
            projectile.width = projectile.height = 40;
            projectile.friendly = false;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 0f;
        }

        public override void AI()
        {
            if (playSound)
            {
                Main.PlaySound(SoundID.Item117, projectile.Center);
                playSound = false;
            }
            projectile.scale = MathHelper.Min(projectile.scale + 0.1f, 1f);

            if (Owner.scale < 0.1f)
            {
                projectile.Kill();
            }
            for (int d = 0; d < 10; d++)
            {
                Vector2 rotation = Vector2.UnitY.RotatedBy(projectile.rotation);
                Vector2 position = rotation * Main.rand.NextFloat(Length);
                float velocity = 2f + Main.rand.NextFloat(3f);

                int dustID = Dust.NewDust(Owner.Center + position, 0, 0, DustID.Clentaminator_Cyan);
                Main.dust[dustID].noGravity = true;
                Main.dust[dustID].velocity = rotation.RotatedBy(MathHelper.PiOver2 * (Main.rand.NextBool() ? 1f : -1f)) * velocity;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            int frames = 7;
            int frameHeight = (texture.Height - ((frames - 1) * 2) - 30) / (frames - 1);

            Vector2 origin = new Vector2(texture.Width / 2, frameHeight / 2);

            Vector2 position = Owner.Center;

            for (int i = 0; i < maxSegments; i++)
            {
                int y;

                if (i == maxSegments - 1)
                {
                    // Final del láser
                    y = texture.Height - frameHeight;
                }
                else if (i > 0)
                {
                    // Cuerpo del láser
                    int bodyFrames = frames - 2;
                    int frameIndex = 1 + (i % bodyFrames);

                    y = (2 + frameHeight) * frameIndex;
                }
                else
                {
                    // Inicio del láser
                    y = 0;
                }

                Rectangle sourceRectangle = new Rectangle(0, y, texture.Width, frameHeight + (i == maxSegments - 1 ? 10 : 0));
                if (i == 0)
                {
                    int remove = 16;
                    sourceRectangle.Y += remove;
                    sourceRectangle.Height -= remove;
                }

                spriteBatch.Draw(texture, position - Main.screenPosition, sourceRectangle, Color.White, projectile.rotation, origin, ScaleFactor, SpriteEffects.None, 0f);

                position += new Vector2(0f, frameHeight * ScaleFactor).RotatedBy(projectile.rotation);
            }

            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Projectile owner = Main.projectile[(int)projectile.ai[0]];

            Vector2 unit = Vector2.UnitY.RotatedBy(projectile.rotation);
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), owner.Center, owner.Center + unit * Length);
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindProjectiles.Remove(index);
        }
    }
}
