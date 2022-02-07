using System;
using Dalamud;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace SubstatTiers
{
    internal class Retrieval
    {

        public static unsafe Attributes GetDataFromGame()
        {
            
            var aState = UIState.Instance()->PlayerState;
            int[] attribs = new int[74];

            for (int i = 0; i < 74; i++)
            {
                attribs[i] = aState.Attributes[i];
            }
            // Links known attribute values to fields
            
            Attributes attributes = new();
            byte Synced = aState.IsLevelSynced;
            if (Synced > 0)
            {
                attributes.Level = aState.SyncedLevel;
            }
            else
            {
                attributes.Level = aState.CurrentLevel;
            }
            attributes.Strength = aState.Attributes[1];
            attributes.Dexterity = aState.Attributes[2];
            attributes.Vitality = aState.Attributes[3];
            attributes.Intelligence = aState.Attributes[4];
            attributes.Mind = aState.Attributes[5];
            attributes.Piety = aState.Attributes[6];
            attributes.MaxHP = aState.Attributes[7];
            attributes.MaxMP = aState.Attributes[8];
            attributes.MaxTP = aState.Attributes[9];
            attributes.MaxGP = aState.Attributes[10];
            attributes.MaxCP = aState.Attributes[11];

            attributes.AutoAttackDelay = aState.Attributes[14];

            attributes.Tenacity = aState.Attributes[19];
            attributes.AttackPower = aState.Attributes[20];
            attributes.Defense = aState.Attributes[21];
            attributes.DirectHit = aState.Attributes[22];
            // attributes.BaseAttackSpeed = aState.Attributes[23];
            attributes.MagicDefense = aState.Attributes[24];

            attributes.CriticalHit = aState.Attributes[27];

            attributes.AttackMagicPotency = aState.Attributes[33];
            attributes.HealingMagicPotency = aState.Attributes[34];

            attributes.Determination = aState.Attributes[44];
            attributes.SkillSpeed = aState.Attributes[45];
            attributes.SpellSpeed = aState.Attributes[46];
            attributes.Haste = aState.Attributes[47];

            attributes.Craftsmanship = aState.Attributes[70];
            attributes.Control = aState.Attributes[71];
            attributes.Gathering = aState.Attributes[72];
            attributes.Perception = aState.Attributes[73];

            attributes.JobId = aState.CurrentClassJobId;

            return attributes;


        }
    }
}
