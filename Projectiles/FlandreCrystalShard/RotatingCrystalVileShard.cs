using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MisterCarlosMod.Projectiles.FlandreCrystalShard
{
    public class RotatingCrystalVileShard : ModProjectile
    {
        public override string Texture => "Terraria/Item_" + ItemID.CrystalVileShard;

        public float RotationSpeed
        {
            get => projectile.ai[0];
            set => projectile.ai[0] = value;
        }

        public float AttackTimer
        {
            get => projectile.ai[1];
            set => projectile.ai[1] = value;
        }

        public override void SetDefaults()
        {
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.width = projectile.height = 32;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        public override void AI()
        {
            projectile.rotation += RotationSpeed;

            if (Main.netMode != NetmodeID.MultiplayerClient && AttackTimer++ > 5f)
            {
                int crystals = 4;
                float separation = MathHelper.TwoPi / crystals;

                for (int i = 0; i < crystals; i++)
                {
                    int projID = Projectile.NewProjectile(
                        projectile.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<CrystalShard>(),
                        projectile.damage,
                        0f,
                        Main.myPlayer,
                        15);

                    Main.projectile[projID].rotation = projectile.rotation - MathHelper.PiOver4 + i * separation;
                }

                Main.PlaySound(SoundID.Item101, projectile.Center);

                AttackTimer = 0f;
            }
        }
    }
}
