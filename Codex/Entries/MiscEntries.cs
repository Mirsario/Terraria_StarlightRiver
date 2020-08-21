﻿using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Codex.Entries
{
    internal class StaminaEntry : CodexEntry
    {
        public StaminaEntry()
        {
            Category = Categories.Misc;
            Title = "Stamina";
            Body = "All of your abilities utilize stamina, a resource represented by the orange diamonds to the right of your mana bar. Your stamina will replenish itself over time. You can gain additional stamina by unlocking new abilities and equipping certain armors and accessories.";
            Hint = "Unlock an ability...";
            Image = GetTexture("StarlightRiver/GUI/Assets/Stamina");
            Icon = GetTexture("StarlightRiver/GUI/Assets/Stamina");
        }
    }

    internal class StaminaShardEntry : CodexEntry
    {
        public StaminaShardEntry()
        {
            Category = Categories.Misc;
            Title = "Stamina Vessels";
            Body = "Shards of these intricate metal devices seem to be scattered around the world everywhere. By combining 3 of them, a completed stamina vessel can be made, allowing it's holder to store an extra unit of stamina. Unfortunately, the strange material and intricate tooling of these fragments makes it nigh impossible to re-create them on your own.";
            Hint = "Find a stamina vessel shard...";
            Image = GetTexture("StarlightRiver/GUI/Assets/StaminaEmpty");
            Icon = GetTexture("StarlightRiver/Pickups/Stamina1");
        }
    }

    internal class InfusionEntry : CodexEntry
    {
        public InfusionEntry()
        {
            Category = Categories.Misc;
            Title = "Infusions";
            Body = "Infusions are powerful magical relics which can augment a user's abilities or change how they work altogether. Two main types of infusions exist, Generic Infusions and Ability Infusions.\n\n" +
                "Generic Infusions typically provide a general boost that applied to all of the player's abilities, or have small enough effects that they do not warrant the ability-type restriction of Ability Infusions. While you can equip as many Generic Infusions as you have slots, you cannot equip duplicates.\n\n" +
                "Ability Infusions are specific to one of your abilities, and greatly augment them or change how they function alltogether. While powerful, these infusions are generally harder to come by than Generic Infusions. You cannot equip two Ability Infusions for the same ability, and you cannot equip duplicates.";
            Hint = "Find a mysterious altar...";
            Image = GetTexture("StarlightRiver/Abilities/Infusion/DashAstral");
            Icon = GetTexture("StarlightRiver/Abilities/Infusion/DashAstral");
        }
    }
}