﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using StarlightRiver.Core;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;

namespace StarlightRiver.Content.Items.BarrierDye
{
	class VitricBossBarrierDye : BarrierDye
	{
		public override string Texture => AssetDirectory.BarrierDyeItem + Name;

		public override float RechargeAnimationRate => 0.01f;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Sentinel's Tincture");
			Tooltip.SetDefault("Causes barrier effect to mimic the shield of the Shattered Sentinel\nEquipable\nVanity Item");
		}

		public override void SetDefaults()
		{
			item.rare = ItemRarityID.Orange;
		}

		public override void LoseBarrierEffects(Player player)
		{
			Main.PlaySound(Terraria.ID.SoundID.Item107, player.Center);

			for (int k = 0; k < 20; k++)
				Dust.NewDustPerfect(player.Center, ModContent.DustType<Dusts.GlassGravity>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(6f), 0, default, 2);
		}

		public override void PostDrawEffects(SpriteBatch spriteBatch, Player player)
		{
			if (!CustomHooks.PlayerTarget.canUseTarget)
				return;

			var barrier = player.GetModPlayer<ShieldPlayer>();

			Texture2D tex = CustomHooks.PlayerTarget.Target;

			var pos = CustomHooks.PlayerTarget.getPositionOffset(player.whoAmI);


			var effect = Terraria.Graphics.Effects.Filters.Scene["MoltenFormAndColor"].GetShader().Shader;
            effect.Parameters["sampleTexture2"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/Bosses/VitricBoss/ShieldMap"));
            effect.Parameters["uTime"].SetValue(barrier.rechargeAnimation * 2 + (barrier.rechargeAnimation >= 1 ? (Main.GameUpdateCount / 30f) % 2f : 0));
            effect.Parameters["sourceFrame"].SetValue(new Vector4((int)pos.X - 30, (int)pos.Y - 60, 60, 120));
            effect.Parameters["texSize"].SetValue(tex.Size());

            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.NonPremultiplied, SamplerState.PointClamp, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

            spriteBatch.Draw(tex, CustomHooks.PlayerTarget.getPlayerTargetPosition(player.whoAmI), CustomHooks.PlayerTarget.getPlayerTargetSourceRectangle(player.whoAmI), Color.White);

            spriteBatch.End();
            spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
		}
	}


}
