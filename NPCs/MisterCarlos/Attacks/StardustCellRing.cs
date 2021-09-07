using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MisterCarlosMod.Projectiles;
using MisterCarlosMod.Buffs;

namespace MisterCarlosMod.NPCs.MisterCarlos.Attacks
{
    // No way another Touhou reference??!!!?!
    public class StardustCellRing : NPCAttack<MisterCarlos>
    {
        public override float Duration => 840f;

        private int TotalCells => Main.expertMode ? 6 : 4;
        private const float arenaRadius = 650f;

        private Vector2 arenaCenter;
        private int damage = 80;

        public StardustCellRing(MisterCarlos carlos) : base(carlos)
        {

        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            damage = (int)(damage * 0.6f);
        }

        public override void Initialize()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                modNPC.weapon = new HoldWeapon("Terraria/Item_" + ItemID.StardustCellStaff, Vector2.Zero, MathHelper.PiOver2);

                Rectangle bounds = modNPC.weapon.Texture.Bounds;
                Vector2 holdDirection = Vector2.Normalize(bounds.TopRight() - bounds.BottomLeft());

                modNPC.weapon.origin = bounds.BottomLeft() + holdDirection * 8;
            }
        }

        public override void AI()
        {
            NPC npc = modNPC.npc;

            int previousDirection = npc.direction;
            npc.direction = npc.spriteDirection = 1;

            Vector2 targetPosition = arenaCenter + (-Vector2.UnitY * (arenaRadius - 150f));
            float moveSpeed = 30f;
            float inertia = 10f;

            float initialDelay = 60f;
            if (modNPC.CycleTimer >= initialDelay)
            {
                float timePassed = modNPC.CycleTimer - initialDelay;
                float shootDelay = 120f;

                CheckStardustInfection();

                if (timePassed == 0f)
                {
                    // Create stardust cell ring
                    int cells = 40;
                    float separation = MathHelper.TwoPi / cells;

                    for (int i = 0; i < cells; i++)
                    {
                        Vector2 position = arenaCenter + Vector2.UnitX.RotatedBy(i * separation) * arenaRadius;

                        Projectile.NewProjectile(
                            position,
                            Vector2.Zero,
                            ModContent.ProjectileType<StardustCellBorder>(),
                            damage / 2,
                            0f,
                            Main.myPlayer,
                            arenaCenter.X, arenaCenter.Y);
                    }
                }
                else if (Main.netMode != NetmodeID.MultiplayerClient && timePassed >= shootDelay)
                {
                    // Shoot stardust cells
                    timePassed -= shootDelay;
                    int shootSpeed = 15;
                    int shootDuration = shootSpeed * (TotalCells + 1);

                    if (timePassed < shootDuration)
                    {

                        float shootTime = timePassed % shootSpeed;

                        if (Main.netMode != NetmodeID.Server)
                        {
                            modNPC.weapon.rotation = shootTime / shootSpeed * -MathHelper.Pi;
                        }

                        if (shootTime == 0)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                // Shoot cell
                                float cellIndex = timePassed / shootSpeed;
                                float spread = MathHelper.Pi * 0.8f;
                                float separation = spread / TotalCells;

                                float rotation = (-spread / 2f) + cellIndex * separation;
                                Vector2 direction = Vector2.UnitY.RotatedBy(rotation);
                                float stopAfter = Main.rand.NextFloat(5, 30f);

                                Projectile.NewProjectile(
                                    npc.Center,
                                    direction * 10f,
                                    ModContent.ProjectileType<NotAStardustCell>(),
                                    damage / 2,
                                    0f,
                                    Main.myPlayer,
                                    modNPC.Target.whoAmI,
                                    stopAfter);
                            }

                            Main.PlaySound(SoundID.Item44, npc.Center);
                        }
                    }
                    else
                    {
                        // Hover over player
                        npc.direction = npc.spriteDirection = previousDirection;

                        modNPC.weapon = null;
                        targetPosition.X = modNPC.Target.position.X;
                        targetPosition.Y += 50f;

                        inertia = 40;
                        moveSpeed = 5f;

                        if (modNPC.CycleTimer > Duration - 60f)
                        {
                            ClearStardustInfection();
                        }
                    }
                }
            }
            else
            {
                arenaCenter = modNPC.Target.Center;
            }

            Vector2 velocity = npc.DirectionTo(targetPosition) * moveSpeed + (modNPC.Target.velocity * 0.5f);
            npc.velocity = (npc.velocity * (inertia - 1) + velocity) / inertia;
        }

        private void CheckStardustInfection()
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (!player.active || player.dead) continue;

                float distance = (player.Center - arenaCenter).Length();

                if (distance > arenaRadius)
                {
                    player.AddBuff(ModContent.BuffType<StardustInfection>(), 200, true);
                } else
                {
                    MisterCarlosPlayer modPlayer = player.GetModPlayer<MisterCarlosPlayer>();
                    if (modPlayer.stardustInfection)
                    {
                        int index = player.FindBuffIndex(ModContent.BuffType<StardustInfection>());
                        if (index != -1)
                            player.DelBuff(index);
                    }
                }
            }
        }

        private void ClearStardustInfection()
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (!player.active || player.dead) continue;

                MisterCarlosPlayer modPlayer = player.GetModPlayer<MisterCarlosPlayer>();
                if (modPlayer.stardustInfection)
                {
                    int index = player.FindBuffIndex(ModContent.BuffType<StardustInfection>());
                    if (index != -1)
                        player.DelBuff(index);
                }
            }
        }
    }
}
