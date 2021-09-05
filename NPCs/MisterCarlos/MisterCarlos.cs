using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MisterCarlosMod.NPCs.MisterCarlos.Attacks;
using Microsoft.Xna.Framework.Graphics;

namespace MisterCarlosMod.NPCs.MisterCarlos
{
    [AutoloadBossHead]
    public class MisterCarlos : ModNPC
    {
        private readonly Dictionary<int, List<NPCAttack<MisterCarlos>>> attacks = new Dictionary<int, List<NPCAttack<MisterCarlos>>>();

        private readonly Texture2D wingsTexture = ModContent.GetTexture("Terraria/Wings_8");
        private readonly Texture2D armTexture = ModContent.GetTexture("MisterCarlosMod/NPCs/MisterCarlos/MisterCarlos_Arm");
        private readonly Texture2D leafTexture = ModContent.GetTexture("Terraria/Projectile_" + ProjectileID.Leaf);

        public HoldWeapon weapon = null;
        private const int wingFrames = 4;
        private int wingsFrameCount = 0;

        // Local AI
        private int wingsFrame = 0;
        private bool initAttack = true;
        private bool transitioning = false;
        private bool despawn = false;

        private const int leafCount = 8;
        private const float leafSpeed = 2f;
        private float leafTimer = 0;

        public MisterCarlos()
        {
            attacks[0] = new List<NPCAttack<MisterCarlos>> { new StardustCellRing(this) };
            attacks[1] = new List<NPCAttack<MisterCarlos>> { new TestAttack(this) };
            attacks[2] = new List<NPCAttack<MisterCarlos>> { new TestAttack(this) };
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("MisterCarlos");
        }

        public override void SetDefaults()
        {
            npc.aiStyle = -1;
            npc.boss = true;
            npc.defense = 40;
            npc.width = 30;
            npc.height = 46;
            npc.lifeMax = 30000;
            npc.value = Item.buyPrice(1, 0, 0, 0);
            npc.lavaImmune = true;
            npc.damage = 70;
            npc.knockBackResist = 0f;
            npc.npcSlots = 20f;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath1;

            music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/Beach_Parade");

            drawOffsetY = -4;
        }

        // AI slot aliases
        public int Phase
        {
            get => (int)npc.ai[0];
            set => npc.ai[0] = value;
        }

        public float CycleTimer
        {
            get => npc.ai[1];
            set => npc.ai[1] = value;
        }

        public int AttackIndex
        {
            get => (int)npc.ai[2];
            set => npc.ai[2] = value;
        }

        // Convenience stuff
        public Player Target
        {
            get => Main.player[npc.target];
        }

        public NPCAttack<MisterCarlos> CurrentAttack
        {
            get => attacks[Phase][AttackIndex];
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            npc.lifeMax = (int)(npc.lifeMax * 0.625f * bossLifeScale);
            npc.damage = (int)(npc.damage * 0.6f);

            for (int p = 0; p < attacks.Count; p++)
            {
                attacks[p].ForEach(attack => attack.ScaleExpertStats(numPlayers, bossLifeScale));
            }
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.SuperHealingPotion;
        }

        public override void AI()
        {
            // Wings animation
            if (++wingsFrameCount > 4)
            {
                wingsFrame = (wingsFrame + 1) % wingFrames;
                wingsFrameCount = 0;
            }

            // Leaf ring animation
            leafTimer = (leafTimer + 1) % (360f / leafCount / leafSpeed);

            // Rotate NPC according to X velocity
            npc.rotation = MathHelper.ToRadians(MathHelper.Min(npc.velocity.X * 2f, 40f));

            // Look towards target
            int direction = Math.Sign((Target.Center - npc.Center).X);
            if (direction != 0)
            {
                npc.direction = direction;
                npc.spriteDirection = direction;
            }

            if (transitioning)
            {
                // Transition AI
                npc.dontTakeDamage = true;
                npc.velocity *= 0.93f;

                // Slow down and run transition cycle
                float velocityLength = npc.velocity.Length();
                if (velocityLength == 0f)
                {
                    CycleTimer++;

                    if (CycleTimer == 90f)
                    {
                        for (int d = 0; d < 30; d++)
                        {
                            int dust = Dust.NewDust(npc.position, npc.width, npc.height, DustID.GreenTorch);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].velocity *= 6f;
                            Main.dust[dust].fadeIn = 1.3f;
                        }
                    }

                    float transitionDuration = 180f;

                    if (CycleTimer >= transitionDuration)
                    {
                        CycleTimer -= transitionDuration;
                        initAttack = true;
                        AttackIndex = 0;
                        transitioning = false;
                        npc.dontTakeDamage = false;

                        npc.netUpdate = true;
                        return;
                    }
                }
                else if (velocityLength < 0.3f)
                {
                    npc.velocity = Vector2.Zero;
                }
            }
            else
            {
                // Check if transition should start
                float lifeRadius = (float)npc.life / npc.lifeMax;

                bool doFirstTransition = Phase == 0 && lifeRadius <= (Main.expertMode ? 0.7f : 0.5f);
                bool doSecondTransition = Main.expertMode && Phase == 1 && lifeRadius <= 0.25f;

                if (Main.netMode != NetmodeID.MultiplayerClient && (doFirstTransition || doSecondTransition))
                {
                    Phase = doFirstTransition ? 1 : 2;
                    StartTransition();

                    npc.netUpdate = true;
                    return;
                }

                // Actual fight AI
                if (initAttack)
                {
                    weapon = null;

                    npc.TargetClosest(false);
                    if (!HasValidTarget())
                    {
                        despawn = true;
                    }
                    else
                    {
                        CurrentAttack.Initialize();
                    }

                    initAttack = false;
                }

                if (despawn)
                    DespawnAI();
                else
                {
                    CurrentAttack.AI();

                    CycleTimer++;
                    if (Main.netMode != NetmodeID.MultiplayerClient && CycleTimer > CurrentAttack.Duration)
                    {
                        CycleTimer -= CurrentAttack.Duration;
                        AttackIndex = (AttackIndex + 1) % attacks[Phase].Count;
                        initAttack = true;

                        npc.netUpdate = true;
                    }
                }
            }
        }

        private void StartTransition()
        {
            transitioning = true;
            CycleTimer = 0f;
        }

        private void DespawnAI()
        {
            // Stop despawning if a valid target is found
            npc.TargetClosest(false);
            if (Main.netMode != NetmodeID.MultiplayerClient && HasValidTarget())
            {
                CycleTimer = 0f;
                initAttack = true;
                despawn = false;

                npc.netUpdate = true;
                return;
            }

            // Slow down and despawn
            npc.velocity *= 0.95f;
            if (npc.velocity.Length() < 0.3f)
            {
                npc.velocity = Vector2.Zero;

                CycleTimer++;
                if (CycleTimer >= 120f)
                {
                    npc.life = -1;
                }
            }
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (npc.life > 0) return;

            // Death effect
            for (int d = 0; d < 20; d++)
            {
                int dust = Dust.NewDust(npc.Center, npc.width, npc.height, DustID.Dirt);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 1.5f;
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(initAttack);
            writer.Write(transitioning);
            //writer.Write(despawn);

            CurrentAttack.SendExtraAI(writer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            initAttack = reader.ReadBoolean();
            transitioning = reader.ReadBoolean();
            //despawn = reader.ReadBoolean();

            CurrentAttack.ReceiveExtraAI(reader);
        }

        public override void BossHeadSpriteEffects(ref SpriteEffects effects)
        {
            if (npc.direction == 1)
                effects = SpriteEffects.FlipHorizontally;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            // Draw wings
            int frameHeight = wingsTexture.Height / wingFrames;
            Rectangle sourceRectangle = new Rectangle(0, wingsFrame * frameHeight, wingsTexture.Width, frameHeight);

            Vector2 origin = sourceRectangle.Size() / 2f;
            origin.X += 7;
            origin.Y -= 3;

            Vector2 position = npc.Center;

            SpriteEffects flip = SpriteEffects.None;
            if (npc.direction == -1)
            {
                flip = SpriteEffects.FlipHorizontally;
                origin.X = sourceRectangle.Width - origin.X;
            }

            spriteBatch.Draw(wingsTexture, position - Main.screenPosition, sourceRectangle, drawColor, npc.rotation, origin, 1f, flip, 0f);

            // Draw leaf rings
            int phasesLeft = (Main.expertMode ? 2 : 1) - Phase;
            if (phasesLeft > 0)
            {
                int frameCount = Main.projFrames[ProjectileID.Leaf];
                frameHeight = leafTexture.Height / frameCount;

                float cycleDegrees = 360f / leafCount;
                int frame = (int)Math.Floor((leafTimer / cycleDegrees) * frameCount);

                sourceRectangle = new Rectangle(0, frame * frameHeight, leafTexture.Width, frameHeight);
                origin = new Vector2(leafTexture.Width / 2f, frameHeight / 2f);

                for (int ring = 0; ring < phasesLeft; ring++)
                {
                    for (int leaf = 0; leaf < leafCount; leaf++)
                    {
                        float rotate = (leafTimer * leafSpeed) + (cycleDegrees * leaf);

                        // Invert rotation on odd ring
                        if (ring % 2 == 1)
                        {
                            rotate *= -1f;
                        }

                        Vector2 rotation = new Vector2(0, 50f * (ring + 1)).RotatedBy(MathHelper.ToRadians(rotate));
                        position = npc.Center + rotation;

                        spriteBatch.Draw(
                            leafTexture,
                            position - Main.screenPosition,
                            sourceRectangle, drawColor * 0.5f,
                            rotation.ToRotation() + MathHelper.PiOver2,
                            origin,
                            1f,
                            SpriteEffects.None,
                            0f);
                    }
                }
            }

            return CurrentAttack.PreDraw(spriteBatch, drawColor);
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            Vector2 position;
            SpriteEffects flip;
            Vector2 origin;

            // Draw held weapon
            if (weapon != null)
            {
                position = npc.Center;
                flip = npc.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                origin = weapon.origin;
                if (npc.direction == -1)
                {
                    origin.X = weapon.Texture.Width - origin.X;
                }

                Vector2 direction = (Vector2.UnitX * -npc.direction).RotatedBy(weapon.rotation);

                if (npc.direction == 1)
                    direction.X *= -1f;

                spriteBatch.Draw(
                    weapon.Texture,
                    position - Main.screenPosition,
                    weapon.Texture.Bounds,
                    drawColor,
                    direction.ToRotation(),
                    origin,
                    1f,
                    flip,
                    0f);
            }

            // Draw arm over held weapon
            position = npc.Center;
            position.Y -= 5f;

            origin = new Vector2(npc.direction == 1 ? armTexture.Width : 0f, 0f);
            flip = npc.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            spriteBatch.Draw(armTexture, position - Main.screenPosition, armTexture.Bounds, drawColor, npc.rotation, origin, 1f, flip, 0f);

            CurrentAttack.PostDraw(spriteBatch, drawColor);
        }

        private bool HasValidTarget()
        {
            if (!npc.HasValidTarget) return false;

            Vector2 difference = Target.Center - npc.Center;
            return difference.Length() < 6000f;
        }
    }
    public class HoldWeapon
    {
        public readonly string TexturePath;
        public Vector2 origin = Vector2.Zero;
        public float rotation = 0f;

        public HoldWeapon(string texture, Vector2 origin, float rotation = 0f)
        {
            this.origin = origin;
            this.rotation = rotation;
            TexturePath = texture;
        }

        public Texture2D Texture
        {
            get => ModContent.GetTexture(TexturePath);
        }
    }
}
