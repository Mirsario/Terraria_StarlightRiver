﻿using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Tiles.Overgrow
{
    internal class SetpieceAltar : ModTile 
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = OvergrowTileLoader.OvergrowTileDir + "SetpieceAltar";
            return true;
        }

        public override void SetDefaults() { QuickBlock.QuickSetFurniture(this, 10, 7, DustID.Stone, SoundID.Tink, true, new Color(100, 100, 80)); } 
    }
}