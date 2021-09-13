using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MisterCarlosMod.Projectiles.Celebration
{
    public class CelebrationRocket : ModProjectile
    {
        public override string Texture => "Terraria/Projectile_" + ProjectileID.RocketFireworkRed;

        private float offset = 0f;
        private bool playSound = true;

        public int RocketType
        {
            get => (int)projectile.ai[0];
            set => projectile.ai[0] = value;
        }

        public float Timer
        {
            get => projectile.ai[1];
            set => projectile.ai[1] = value;
        }

        public override void SetDefaults()
        {
            projectile.hostile = true;
            projectile.width = projectile.height = 15;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = 60;
        }

        public override void AI()
        {
            if (playSound)
            {
                Main.PlaySound(SoundID.Item65, projectile.Center);
                playSound = false;
            }

            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            VanillaDustThingy();

            // Cool rocket wave (not completely sure how I got this right but it works)
            float previousOffset = offset;
            offset = (float)Math.Sin(MathHelper.ToRadians(Timer * 10f)) * 0.3f;

            projectile.velocity = projectile.velocity.RotatedBy(-previousOffset + offset);

            // Shoot sparkle trail
            if (Main.netMode != NetmodeID.MultiplayerClient && Timer % 10 == 0)
            {
                Vector2 perpendicular = Vector2.Normalize(projectile.velocity).RotatedBy(MathHelper.PiOver2);
                Vector2 velocity = perpendicular * 5f;

                for (int direction = -1; direction <= 1; direction += 2)
                {
                    Projectile.NewProjectile(
                        projectile.Center,
                        velocity * direction,
                        ModContent.ProjectileType<CelebrationSparkle>(),
                        projectile.damage,
                        projectile.knockBack,
                        Main.myPlayer,
                        RocketType);
                }
            }

            Timer++;
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.Item14, projectile.Center);
            Fireworks();

            //if (Main.netMode != NetmodeID.MultiplayerClient)
            //{
            //    // Shoot celebration sparkles
            //    switch (RocketType)
            //    {
            //        case 0:
            //            RedExplosion();
            //            break;
            //        case 1:
            //            GreenExplosion();
            //            break;
            //        case 2:
            //            BlueExplosion();
            //            break;
            //        case 3:
            //            YellowExplosion();
            //            break;
            //    }
            //}
        }

        private void RedExplosion()
        {
            int rings = 4;
            int sparkles = 40;
            float separation = MathHelper.TwoPi / sparkles;

            for (int ring = 0; ring < rings; ring++)
            {
                float velocity = 1f + ring * 0.5f;

                for (int i = 0; i < sparkles; i++)
                {
                    Vector2 direction = (i * separation).ToRotationVector2();
                    Projectile.NewProjectile(
                        projectile.Center,
                        direction * velocity,
                        ModContent.ProjectileType<CelebrationSparkle>(),
                        projectile.damage,
                        0f,
                        Main.myPlayer,
                        RocketType);
                }
            }
        }

        //private void GreenExplosion()
        //{
        //    int rings = 2;
        //    float baseVelocity = 10f;

        //    for (int ring = 0; ring < rings; ring++)
        //    {
        //        int sparkles = ring == 0 ? 30 : 50;
        //        float separation = MathHelper.TwoPi / sparkles;

        //        for (int sparkle = 0; sparkle < sparkles; sparkle++)
        //        {
        //            Vector2 velocity = (sparkle * separation).ToRotationVector2() * baseVelocity;
        //            if (ring == 1)
        //            {
        //                velocity.X *= 2.8f;
        //            }

        //            Projectile.NewProjectile(
        //                projectile.Center,
        //                velocity,
        //                ModContent.ProjectileType<CelebrationSparkle>(),
        //                projectile.damage,
        //                0f,
        //                Main.myPlayer,
        //                RocketType);
        //        }
        //    }
        //}

        private void BlueExplosion()
        {
            int petals = 5;
            float petalSeparation = MathHelper.TwoPi / petals;
            Vector2 baseDirection = projectile.DirectionTo(Target.Center);

            for (int flower = 0; flower < 2; flower++)
            {
                float minVelocity = 5f * (flower + 1);
                float maxVelocity = 12f * (flower + 1);
                int sparklesPerHalfPetal = flower == 0 ? 5 : 8;

                for (int petal = 0; petal < petals; petal++)
                {
                    for (int side = 0; side < 2; side++) // For each half-petal
                    {
                        Vector2 sideBaseDirection = baseDirection.RotatedBy((side == 0 ? -1f : 1f) * (petalSeparation / 2f));

                        for (float sparkle = 0f; sparkle < sparklesPerHalfPetal; sparkle++)
                        {
                            Vector2 direction = Vector2.Lerp(baseDirection, sideBaseDirection, sparkle / sparklesPerHalfPetal);
                            float velocity = maxVelocity - ((maxVelocity - minVelocity) * (sparkle / sparklesPerHalfPetal));

                            Projectile.NewProjectile(
                                projectile.Center,
                                direction * velocity,
                                ModContent.ProjectileType<CelebrationSparkle>(),
                                projectile.damage,
                                0f,
                                Main.myPlayer,
                                RocketType);
                        }
                    }

                    // Front sparkle
                    Projectile.NewProjectile(
                        projectile.Center,
                        baseDirection * maxVelocity,
                        ModContent.ProjectileType<CelebrationSparkle>(),
                        projectile.damage,
                        0f,
                        Main.myPlayer,
                        RocketType);

                    // Rotate to next petal
                    baseDirection = baseDirection.RotatedBy(petalSeparation);
                }
            }
        }

        private void YellowExplosion()
        {
            float baseVelocity = 2f;
            float velocityMultiplier = 2f;

            for (int eye = 0; eye < 2; eye++)
            {
                int sparkles = eye == 0 ? 20 : 40;
                float separation = MathHelper.TwoPi / sparkles;

                if (eye == 1)
                    baseVelocity *= velocityMultiplier;

                for (int ring = 0; ring < 2; ring++)
                {
                    for (int sparkle = 0; sparkle < sparkles; sparkle++)
                    {
                        Vector2 velocity = (sparkle * separation).ToRotationVector2() * baseVelocity;
                        if (ring == 1)
                            velocity.X *= velocityMultiplier;

                        Projectile.NewProjectile(
                            projectile.Center,
                            velocity,
                            ModContent.ProjectileType<CelebrationSparkle>(),
                            projectile.damage,
                            0f,
                            Main.myPlayer,
                            RocketType);
                    }
                }
            }
        }

        private void Fireworks()
        {
            // Vanilla firework explosions "borrowed" from source code
            // Actually took my time and now i get how they work
            switch (RocketType)
            {
                case 0:
                    // Red
                    for (int i = 0; i < 400; i++)
                    {
                        float velocityLength = 16f - ((3f - (float)Math.Floor(i / 100f)) * 4f);

                        int dustID = Dust.NewDust(projectile.Center, 6, 6, DustID.Firework_Red, 0f, 0f, 100);
                        Vector2 newVelocity = Vector2.Normalize(Main.dust[dustID].velocity);

                        if (newVelocity == Vector2.Zero)
                            newVelocity = Vector2.UnitX;

                        newVelocity *= velocityLength;

                        Main.dust[dustID].velocity *= 0.5f;
                        Main.dust[dustID].velocity = newVelocity;
                        Main.dust[dustID].scale = 1.3f;
                        Main.dust[dustID].noGravity = true;
                    }
                    break;
                case 1:
                    // Green
                    for (int i = 0; i < 400; i++)
                    {
                        float velocityLength;
                        if (i > 250)
                            velocityLength = 13f;
                        else if (i > 100)
                            velocityLength = 10f;
                        else
                            velocityLength = i / 50f;

                        int dustID = Dust.NewDust(projectile.Center, 6, 6, DustID.Firework_Green, 0f, 0f, 100);
                        Vector2 newVelocity = Main.dust[dustID].velocity;

                        if (newVelocity == Vector2.Zero)
                        {
                            newVelocity.X = 1f;
                        }

                        float thing = velocityLength / newVelocity.Length();

                        if (i <= 200)
                        {
                            newVelocity *= thing;
                        }
                        else
                        {
                            newVelocity.X *= thing * 1.25f;
                            newVelocity.Y *= thing * 0.75f;
                        }

                        Main.dust[dustID].velocity *= 0.5f;
                        Main.dust[dustID].velocity += newVelocity;

                        if (i > 100)
                        {
                            Main.dust[dustID].scale = 1.3f;
                            Main.dust[dustID].noGravity = true;
                        }
                    }
                    break;
                case 2:
                    // Blue
                    Vector2 baseRotation = Main.rand.NextFloat(MathHelper.TwoPi).ToRotationVector2();
                    float petals = Main.rand.Next(5, 9);
                    float maxSpeed = Main.rand.Next(12, 17);
                    float minSpeed = Main.rand.Next(3, 7);
                    float dustPerPetal = 20f;

                    // Outer flower
                    for (int petal = 0; petal < petals; petal++)
                    {
                        for (int side = 0; side < 2; side++)
                        {
                            Vector2 sideBaseRotation = baseRotation.RotatedBy((side == 0 ? 1f : -1f) * MathHelper.Pi / petals);

                            for (int dust = 0; dust < dustPerPetal; dust++)
                            {
                                Vector2 direction = Vector2.Lerp(baseRotation, sideBaseRotation, dust / dustPerPetal);
                                float velocity = MathHelper.Lerp(maxSpeed, minSpeed, dust / dustPerPetal);

                                int dustID = Dust.NewDust(projectile.Center, 6, 6, DustID.Firework_Yellow, 0f, 0f, 100, default, 1.3f);
                                Main.dust[dustID].velocity *= 0.1f;
                                Main.dust[dustID].velocity += direction * velocity;
                                Main.dust[dustID].noGravity = true;
                            }
                        }

                        baseRotation = baseRotation.RotatedBy(MathHelper.TwoPi / petals);
                    }

                    // Inner flower
                    for (int petal = 0; petal < petals; petal++)
                    {
                        for (int side = 0; side < 2; side++)
                        {
                            Vector2 sideBaseRotation = baseRotation.RotatedBy((side == 0 ? 1f : -1f) * MathHelper.Pi / petals);

                            for (int dust = 0; dust < dustPerPetal; dust++)
                            {
                                Vector2 direction = Vector2.Lerp(baseRotation, sideBaseRotation, dust / dustPerPetal);
                                float velocity = MathHelper.Lerp(maxSpeed, minSpeed, dust / dustPerPetal) / 2f;

                                int dustID = Dust.NewDust(projectile.Center, 6, 6, DustID.Firework_Yellow, 0f, 0f, 100, default, 1.3f);
                                Main.dust[dustID].velocity *= 0.1f;
                                Main.dust[dustID].noGravity = true;
                                Main.dust[dustID].velocity += direction * velocity;
                            }
                        }

                        baseRotation = baseRotation.RotatedBy(MathHelper.TwoPi / petals);
                    }

                    // Outer circle
                    for (int i = 0; i < 100; i++)
                    {
                        int dustID = Dust.NewDust(projectile.Center, 6, 6, DustID.Firework_Blue, 0f, 0f, 100, default, 1.3f);

                        Vector2 velocity = Vector2.Normalize(Main.dust[dustID].velocity);
                        if (velocity == Vector2.Zero)
                            velocity = Vector2.UnitX;

                        velocity *= maxSpeed;

                        Main.dust[dustID].velocity *= 0.5f;
                        Main.dust[dustID].velocity += velocity;
                        Main.dust[dustID].noGravity = true;
                    }
                    break;
                case 3:
                    // Yellow
                    for (int i = 0; i < 400; i++)
                    {
                        int dustType = DustID.Firework_Yellow;
                        float velocityLength = 16f;

                        if ((i > 100 && i <= 200) || i > 300)
                        {
                            dustType = DustID.Firework_Pink;
                        }

                        if (i > 100)
                        {
                            velocityLength -= 2f + (3f * (float)Math.Floor(i / 100f));
                        }

                        int dustID = Dust.NewDust(projectile.Center, 6, 6, dustType, 0f, 0f, 100);
                        Vector2 velocity = Main.dust[dustID].velocity;

                        if (velocity == Vector2.Zero)
                            velocity = Vector2.UnitX;

                        float thing = velocityLength / velocity.Length();

                        if (i > 300)
                        {
                            velocity.X = velocity.X * thing * 0.7f;
                            velocity.Y *= thing;
                        }
                        else if (i > 200)
                        {
                            velocity.X *= thing;
                            velocity.Y = velocity.Y * thing * 0.7f;
                        }
                        else if (i > 100)
                        {
                            velocity.X = velocity.X * thing * 0.7f;
                            velocity.Y *= thing;
                        }
                        else
                        {
                            velocity.X *= thing;
                            velocity.Y = velocity.Y * thing * 0.7f;
                        }

                        Main.dust[dustID].velocity *= 0.5f;
                        Main.dust[dustID].velocity += velocity;

                        if (!Main.rand.NextBool(3))
                        {
                            Main.dust[dustID].scale = 1.3f;
                            Main.dust[dustID].noGravity = true;
                        }
                    }
                    break;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            int textureID = ProjectileID.RocketFireworkRed + RocketType;
            Texture2D texture = ModContent.GetTexture("Terraria/Projectile_" + textureID);

            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 4f);

            spriteBatch.Draw(
                texture,
                projectile.Center - Main.screenPosition,
                null,
                lightColor,
                projectile.rotation,
                origin,
                projectile.scale * 1.5f,
                SpriteEffects.None,
                0f);

            return false;
        }

        private void VanillaDustThingy()
        {
            // Vanilla source code my beloved
            if (Timer == 0)
            {
                // Initial smoke blast
                for (int i = 0; i < 8; i++)
                {
                    int dustID = Dust.NewDust(projectile.Center, projectile.width, projectile.height, DustID.Fire, 0f, 0f, 100, default, 1.8f);
                    Main.dust[dustID].noGravity = true;
                    Main.dust[dustID].velocity *= 3f;
                    Main.dust[dustID].fadeIn = 0.5f;
                    Main.dust[dustID].position += projectile.velocity / 2f;
                    Main.dust[dustID].velocity += projectile.velocity / 4f + projectile.velocity * 0.1f;
                }
            }
            else
            {
                // Smoke trail
                for (int i = 0; i < 3; i++)
                {
                    float offset = 20f - (i * 5f);
                    int dustID = Dust.NewDust(new Vector2(projectile.position.X + 2f, projectile.position.Y + offset), 8, 8, DustID.Fire, projectile.velocity.X, projectile.velocity.Y, 100, default, 1.2f);
                    Main.dust[dustID].noGravity = true;
                    Main.dust[dustID].velocity *= 0.2f;
                    Main.dust[dustID].position = Main.dust[dustID].position.RotatedBy(projectile.rotation, projectile.Center);
                }
            }
        }
    }
}
