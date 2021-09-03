using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MisterCarlosMod.Projectiles;

namespace MisterCarlosMod.NPCs.MisterCarlos.Attacks
{
    public class LunarPortals : NPCAttack<MisterCarlos>
    {
        public override float Duration => 330f;

        private int TotalAttacks => Main.expertMode ? 8 : 6;
        private int damage = 90;

        private Vector2 position;
        private int timer;
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
            timer = 0;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                float distance = 400f + Main.rand.NextFloat(100f);
                Vector2 direction = modNPC.Target.DirectionTo(modNPC.npc.Center);
                float spread = MathHelper.PiOver2;

                position = direction.RotatedBy(Main.rand.NextFloat(spread) - spread / 2) * distance;

                modNPC.npc.netUpdate = true;
            }
        }

        public override void AI()
        {
            NPC npc = modNPC.npc;

            int moveDuration = 60;
            int totalAttackDuration = 60;

            // Moverse cerca del jugador
            float moveSpeed = 16f + (modNPC.Target.velocity.Length() * 0.2f);
            float inertia = 25f;
            Vector2 velocity = npc.DirectionTo(modNPC.Target.Center + position) * moveSpeed;

            npc.velocity = (npc.velocity * (inertia - 1) + velocity) / inertia;

            if (timer > moveDuration)
            {
                int timePassed = timer - moveDuration;
                int attackSpeed = (int)Math.Floor((double)totalAttackDuration / TotalAttacks);

                if (attacksCounter > 0)
                {
                    modNPC.weapon = new MisterCarlos.HoldWeapon(ItemID.MoonlordTurretStaff, Vector2.Zero);
                    Texture2D staffTexture = modNPC.weapon.Texture;

                    modNPC.weapon.origin = staffTexture.Bounds.BottomLeft() + Vector2.Normalize(staffTexture.Bounds.TopRight() - staffTexture.Bounds.BottomLeft()) * 4f;

                    if (Main.netMode != NetmodeID.MultiplayerClient && timePassed % attackSpeed == 0)
                    {
                        LaunchPortal(totalAttackDuration, timePassed);
                        npc.netUpdate = true;
                    }
                } else if(timePassed > totalAttackDuration + 30)
                {
                    modNPC.weapon = null;
                }
            }

            timer++;
        }

        private void LaunchPortal(int totalAttackDuration, int timePassed)
        {
            // Lanzar portal lunar
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

            attacksCounter--;
        }
    }
}
