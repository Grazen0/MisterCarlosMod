using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MisterCarlosMod.Projectiles
{
    public class StardustCellBorder : ModProjectile
    {
        public override string Texture => "Terraria/NPC_" + NPCID.StardustCellBig;
        public Vector2 Center
        {
            get => new Vector2(projectile.ai[0], projectile.ai[1]);
            set
            {
                projectile.ai[0] = value.X;
                projectile.ai[1] = value.Y;
            }
        }

        private const float ScaleDuration = 30f;
        private float scale = 0f;

        public override void SetStaticDefaults()
        {
            Main.projFrames[projectile.type] = Main.projFrames[NPCID.StardustCellBig];
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 50;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.timeLeft = 720;
        }

        public override void AI()
        {
            // Frame animation
            if (++projectile.frameCounter >= 5)
            {
                projectile.frame = (projectile.frame + 1) % Main.projFrames[projectile.type];
                projectile.frameCounter = 0;
            }

            // Spin around center
            float rotationSpeed = MathHelper.TwoPi / 300f;

            Vector2 rotation = Center - projectile.Center;
            projectile.Center = Center + rotation.RotatedBy(rotationSpeed);

            projectile.rotation -= rotationSpeed;

            float scaleSpeed = 1f / ScaleDuration;

            if (projectile.timeLeft > ScaleDuration)
            {
                scale = MathHelper.Min(scale + scaleSpeed, 1f);
            } else
            {
                scale = MathHelper.Max(scale - scaleSpeed, 0f);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            float scaleFactor = 1.5f;
            Texture2D texture = Main.projectileTexture[projectile.type];

            int frameHeight = texture.Height / Main.npcFrameCount[NPCID.StardustCellBig];
            Vector2 origin = new Vector2(texture.Width, frameHeight) / 2f;

            Rectangle sourceRectangle = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);

            // Draw trail
            Texture2D trailTexture = mod.GetTexture("Projectiles/StardustCellTrail");
            int trails = 6;
            float trailSeparation = MathHelper.ToRadians(0.5f);

            for (int i = 1; i <= trails; i++)
            {
                Vector2 toCenter =  projectile.Center - Center;
                Vector2 position = Center + toCenter.RotatedBy(-(i * trailSeparation));

                spriteBatch.Draw(
                    trailTexture,
                    position - Main.screenPosition,
                    sourceRectangle,
                    Color.Cyan * 0.2f,
                    projectile.rotation,
                    origin,
                    scale * scaleFactor,
                    SpriteEffects.None,
                    0f);
            }

            // Draw projectile manually bcs scale issues
            spriteBatch.Draw(
                texture,
                projectile.Center - Main.screenPosition,
                sourceRectangle,
                Color.White,
                projectile.rotation,
                origin,
                scale * scaleFactor,
                SpriteEffects.None,
                0f);

            return false;
        }

        public override bool CanHitPlayer(Player target)
        {
            return projectile.timeLeft > (ScaleDuration / 2f);
        }

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {
            target.noKnockback = false;
        }

        public override void Kill(int timeLeft)
        {
            // Create dust
            Color dustColor = new Color(136, 226, 255);
            for (int d = 0; d < 10; d++)
            {
                int dustID = Dust.NewDust(projectile.Center, 0, 0, DustID.Snow, 0, 0, 0, dustColor, 2f);
                Main.dust[dustID].fadeIn *= 2f;
                Main.dust[dustID].velocity *= 3f;
                Main.dust[dustID].noGravity = true;
            }
        }
    }
}
