using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MisterCarlosMod.Projectiles;
using System.IO;

namespace MisterCarlosMod.NPCs.MisterCarlos.Attacks
{
    public class LunarPortals : NPCAttack<MisterCarlos>
    {
        public override float Duration => 330f;

        private int TotalAttacks => Main.expertMode ? 8 : 6;
        private int damage = 90;

        private Vector2 position;
        private int attacksCounter;

        public LunarPortals(MisterCarlos carlos) : base(carlos)
        {

        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            damage = (int)(damage * 0.6f);
        }

        public override void Initialize()
        {
            attacksCounter = TotalAttacks;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                float distance = 400f + Main.rand.NextFloat(100f);
                Vector2 direction = modNPC.Target.DirectionTo(modNPC.npc.Center);
                float spread = MathHelper.PiOver2;

                position = direction.RotatedBy(Main.rand.NextFloat(spread) - spread / 2) * distance;

                modNPC.npc.netUpdate = true;
            }

            if (Main.netMode != NetmodeID.Server)
            {
                modNPC.weapon = new HoldWeapon("Terraria/Item_" + ItemID.MoonlordTurretStaff, Vector2.Zero);

                Rectangle bounds = modNPC.weapon.Texture.Bounds;
                modNPC.weapon.origin = bounds.BottomLeft() + Vector2.Normalize(bounds.TopRight() - bounds.BottomLeft()) * 4f;
            }
        }

        public override void AI()
        {
            NPC npc = modNPC.npc;

            int moveDuration = 60;
            int totalAttackDuration = 60;

            // Move next to player
            float moveSpeed = 16f + (modNPC.Target.velocity.Length() * 0.5f);
            float inertia = 25f;
            Vector2 velocity = npc.DirectionTo(modNPC.Target.Center + position) * moveSpeed;

            npc.velocity = (npc.velocity * (inertia - 1) + velocity) / inertia;

            if (modNPC.CycleTimer > moveDuration)
            {
                float timePassed = modNPC.CycleTimer - moveDuration;
                int attackSpeed = (int)Math.Floor((double)totalAttackDuration / TotalAttacks);

                if (attacksCounter > 0)
                {
                    if (timePassed % attackSpeed == 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            LaunchPortal(totalAttackDuration, timePassed);
                        }
                        attacksCounter--;
                    }
                } else if(timePassed > totalAttackDuration + 30)
                {
                    modNPC.weapon = null;
                }
            }
        }

        private void LaunchPortal(int totalAttackDuration, float timePassed)
        {
            Vector2 toPlayer = modNPC.npc.DirectionTo(modNPC.Target.Center);
            float speed = 16f + Main.rand.NextFloat(20f);

            float maxRotation = 45f;
            float minRotation = 15f;

            float rotation = minRotation + Main.rand.NextFloat(maxRotation - minRotation);
            speed *= Math.Max(1f - rotation / maxRotation, 0.5f);

            if (attacksCounter % 2 == 0)
                rotation *= -1f;

            float duration = totalAttackDuration - timePassed;

            Projectile.NewProjectile(
                modNPC.npc.Center,
                toPlayer.RotatedBy(MathHelper.ToRadians(rotation)) * speed + (modNPC.Target.velocity * 1.2f),
                ModContent.ProjectileType<BigLunarPortal>(),
                damage / 2,
                0f,
                Main.myPlayer,
                duration,
                Main.rand.NextFloat() * MathHelper.TwoPi / BigLunarPortal.LaserCount);
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WritePackedVector2(position);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            position = reader.ReadPackedVector2();
        }
    }
}
