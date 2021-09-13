using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MisterCarlosMod.Projectiles.Celebration;

namespace MisterCarlosMod.NPCs.MisterCarlos.Attacks
{
    public class CelebrationRain : NPCAttack<MisterCarlos>
    {
        private const float initialDelay = 60f;
        private const float launchRocketsDuration = 120f;
        private const float idleDuration = 300f;

        public override float Duration => initialDelay + launchRocketsDuration + idleDuration;

        private int damage = 85;
        private Vector2 position;

        public CelebrationRain(MisterCarlos carlos) : base(carlos)
        {

        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            damage = (int)(damage * 0.6f);
        }

        public override void Initialize()
        {
            position = Vector2.UnitX * Math.Abs(modNPC.Target.DirectionTo(modNPC.npc.Center).X) * 600f;
        }

        public override void AI()
        {
            NPC npc = modNPC.npc;

            float initialDelay = 60f;

            float moveSpeed = 20f;
            float inertia = 10f;

            float timer = modNPC.CycleTimer;

            if (timer >= initialDelay)
            {

                float launchRocketsDuration = 120f;

                if ((timer -= initialDelay) < launchRocketsDuration)
                {
                    LaunchRockets(timer);
                } else if ((timer -= launchRocketsDuration) < idleDuration)
                {
                    Idle(timer, ref moveSpeed, ref inertia);
                }
            }

            Vector2 velocity = npc.DirectionTo(modNPC.Target.Center + position) * moveSpeed + (modNPC.Target.velocity * 0.5f);
            npc.velocity = (npc.velocity * (inertia - 1) + velocity) / inertia;
        }

        private void LaunchRockets(float timer)
        {
            if (timer % 30f == 0f)
            {
                float baseRotation = MathHelper.PiOver4;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC npc = modNPC.npc;

                    float spread = MathHelper.PiOver4 / 6f;

                    float rotation = baseRotation + Main.rand.NextFloat(-spread, spread);
                    Vector2 direction = (Vector2.UnitX * npc.direction).RotatedBy(rotation);

                    int rocketType = Main.rand.Next(4);

                    Projectile.NewProjectile(
                        npc.Center,
                        direction * 5f,
                        ModContent.ProjectileType<CelebrationRocket>(),
                        damage,
                        0f,
                        Main.myPlayer,
                        rocketType
                        );
                }
            }
        }

        private void Idle(float timer, ref float moveSpeed, ref float inertia)
        {
            moveSpeed = 8f;
            inertia = 10f;
            // Switch idle position every 60 frames
            if (Main.netMode != NetmodeID.MultiplayerClient && timer % 60f == 0f)
            {
                float range = 400f;
                position = new Vector2(Main.rand.NextFloat(-range, range), 400f);
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            if (modNPC.CycleTimer >= initialDelay + launchRocketsDuration)
            {
                writer.WritePackedVector2(position);
            }
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            if (modNPC.CycleTimer >= initialDelay + launchRocketsDuration)
            {
                position = reader.ReadPackedVector2();
            }
        }
    }
}
