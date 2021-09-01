using System.Collections.Generic;
using System.IO;
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
        private readonly HoldWeapon weapon = new HoldWeapon();
        private const int wingFrames = 4;
        private int wingsFrameCount = 0;

        // Local AI
        private int wingsFrame = 0;
        private bool initAttack = true;
        private bool transitioning = false;
        private bool despawn = false;

        public MisterCarlos()
        {
            attacks[0] = new List<NPCAttack<MisterCarlos>> { new TestAttack(this) };
            attacks[1] = new List<NPCAttack<MisterCarlos>> { new TestAttack(this) };
            attacks[2] = new List<NPCAttack<MisterCarlos>> { new TestAttack(this) };
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("MisterCarlos");
        }

        public override void SetDefaults()
        {
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

            float lifeRadius = (float)npc.life / npc.lifeMax;
            float transitionDuration = 180f;

            bool doFirstTransition = Phase == 0 && lifeRadius <= (Main.expertMode ? 0.7f : 0.5f);
            bool doSecondTransition = Main.expertMode && Phase == 1 && lifeRadius <= 0.25f;

            if (!transitioning)
            {
                if (doFirstTransition)
                {
                    Phase = 1;
                    StartTransition();
                } else if (doSecondTransition)
                {
                    Phase = 2;
                    StartTransition();
                } 
            }

            if (transitioning)
            {
                // Transition AI
                npc.dontTakeDamage = true;
                npc.velocity *= 0.93f;

                // Slow down, then run transition timer
                if (npc.velocity.Length() < 0.3f)
                {
                    npc.velocity = Vector2.Zero;

                    if (++CycleTimer >= transitionDuration)
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
            } else
            {
                // Actual fight AI
                if (initAttack)
                {
                    npc.TargetClosest(false);
                    if (!npc.HasValidTarget)
                    {
                        despawn = true;
                    } else
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

                    // Progress cycle
                    CycleTimer++;
                    if (Main.netMode != NetmodeID.MultiplayerClient && CycleTimer >= CurrentAttack.Duration)
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

            npc.netUpdate = true;
        }

        private void DespawnAI()
        {
            // Stop despawning if target found
            npc.TargetClosest(false);
            if (Main.netMode != NetmodeID.MultiplayerClient && npc.HasValidTarget)
            {
                CycleTimer = 0f;
                initAttack = true;
                despawn = false;

                npc.netUpdate = true;
                return;
            }

            // Actual despawn AI
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

            // Death particles
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
            //writer.Write(despawn);

            CurrentAttack.SendExtraAI(writer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            initAttack = reader.ReadBoolean();
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
            Rectangle rect = new Rectangle(0, wingsFrame * frameHeight, wingsTexture.Width, frameHeight);

            Vector2 position = npc.Center;
            position.Y -= 28f;
            position.X -= wingsTexture.Width / 2 + (8 * npc.direction);

            SpriteEffects flip = npc.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(wingsTexture, position - Main.screenPosition, rect, drawColor, npc.rotation, Vector2.Zero, 1f, flip, 0f);

            return true;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            Vector2 position;
            SpriteEffects flip;
            Vector2 origin;

            // Draw holding weapon
            if (weapon.Texture != null)
            {
                position = npc.Center;
                flip = npc.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                origin = weapon.origin;
                if (npc.direction == -1)
                {
                    // Flip origin
                    origin.X = weapon.Texture.Width - origin.X;
                }

                float maxRotation = MathHelper.PiOver2;
                float rotation = MathHelper.Clamp(weapon.rotation, -maxRotation, maxRotation);

                if (npc.direction == 1)
                {
                    // Flip rotation to other side
                    rotation = (maxRotation * 2) - (rotation + maxRotation) - maxRotation;
                }

                spriteBatch.Draw(
                    weapon.Texture,
                    position - Main.screenPosition,
                    weapon.Texture.Bounds,
                    drawColor,
                    rotation,
                    origin,
                    1f,
                    flip,
                    0f);
            }

            // Draw arm
            position = npc.Center;
            position.Y -= 5f;

            origin = new Vector2(npc.direction == 1 ? armTexture.Width : 0f, 0f);
            flip = npc.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            spriteBatch.Draw(armTexture, position - Main.screenPosition, armTexture.Bounds, drawColor, npc.rotation, origin, 1f, flip, 0f);
            
        }

        public class HoldWeapon
        {
            private int id = 0;
            public Vector2 origin = Vector2.Zero;
            public float rotation = 0f;

            public Texture2D Texture { get; private set; }

            public int Item
            {
                get => id;
                set
                {
                    Texture = ModContent.GetTexture("Terraria/Item_" + value);
                    origin = new Vector2(Texture.Width / 8f, Texture.Height / 2f);
                    id = value;
                }
            }
        }
    }
}
