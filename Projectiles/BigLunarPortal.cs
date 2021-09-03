using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MisterCarlosMod.Projectiles
{
    public class BigLunarPortal : ModProjectile
    {
        public override string Texture => "Terraria/Projectile_" + ProjectileID.MoonlordTurret;

        public static int LaserCount => Main.expertMode ? 5 : 4;

        private const float telegraphDuration = 60f;
        private const float scaleFactor = 1.5f;

        // Local AI
        private float attackTimer = 0f;
        private bool playSound = true;
        private float speedReduction = -1f;

        public float StopTimer
        {
            get => projectile.ai[0];
            set => projectile.ai[0] = value;
        }

        public float LaserOffset
        {
            get => projectile.ai[1];
            set => projectile.ai[1] = value;
        }

        public override void SetDefaults()
        {
            projectile.penetrate = -1;
            projectile.width = projectile.height = 50;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.friendly = false;
            projectile.hostile = true;
            projectile.alpha = 255;
        }

        public override void AI()
        {
            if (playSound)
            {
                Main.PlaySound(SoundID.Item78, projectile.Center);
                playSound = false;
            }

            VanillaAnimationThingy();

            if (StopTimer > 0)
                StopTimer--;
            else
            {
                float velocityLength = projectile.velocity.Length();

                if (velocityLength < 0.2f)
                {
                    projectile.velocity = Vector2.Zero;
                    float attackAt = telegraphDuration + 10f;

                    attackTimer++;
                    if (Main.netMode != NetmodeID.MultiplayerClient && attackTimer == attackAt)
                    {
                        for (int i = 0; i < LaserCount; i++)
                        {
                            int projID = Projectile.NewProjectile(projectile.Center, Vector2.Zero, ModContent.ProjectileType<BigLunarPortalLaser>(), projectile.damage, 0f, projectile.owner, projectile.whoAmI);

                            float angleBetweenLasers = MathHelper.TwoPi / LaserCount;
                            Main.projectile[projID].rotation = LaserOffset + (i * angleBetweenLasers);
                        }
                    } else if (attackTimer > attackAt)
                    {
                        projectile.timeLeft = (int)Math.Min(projectile.timeLeft, 100f);
                    }
                } else
                {
                    float stopDuration = 20f;

                    if (speedReduction == -1f)
                    {
                        speedReduction = velocityLength / stopDuration;
                    }

                    projectile.velocity *= (velocityLength - speedReduction) / velocityLength;
                }
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Código fuente del vanilla
            int val = 255 - projectile.alpha;
            return new Color(val, val, val, val);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            // Más del código fuente vanilla
            // Ni idea de como funciona pero hace la animación de vórtice
            Texture2D texture = Main.projectileTexture[projectile.type];
            Texture2D extraTexture = Main.extraTexture[50];

            Vector2 position = projectile.position + new Vector2(projectile.width, projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
            Color lightning = Lighting.GetColor((int)(projectile.position.X + projectile.width * 0.5) / 16, (int)(projectile.position.Y + projectile.height * 0.5) / 16);
            Color alpha = projectile.GetAlpha(lightning);
            Color color12 = alpha * 0.8f;
            color12.A /= 2;
            Color color48 = Color.Lerp(alpha, Color.Black, 0.5f);
            color48.A = alpha.A;
            float num175 = 0.95f + (projectile.rotation * 0.75f).ToRotationVector2().Y * 0.1f;
            color48 *= num175;
            float scale10 = 0.6f + projectile.scale * 0.6f * num175;

            Vector2 origin = new Vector2(extraTexture.Width, extraTexture.Height) / 2f;

            spriteBatch.Draw(extraTexture, position, null, color48, -projectile.rotation + 0.35f, origin, scale10, SpriteEffects.FlipHorizontally, 0f);
            spriteBatch.Draw(extraTexture, position, null, alpha, -projectile.rotation, origin, projectile.scale, SpriteEffects.FlipHorizontally, 0f);
            spriteBatch.Draw(extraTexture, position, null, alpha * 0.8f, projectile.rotation * 0.5f, origin, projectile.scale * 0.9f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, color12, -projectile.rotation * 0.7f, origin, projectile.scale, SpriteEffects.FlipHorizontally, 0f);

            // Dibujar líneas de advertencia
            float telegraphAt = 0f;
            if (attackTimer > telegraphAt && attackTimer < telegraphDuration)
            {
                float fadeOut = 8f;

                Vector2 pixelOrigin = new Vector2(0f, Main.magicPixel.Height / 2f);
                position = projectile.Center - Main.screenPosition;

                Color color = Color.White;
                if (attackTimer >= telegraphAt + telegraphDuration - fadeOut)
                {
                    color *= 1f - ((attackTimer - telegraphAt - (telegraphDuration - fadeOut)) / fadeOut);
                }

                float angleBetweenLasers = MathHelper.TwoPi / LaserCount;
                for (int i = 0; i < LaserCount; i++)
                {
                    float rotation = LaserOffset + (i * angleBetweenLasers) + MathHelper.PiOver2;
                    Rectangle rect = new Rectangle((int)position.X, (int)position.Y, 4000, 6);
                    spriteBatch.Draw(Main.magicPixel, rect, Main.magicPixel.Bounds, color, rotation, pixelOrigin, SpriteEffects.None, 0f);
                }
            }

            // Dibujar manualmente el proyectile porque el vanilla no pone bien la escala
            spriteBatch.Draw(texture, position, null, color12, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);

            return false;
        }

        private void VanillaAnimationThingy()
        {
            // Partículas de vórtice copiadas del código fuente vanilla
            if (Main.rand.NextBool(2))
            {
                Vector2 rotation = Vector2.UnitY.RotatedByRandom(6.2831854820251465);
                int dustID = Dust.NewDust(projectile.Center - rotation * 30f * scaleFactor, 0, 0, DustID.Vortex);
                Main.dust[dustID].noGravity = true;
                Main.dust[dustID].position = projectile.Center - rotation * Main.rand.Next((int)(10 * scaleFactor), (int)(21 * scaleFactor));
                Main.dust[dustID].velocity = rotation.RotatedBy(1.5707963705062866) * 6f * scaleFactor;
                Main.dust[dustID].scale = 0.5f + Main.rand.NextFloat() * scaleFactor;
                Main.dust[dustID].fadeIn = 0.5f * scaleFactor;
                Main.dust[dustID].customData = projectile.Center;
            }

            if (Main.rand.NextBool(2))
            {
                Vector2 rotation = Vector2.UnitY.RotatedByRandom(6.2831854820251465f);
                int dustID = Dust.NewDust(projectile.Center - rotation * 30f * scaleFactor, 0, 0, DustID.Granite);
                Main.dust[dustID].noGravity = true;
                Main.dust[dustID].position = projectile.Center - rotation * 30f * scaleFactor;
                Main.dust[dustID].velocity = rotation.RotatedBy(-1.5707963705062866f) * 5f;
                Main.dust[dustID].scale = 0.5f + Main.rand.NextFloat() * scaleFactor;
                Main.dust[dustID].fadeIn = 0.5f * scaleFactor;
                Main.dust[dustID].customData = projectile.Center;
            }

            int fadeDuration = 26;
            int fadeSpeed = (int)Math.Ceiling(255f / fadeDuration);

            if (projectile.timeLeft > fadeDuration)
            {
                projectile.alpha = Math.Max(projectile.alpha - fadeSpeed, 0);
            } else
            {
                projectile.alpha = Math.Min(projectile.alpha + fadeSpeed, 255);
            }

            projectile.rotation -= projectile.direction * MathHelper.TwoPi / 120f;
            projectile.scale = projectile.Opacity * scaleFactor;

            Lighting.AddLight(projectile.Center, new Vector3(0.3f, 0.9f, 0.7f) * projectile.Opacity);
        }
    }
}
