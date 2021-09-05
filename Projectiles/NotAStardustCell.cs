using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MisterCarlosMod.Projectiles
{
    public class NotAStardustCell : ModProjectile
    {
        public override string Texture => "Terraria/Projectile_" + ProjectileID.StardustCellMinion;

        private float attackTimer = 0f;
        private float scale = 0f;

        public Player Target
        {
            get => Main.player[(int)projectile.ai[0]];
        }

        public float MoveTimer
        {
            get => projectile.ai[1];
            set => projectile.ai[1] = value;
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[projectile.type] = Main.projFrames[ProjectileID.StardustCellMinion];
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 20;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.timeLeft = 500;
        }


        public override void AI()
        {
            // Animate frames
            if (++projectile.frameCounter > 10)
            {
                projectile.frameCounter = 0;
                projectile.frame = (projectile.frame + 1) % Main.projFrames[ProjectileID.StardustCellMinion];
            }

            projectile.rotation += MathHelper.TwoPi / 180f;

            float scaleDuration = 20f;
            float scaleSpeed = 1f / scaleDuration;

            if (projectile.timeLeft > scaleDuration)
            {
                scale = MathHelper.Min(scale + scaleSpeed, 1f);
            } else
            {
                scale = MathHelper.Max(scale - scaleSpeed, 0f);
            }

            float attackSpeed = 60f;

            if (--MoveTimer < 0f)
            {
                float velocityLength = projectile.velocity.Length();

                if (velocityLength > 0.3f)
                {
                    projectile.velocity *= 0.9f;
                } else if (velocityLength > 0f)
                {
                    projectile.velocity = Vector2.Zero;
                } else
                {
                    attackTimer++;
                    if (Main.netMode != NetmodeID.MultiplayerClient && attackTimer >= 60f)
                    {
                        // Shoot cells
                        int cells = Main.expertMode ? 7 : 5;
                        float separation = MathHelper.TwoPi / cells;

                        Vector2 startRotation = Vector2.UnitY.RotatedByRandom(separation);

                        for (int i = 0; i < cells; i++)
                        {
                            Vector2 velocity = startRotation.RotatedBy(i * separation) * 5f;
                            Projectile.NewProjectile(
                                projectile.Center,
                                velocity,
                                ModContent.ProjectileType<StardustBulllet>(),
                                projectile.damage,
                                0f,
                                Main.myPlayer);
                        }

                        attackTimer -= attackSpeed;
                        projectile.netUpdate = true;
                    }
                }
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
                scale * 1.5f,
                SpriteEffects.None,
                0f);

            return false;
        }

        public override void Kill(int timeLeft)
        {
            // Create dust
            for (int d = 0; d < 15; d++)
            {
                int dustID = Dust.NewDust(projectile.Center, 0, 0, DustID.FartInAJar, 0, 0, 0, Color.Blue * 0.9f, 1.5f);
                Main.dust[dustID].velocity *= 3f;
                Main.dust[dustID].fadeIn *= 1f;
                Main.dust[dustID].noGravity = true;
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(attackTimer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            attackTimer = reader.ReadSingle();
        }
    }
}
