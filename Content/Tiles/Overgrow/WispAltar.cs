﻿using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Overgrow
{
	class WispAltarL : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.OvergrowTile + "WispAltarL";
            return true;
        }

        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 6, 11, DustType<Dusts.GoldNoMovement>(), SoundID.Tink, false, new Color(200, 200, 200));
    }

    class WispAltarLItem : QuickTileItem
    {
        public override string Texture => AssetDirectory.Debug;

        public WispAltarLItem() : base("Wisp Altar L Placer", "Debug item", TileType<WispAltarL>(), -1) { }

    }

    class WispAltarR : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.OvergrowTile + "WispAltarR";
            return true;
        }

        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 6, 11, DustType<Dusts.GoldNoMovement>(), SoundID.Tink, false, new Color(200, 200, 200));
    }

    class WispAltarRItem : QuickTileItem
    {
        public override string Texture => AssetDirectory.Debug;

        public WispAltarRItem() : base("Wisp Altar R Placer", "Debug item", TileType<WispAltarR>(), -1) { }
    }
}
