using EBF.Abstract_Classes;
using EBF.EbfUtils;
using EBF.Items.Magic;
using EBF.Items.Ranged.Bows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Mono.CompilerServices.SymbolWriter.CodeBlockEntry;

namespace EBF.Items.Summon
{
    public class HonjoMasamune : EBFCatToy, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public override void SetDefaultsSafe()
        {
            Item.width = 12;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 40;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 111;//Item's base damage value
            Item.knockBack = 3f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 15;//How fast the item is used
            Item.useAnimation = 15;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 2, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Yellow;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held

            Item.shoot = ModContent.ProjectileType<HonjoMasamuneStab>();
            BonusMinion = ModContent.ProjectileType<WindElemental>();
        }
        public override void HoldItemSafe(Player player)
        {
            // Speed-up happens in EBFPlayer class
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.BeetleHusk, stack: 5)
                .AddIngredient(ItemID.SoulofFlight, stack: 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class HonjoMasamuneStab : EBFToyStab
    {
        public override void SetDefaultsSafe()
        {
            DrawOffsetX = 0;
            DrawOriginOffsetY = -2;

            ProjOffset = 6;
            BoostDuration = 180;
            TagDamage = 10;
        }
    }
    public class WindElemental : EBFMinion
    {
        public override string Texture => "EBF/Items/Summon/HonjoMasamune_WindElemental";
        public override void SetStaticDefaultsSafe()
        {
            Main.projFrames[Projectile.type] = 23;
        }
        public override void SetDefaultsSafe()
        {
            Projectile.width = 70;
            Projectile.height = 72;
            DetectRange = 500;
            AttackRange = 75;
            MoveSpeed = 16f;
            UseHoverAI = true;
            Projectile.tileCollide = false;
        }
        public override void OnSpawnSafe(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.DD2_BookStaffCast, Projectile.Center);
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Cloud);
            }
        }
        public override void OnAttack(NPC target)
        {
            AttackTime = 20;
            var damage = Projectile.damage;
            var projectile = ModContent.ProjectileType<HonjoMasamune_GaleProjectile>();
            SoundEngine.PlaySound(SoundID.Item7, Projectile.Center);
            if(Projectile.frame < 13)
            {
                Projectile.frame = 12;
                Projectile.frameCounter = 1;
            }
            if (IsBoosted)
            {
                Projectile.frame = 17;
                Projectile.frameCounter = 1;
                AttackTime = 60;
                SoundEngine.PlaySound(SoundID.DD2_BetsyWindAttack, Projectile.Center);
                projectile = ModContent.ProjectileType<HonjoMasamune_GaleBallProjectile>();
                damage = Projectile.damage * 4;
                BoostedEffect();
            }
            Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.Center.DirectionTo(target.Top) * 5, projectile, damage, Projectile.knockBack, Projectile.owner);
        }
        public override void AISafe()
        {
            Animate();
        }
        private void Animate()
        {
            if (Main.GameUpdateCount % 6 == 0)
            {
                Projectile.frame++;

                //Idle animation
                if (Projectile.frameCounter == 0 && Projectile.frame > 1)
                {
                    Projectile.frame = 0;
                }
                //Attacking animation, initial frame set in OnAttack()
                if (Projectile.frameCounter == 1 && Projectile.frame > 17 && !IsBoosted)
                {
                    Projectile.frame = 0;
                    Projectile.frameCounter = 0;
                }
                //Boosted attacking animation, initial frame set in Isboosted in OnAttack()
                if (Projectile.frameCounter == 1 && Projectile.frame > 22 && IsBoosted)
                {
                    Projectile.frame = 0;
                    Projectile.frameCounter = 0;
                }
            }
        }

        private void BoostedEffect()
        {
            //Spawn dust in circle
            int numberOfProjectiles = 8;
            for (float theta = 0; theta <= Math.Tau; theta += (float)Math.Tau / numberOfProjectiles)
            {
                Vector2 vel = Vector2.UnitX.RotatedBy(theta) * 2;
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RedTorch, vel, Scale: 2f);
                dust.noGravity = true;
            }
        }
    }
    public class HonjoMasamune_GaleProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 30;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.scale = 0.1f;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10; // The length of old position to be recorded
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0; // The recording mode
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            return true;
        }
        public override void AI()
        {
            FadeOut();

            Projectile.ai[0] += 1f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; // projectile sprite faces up
            //Scale in
            if (Projectile.scale < 1)
            {
                Projectile.scale += 0.2f;
            }
        }
        public void FadeOut()
        {
            // If last less than 12 ticks — fade in, than more — fade out
            if (Projectile.ai[0] <= 25f)
                // Fade out
                Projectile.alpha += 9;

        }
    }
    public class HonjoMasamune_GaleBallProjectile : ModProjectile
    {
        private bool isFast;

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 30;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.penetrate = -1;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; // projectile sprite faces up
            //Slow over time
            Projectile.velocity *= 0.98f;
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item66, Projectile.Center);

            //Spawn dust
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(Projectile.Center, 0, 0, DustID.Ghost);
            }
                //Spawn rings of gale on death
                const int projectileCount = 16;
                const int  RINGCOUNT = 2;
                float speed;
             
            for (float angle = 0; angle < Math.Tau * RINGCOUNT - 0.01f; angle += (float)Math.Tau / projectileCount)
            {
                speed = isFast ? 6f : 3f * (1 * Main.rand.NextFloat(0.8f, 1.2f));
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    angle.ToRotationVector2() * speed,
                    ModContent.ProjectileType<HonjoMasamune_GaleProjectile>(),
                    Projectile.damage,
                    Projectile.knockBack / 2,
                    Projectile.owner); 
            }
        }
    }
}
