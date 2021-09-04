using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MisterCarlosMod.Projectiles;

namespace MisterCarlosMod.NPCs.MisterCarlos.Attacks
{
    // Touhou reference??!?! :flushed:
    public class CirnoWithAStarlight : NPCAttack<MisterCarlos>
    {
        public override float Duration => 720f;

        private const string starlightTexture = "MisterCarlosMod/NPCs/MisterCarlos/Attacks/Starlight";
        private const int totalShootDuration = 240;
        private const int freezeDelay = 30;

        private int damage = 75;

        private Vector2 position;
        private Vector2 starlightOrigin;
        private bool updatePosition;

        public CirnoWithAStarlight(MisterCarlos carlos) : base(carlos)
        {
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            damage = (int)(damage * 0.6f);
        }

        public override void Initialize()
        {
            updatePosition = true;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC npc = modNPC.npc;
                float distance = 600f + Main.rand.NextFloat(100f);
                float spread = MathHelper.PiOver4 / 2f;

                Vector2 direction = Vector2.UnitX;
                if (Math.Sign(npc.DirectionFrom(modNPC.Target.Center).X) < 0)
                {
                    direction *= -1f;
                }

                position = direction.RotatedBy(Main.rand.NextFloat(-spread, spread)) * distance;

                npc.netUpdate = true;
            }

            if (Main.netMode != NetmodeID.Server)
            {
                modNPC.weapon = new HoldWeapon(starlightTexture, Vector2.Zero);

                Rectangle bounds = modNPC.weapon.Texture.Bounds;
                starlightOrigin = modNPC.weapon.origin = bounds.BottomLeft() + Vector2.Normalize(bounds.TopRight() - bounds.BottomLeft()) * 5f;

            }
        }

        public override void AI()
        {
            NPC npc = modNPC.npc;

            // Move next to player
            float moveSpeed = 16f + (modNPC.Target.velocity.Length() * 0.6f);
            float inertia = 20f;

            int initialDelay = 60;

            if (modNPC.CycleTimer >= initialDelay)
            {
                int timePassed = (int)modNPC.CycleTimer - initialDelay;
                int intervalDuration = 120;

                if (timePassed < totalShootDuration + intervalDuration)
                {
                    // First attack (random projectile spread)
                    if (timePassed < totalShootDuration)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int amount = Main.expertMode ? Main.rand.Next(1, 2) : 1;
                            for (int i = 0; i < amount; i++)
                                Shoot(timePassed);
                        }

                        // Sostener starlight
                        if (Main.netMode != NetmodeID.Server && modNPC.weapon != null)
                        {
                            modNPC.weapon.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                        }
                    }
                    else if (Main.netMode != NetmodeID.Server)
                    {
                        modNPC.weapon = null;
                    }
                }
                else
                {
                    // Second attack (pog spiral)
                    float secondAttackDuration = 120f;
                    timePassed -= totalShootDuration + intervalDuration;

                    if (timePassed < secondAttackDuration)
                    {
                        position = position.RotatedBy(MathHelper.Pi / secondAttackDuration);
                        inertia = 5;
                        moveSpeed = 40f;

                        // Rotate held starlight
                        if (Main.netMode != NetmodeID.Server)
                        {
                            if (modNPC.weapon == null)
                            {
                                modNPC.weapon = new HoldWeapon(starlightTexture, starlightOrigin);
                            }

                            float rotationSpeed = MathHelper.Pi / 10f;
                            modNPC.weapon.rotation = (modNPC.weapon.rotation + rotationSpeed) % MathHelper.TwoPi;
                        }

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int shootSpeed = 3;
                            if (timePassed % shootSpeed == 0)
                            {
                                int waves = Main.expertMode ? 16 : 10;
                                float separation = MathHelper.TwoPi / waves;

                                float hue = 360f - (timePassed * 2f % 360);

                                Vector2 startDirection = Vector2.Normalize(modNPC.Target.Center - npc.Center);

                                for (int i = 0; i < waves; i++)
                                {
                                    Vector2 direction = startDirection.RotatedBy(i * separation);
                                    Projectile.NewProjectile(npc.Center, direction * 4f, ModContent.ProjectileType<SpiralStarlightLight>(), damage / 2, 0f, Main.myPlayer, hue, 1f);
                                }
                            }
                        }
                    }
                    else if (Main.netMode != NetmodeID.Server)
                    {
                        modNPC.weapon = null;
                    }
                }
            }

            Vector2 velocity = npc.DirectionTo(modNPC.Target.Center + position) * moveSpeed + (modNPC.Target.velocity * 0.5f);
            npc.velocity = (npc.velocity * (inertia - 1) + velocity) / inertia;
        }

        private int Shoot(int timePassed)
        {
            Vector2 direction = Main.rand.NextVector2Unit();

            float speed = Main.rand.NextFloat(7f, 9f);
            Vector2 velocity = direction * speed + modNPC.npc.velocity;

            int freezeAfter = totalShootDuration + freezeDelay - timePassed + 30;
            return Projectile.NewProjectile(modNPC.npc.Center, velocity, ModContent.ProjectileType<StarlightLight>(), damage / 2, 0f, Main.myPlayer, freezeAfter);
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            if (updatePosition)
            {
                writer.WritePackedVector2(position);
                updatePosition = false;
            }
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            if (updatePosition)
            {
                position = reader.ReadPackedVector2();
                updatePosition = false;
            }
        }
    }
}
