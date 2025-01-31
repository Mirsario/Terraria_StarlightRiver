using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.SteampunkSet
{
	public class RebarLauncher : ModItem
	{
		public override string Texture => AssetDirectory.SteampunkItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Rebar Launcher");
			Tooltip.SetDefault("Impales enemies \nShoot rebar to drive it deeper into enemies");
		}

		//TODO: Adjust rarity sellprice and balance

		Vector2 direction = Vector2.Zero;
		public override void SetDefaults()
		{
			item.damage = 40;
			item.ranged = true;
			item.width = 24;
			item.height = 24;
			item.useTime = 65;
			item.useAnimation = 65;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.noMelee = true;
			item.knockBack = 0;
			item.rare = ItemRarityID.Orange;
			item.shoot = ModContent.ProjectileType<RebarProj>();
			item.shootSpeed = 25f;
			item.autoReuse = true;
		}

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
			Helper.PlayPitched("Guns/RebarLauncher", 0.6f, 0);
			player.GetModPlayer<StarlightPlayer>().Shake += 6;
			direction = new Vector2(speedX, speedY);
			position += new Vector2(speedX, speedY) * 0.9f;
			return true;
        }

        public override void HoldItem(Player player)
        {
			if (player.itemTime > 1)
				Dust.NewDustPerfect(player.Center + (direction.RotatedBy(-0.1f * player.direction) * 0.75f), ModContent.DustType<Dusts.BuzzsawSteam>(), new Vector2(0.2f, -Main.rand.NextFloat(0.7f, 1.6f)), Main.rand.Next(30), Color.White, Main.rand.NextFloat(0.2f, 0.5f));
			base.HoldItem(player);
        }

        public override Vector2? HoldoutOffset()
		{
			return new Vector2(-20, 0);
		}
	}

	public class RebarProj : ModProjectile, IDrawPrimitive
	{
		public override string Texture => AssetDirectory.SteampunkItem + Name;

		const int TRAILLENGTH = 150;

		private List<Vector2> cache;
		private List<Vector2> cache2;
		private Trail trail;
		private Trail trail2;

		private int trailCounter;

		private List<Vector2> offsetCache;
		private List<Vector2> offsetCache2;

		float trailWidth = 4;

		private Vector2 initialVel;

		private int initialDamage;

		private int cooldown;

		private bool collided = false;

		int enemyID = -1;
		bool stuck = false;
		Vector2 offset = Vector2.Zero;

		public List<Projectile> collidedWith = new List<Projectile>();

		public float distanceIn = 0;

		public override bool Autoload(ref string name)
		{
			StarlightRiver.Instance.AddGore(AssetDirectory.SteampunkItem + "RebarProj");
			return base.Autoload(ref name);
		}
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Rebar");
		}

		public override void SetDefaults()
		{
			projectile.penetrate = 1;
			projectile.tileCollide = true;
			projectile.timeLeft = 150;
			projectile.hostile = false;
			projectile.friendly = true;
			projectile.width = projectile.height = 16;
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[projectile.type] = 0;
			projectile.extraUpdates = 5;
		}

		public override bool PreAI()
		{
			if (collided || stuck || projectile.timeLeft % 6== 0)
            {
				if (trailWidth > 0.3f)
				{
					trailWidth *= 0.995f;
					if (trailWidth < 3.5f)
					{
						trailWidth *= 0.95f;
					}
				}
				else
					trailWidth = 0;
			}

			if (!collided)
			{
				if (stuck)
				{
					NPC target = Main.npc[enemyID];
					cooldown--;

					if (!target.active)
					{
						if (projectile.timeLeft > 5)
							projectile.timeLeft = 5;
						projectile.velocity = Vector2.Zero;
					}
					else
					{
						var collidingProj = Main.projectile.Where(n => n.active && n != projectile && !collidedWith.Contains(n) && n.modProjectile is RebarProj modProj && Collision.CheckLinevLine(GetA(), GetB(), modProj.GetA2(), modProj.GetB2()).Length > 0 && !modProj.collidedWith.Contains(projectile) && modProj.distanceIn < distanceIn).OrderBy(n => n.velocity.Length()).FirstOrDefault();

						if (collidingProj != default)
						{
							float angleBetween = Math.Abs(((((collidingProj.rotation - projectile.rotation) % 6.28f) + 9.42f) % 6.28f) - 3.14f);

							float effectiveness = (3.14f - angleBetween) / 3.14f;

							if (effectiveness >= 0.5f)
							{
								var mp = collidingProj.modProjectile as RebarProj;
								mp.collidedWith.Add(collidingProj);

								Vector2 vel = initialVel;
								vel.Normalize();
								vel *= effectiveness;
								vel *= 4;
								collidedWith.Clear();
								if (cooldown < 0)
								{
									projectile.friendly = true;
								}

								Vector2[] dustPositions = Collision.CheckLinevLine(GetA(), GetB(), mp.GetA2(), mp.GetB2());

								if (dustPositions.Length > 0 && initialVel.Length() < 1)
								{
									for (float i = 0; i < 15; i++)
										Dust.NewDustPerfect(dustPositions[0], ModContent.DustType<RebarLine>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(3, 6), 0, new Color(248, 126, 0), Main.rand.NextFloat(0.35f, 0.5f));
								}

								initialVel += vel;

								projectile.damage = initialDamage + (int)Math.Pow(distanceIn, 0.5f);
							}
						}

						offset += initialVel;
						distanceIn += initialVel.Length();

						if (initialVel.Length() > 0.02f)
							initialVel *= 0.5f;

						var targetCollection = Main.npc.Where(n => n.active && !n.townNPC && (Collision.CheckAABBvLineCollision(n.position, n.Size, GetA3(), GetB3()) || Collision.CheckAABBvAABBCollision(projectile.position, projectile.Size, n.position, n.Size))).OrderBy(n => 1).FirstOrDefault();
						if (targetCollection == default)
						{
							projectile.extraUpdates = 0;

							if (projectile.timeLeft > 80)
								projectile.timeLeft = 80;

							projectile.friendly = false;
							projectile.velocity = Vector2.Zero;

							if (!collided)
							{
								projectile.alpha = 255;
								Gore.NewGorePerfect(projectile.Center, initialVel, ModGore.GetGoreSlot(Texture));
								collided = true;
							}
						}

						projectile.position = target.position + offset;
					}
				}

				else
				{
					projectile.rotation = projectile.velocity.ToRotation();
					ManageCaches();
				}
			}
			else
            {
				if (enemyID != -1)
				{
					NPC target = Main.npc[enemyID];
					projectile.position = target.position + offset;
				}
				projectile.alpha += 4;
            }

			ManageTrail();
			return true;
		}

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), GetA3(), GetB3());
		}
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			projectile.penetrate++;
			if (target.life > 0 && enemyID != target.whoAmI)
			{
				enemyID = target.whoAmI;
				offset = projectile.position - target.position;
				projectile.extraUpdates = 0;
			}

			if (stuck)
            {
				cooldown = 30;
				projectile.friendly = false;
            }

			if (!stuck && target.life > 0)
			{
				projectile.extraUpdates = 0;
				initialDamage = projectile.damage;
				initialVel = projectile.velocity;
				stuck = true;
				projectile.friendly = false;
				projectile.tileCollide = false;
				offset -= projectile.velocity;
				projectile.timeLeft = 800;
			}
		}

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
			Texture2D overlay = ModContent.GetTexture(Texture + "_White");
			Texture2D glow = ModContent.GetTexture(Texture + "_Glow");
			Color color = HeatColor(trailWidth / 4f, 0.5f);
			color.A = 0;

			float transparency = 1 - (projectile.alpha / 255f);

			spriteBatch.Draw(glow, projectile.Center - Main.screenPosition, null, color * transparency, projectile.rotation, glow.Size() / 2, projectile.scale, SpriteEffects.None, 0);
			spriteBatch.Draw(Main.projectileTexture[projectile.type], projectile.Center - Main.screenPosition, null, lightColor * transparency, projectile.rotation, new Vector2(30, 3), projectile.scale, SpriteEffects.None, 0);
			spriteBatch.Draw(overlay, projectile.Center - Main.screenPosition, null, HeatColor(trailWidth / 4f, 0.5f) * transparency, projectile.rotation, new Vector2(30, 3), projectile.scale, SpriteEffects.None, 0);
			return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
			projectile.extraUpdates = 0;
			if (projectile.timeLeft > 80)
				projectile.timeLeft = 80;
			projectile.friendly = false;
            projectile.velocity = Vector2.Zero;
			collided = true;
			return false;
        }

        private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();
				offsetCache = new List<Vector2>();
				for (int i = 0; i < TRAILLENGTH; i++)
				{
					cache.Add(GetA4() + GetTop());

					offsetCache.Add(Vector2.Zero);
				}
			}

			cache.Add(GetA4() + GetTop());
			offsetCache.Add((projectile.rotation + 1.57f).ToRotationVector2() * Main.rand.NextFloat(-0.3f, 0.3f));

			while (cache.Count > TRAILLENGTH)
			{
				cache.RemoveAt(0);
				offsetCache.RemoveAt(0);
			}

			if (cache2 == null)
			{
				cache2 = new List<Vector2>();
				offsetCache2 = new List<Vector2>();
				for (int i = 0; i < TRAILLENGTH; i++)
				{
					cache2.Add(GetA4() + GetTop());
					offsetCache2.Add(Vector2.Zero);
				}
			}

			cache2.Add(GetA4() + GetTop());
			offsetCache2.Add((projectile.rotation - 1.57f).ToRotationVector2() * Main.rand.NextFloat(-0.3f, 0.3f));

			while (cache2.Count > TRAILLENGTH)
			{
				cache2.RemoveAt(0);
				offsetCache2.RemoveAt(0);
			}
			trailCounter++;
		}

		private void ManageTrail()
		{

			trail = trail ?? new Trail(Main.instance.GraphicsDevice, TRAILLENGTH, new TriangularTip(40), factor => factor * trailWidth * 4f, factor =>
			{
				return Color.Red;
			});

			trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, TRAILLENGTH, new TriangularTip(40), factor => factor * trailWidth * 4f, factor =>
			{
				return Color.Red;
			});

			if (trailWidth < 3.9f && (collided || stuck || projectile.timeLeft % 6 == 0))
			{
				for (int i = 0; i < cache.Count; i++)
				{
					cache[i] += offsetCache[i] * 2f;
				}

				for (int i = 0; i < cache2.Count; i++)
				{
					cache2[i] += offsetCache2[i];
				}
			}

			trail.Positions = cache.ToArray();

			trail2.Positions = cache2.ToArray();

			if (!stuck)
			{
				trail.NextPosition = GetA4() + (projectile.rotation.ToRotationVector2() * 25) + GetTop();
				trail2.NextPosition = GetA4() + (projectile.rotation.ToRotationVector2() * 25) + GetTop();
			}
		}

		public Vector2 GetA()
		{
			return projectile.Center + ((projectile.rotation + 0.3f).ToRotationVector2() * 40);
		}

		public Vector2 GetB()
		{
			return projectile.Center - ((projectile.rotation + 0.3f).ToRotationVector2() * 40);
		}

		public Vector2 GetA2()
		{
			return projectile.Center + ((projectile.rotation - 0.3f).ToRotationVector2() * 40);
		}

		public Vector2 GetB2()
		{
			return projectile.Center - ((projectile.rotation - 0.3f).ToRotationVector2() * 40);
		}

		public Vector2 GetA3()
		{
			return projectile.Center + ((projectile.rotation).ToRotationVector2() * 30);
		}

		public Vector2 GetB3()
		{
			return projectile.Center - ((projectile.rotation).ToRotationVector2() * 30);
		}

		public Vector2 GetA4()
		{
			return projectile.Center + ((projectile.rotation).ToRotationVector2() * 35);
		}

		public Vector2 GetTop()
        {
			return (projectile.rotation - 1.57f).ToRotationVector2() * 5;
        }
		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["RebarTrail"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture(AssetDirectory.SteampunkItem + "RebarTrailTexture"));
			effect.Parameters["noiseTexture"].SetValue(ModContent.GetTexture(AssetDirectory.SteampunkItem + "RebarNoiseTexture"));
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["progress"].SetValue(trailWidth / 4f);
			effect.Parameters["repeats"].SetValue(18);
			effect.Parameters["midColor"].SetValue(new Color(248, 126, 0).ToVector3());


			trail?.Render(effect);
			trail2?.Render(effect);
		}

		private Color HeatColor(float progress, float midpoint)
        {
			Color orange = new Color(248, 126, 0);
			if (progress > midpoint)
				return Color.Lerp(Color.Red, orange, (progress - midpoint) / (1 - midpoint));
			else
				return Color.Lerp(Color.Red, new Color(0, 0, 0, 0), (midpoint - progress) / midpoint);
		}
	}
}