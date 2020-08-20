﻿using Microsoft.Xna.Framework;
using StarlightRiver.Abilities.Content;
using Terraria;

namespace StarlightRiver.Abilities
{
    //This class serves to simplify implementing ability interactions
    internal static class AbilityHelper
    {
        public static bool CheckDash(Player player, Rectangle hitbox)
        {
            return player.ActiveAbility<Dash>() && Collision.CheckAABBvAABBCollision(player.Hitbox.TopLeft(), player.Hitbox.Size(), hitbox.TopLeft(), hitbox.Size());
        }

        public static bool CheckWisp(Player player, Rectangle hitbox)
        {
            return player.ActiveAbility<Wisp>() && Collision.CheckAABBvAABBCollision(player.Hitbox.TopLeft(), player.Hitbox.Size(), hitbox.TopLeft(), hitbox.Size());
        }

        public static bool CheckSmash(Player player, Rectangle hitbox)
        {
            return player.ActiveAbility<Smash>() && Collision.CheckAABBvAABBCollision(player.Hitbox.TopLeft(), player.Hitbox.Size(), hitbox.TopLeft(), hitbox.Size());
        }

        public static bool ActiveAbility<T>(this Player player) where T : Ability => player.GetHandler().ActiveAbility is T;

        public static AbilityHandler GetHandler(this Player player) => player.GetModPlayer<AbilityHandler>();
    }
}