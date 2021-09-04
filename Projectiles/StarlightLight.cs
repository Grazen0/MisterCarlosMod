using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MisterCarlosMod.Projectiles
{
    public class StarlightLight : ModProjectile
    {
        private bool init = true;
        private readonly Color color = Utils.HsvToColor(Main.rand.NextFloat(360f), 0.5f, 1f);

        public override void SetDefaults()
        {
            projectile.hostile = true;
            projectile.width = 11;
            projectile.height = 10;
            projectile.tileCollide = false;
            projectile.alpha = 255;

            drawOffsetX = -31;
            drawOriginOffsetY = -31;
        }

        public float FreezeTimer
        {
            get => projectile.ai[0];
            set => projectile.ai[0] = value;
        }

        public float ReactivateTimer
        {
            get => projectile.ai[1];
            set => projectile.ai[1] = value;
        }

        public override void AI()
        {
            if (init)
            {
                Main.PlaySound(SoundID.Item1, projectile.Center);
                projectile.rotation = projectile.velocity.ToRotation();

                init = false;
            }

            projectile.alpha = (int)MathHelper.Max(projectile.alpha - 15, 0);

            if (--FreezeTimer < 0f)
            {
                ReactivateTimer++;
                bool isFrozen = projectile.velocity.LengthSquared() == 0f;

                if (ReactivateTimer < 240f)
                {
                    if (!isFrozen)
                    {
                        // Congelar proyectil
                        CreateDust();
                        Main.PlaySound(SoundID.NPCHit5);

                        projectile.velocity = Vector2.Zero;
                    }
                }
                else
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient && isFrozen)
                    {
                        float spread = MathHelper.PiOver4 / 2f;
                        float rotation = Main.rand.NextFloat(-spread, spread);
                        projectile.velocity = -Vector2.UnitY.RotatedBy(rotation) * 5f;

                        projectile.timeLeft = (int)MathHelper.Min(projectile.timeLeft, 120f);

                        projectile.netUpdate = true;
                    }

                    projectile.velocity.Y = Math.Min(projectile.velocity.Y + 0.23f, 15f); // Gravedad
                    projectile.rotation = projectile.velocity.ToRotation();
                }
            }

            Lighting.AddLight(projectile.Center, color.ToVector3());
        }

        public override void Kill(int timeLeft)
        {
            CreateDust();
            Main.PlaySound(SoundID.NPCHit5);
        }

        private void CreateDust()
        {
            for (int d = 0; d < 15; d++)
            {
                int dustID = Dust.NewDust(projectile.Center, 0, 0, DustID.FartInAJar, 0, 0, 0, color, 1.5f);
                Main.dust[dustID].velocity *= 3f;
                Main.dust[dustID].fadeIn *= 1f;
                Main.dust[dustID].noGravity = true;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return color * (1f - (projectile.alpha / 255f));
        }
    }
}
