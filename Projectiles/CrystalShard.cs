using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MisterCarlosMod.Projectiles
{
    public class CrystalShard : ModProjectile
    {
        public override string Texture => "Terraria/Projectile_" + ProjectileID.CrystalVileShardShaft;

        private bool continueShard = true;

        private const int LifeSpan = 60;

        public int ShardsLeft
        {
            get => (int)projectile.ai[0];
        }

        public float ContinueTimer
        {
            get => projectile.ai[1];
            set => projectile.ai[1] = value;
        }

        public override void SetDefaults()
        {
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.width = projectile.height = 32;
            projectile.timeLeft = LifeSpan;
        }

        public override void AI()
        {
            projectile.velocity *= 0f;
            int alphaSpeed = (int)Math.Ceiling(255f / LifeSpan);
            projectile.alpha = Math.Min(projectile.alpha + alphaSpeed, 255);

            if (ShardsLeft <= 0)
                continueShard = false;

            if (Main.netMode != NetmodeID.MultiplayerClient && continueShard && ContinueTimer++ > 0)
            {
                Vector2 addPosition = projectile.rotation.ToRotationVector2() * projectile.height;

                int projID = Projectile.NewProjectile(
                    projectile.Center - addPosition,
                    Vector2.Zero,
                    projectile.type,
                    projectile.damage,
                    0f,
                    projectile.owner,
                    ShardsLeft - 1);

                Main.projectile[projID].rotation = projectile.rotation;

                continueShard = false;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[ShardsLeft > 0 ? ProjectileID.CrystalVileShardShaft : ProjectileID.CrystalVileShardHead];

            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height); // TEST

            spriteBatch.Draw(
                texture,
                projectile.position - Main.screenPosition,
                null,
                projectile.GetAlpha(lightColor),
                projectile.rotation - MathHelper.PiOver2,
                origin,
                projectile.scale,
                SpriteEffects.None,
                0f);

            return false;
        }

        public override bool CanHitPlayer(Player target)
        {
            return projectile.alpha < 80;
        }
    }
}
