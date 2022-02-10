using System;
using System.Collections.Generic;
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

    internal class Job
    {

        internal enum RoleStats
        {
            None,
            Tenacity,
            Piety
        }

        internal struct ClassJobData
        {
            internal string JobName { get; set; }
            internal string JobThreeLetter { get; set; }
            internal bool IsPhysical { get; set; }
            internal bool HasHaste { get; set; }
            internal int[,] HasteValues { get; set; }
            internal string HasteName { get; set; }
            internal RoleStats RoleStat { get; set; }


            internal ClassJobData(string name, string tla, bool p, bool h, int[,]? haste, string hname, RoleStats roleStats)
            {
                JobName = name;
                JobThreeLetter = tla;
                IsPhysical = p;
                HasHaste = h;
                if (haste == null)
                {
                    HasteValues = new int[,] { { 0, 0 } };
                    HasteName = "";
                }
                else
                {
                    HasteValues = haste;
                    HasteName = hname;
                }

                RoleStat = roleStats;

            }

        }

         internal static readonly Dictionary<int, ClassJobData> JobData = new() {
            { 1, new ClassJobData("Gladiator", "GLA", true, false, null, "", RoleStats.Tenacity ) },
            { 2, new ClassJobData("Pugilist", "PGL", true, true, new int[,] { { 1, 5 }, { 20, 10 } }, "Greased Lightning", RoleStats.None) },
            { 3, new ClassJobData("Marauder", "MRD", true, false, null, "", RoleStats.Tenacity) },
            { 4, new ClassJobData("Lancer", "LNC", true, false, null, "", RoleStats.None) },
            { 5, new ClassJobData("Archer", "ARC", true, false, null, "", RoleStats.None) },
            { 6, new ClassJobData("Conjurer", "CNJ", false, false, null, "", RoleStats.Piety) },
            { 7, new ClassJobData("Thaumaturge", "THM", false, false, null, "", RoleStats.None) },

            { 19, new ClassJobData("Paladin", "PLD", true, false, null, "", RoleStats.Tenacity) },
            { 20, new ClassJobData("Monk", "MNK", true, true, new int[,] { { 1, 5 }, { 20, 10 }, { 40, 15 }, { 76, 20 } }, "Greased Lightning", RoleStats.None) },
            { 21, new ClassJobData("Warrior", "WAR", true, false, null, "", RoleStats.Tenacity) },
            { 22, new ClassJobData("Dragoon", "DRG", true, false, null, "", RoleStats.None) },
            { 23, new ClassJobData("Bard", "BRD", true, true, new int[,] { { 1, 0 }, { 40, 16 } }, "Army's Paeon [MAX]", RoleStats.None) },
            { 24, new ClassJobData("White Mage", "WHM", false, true, new int[,] { { 1, 0 }, { 30, 20 } }, "Presence of Mind", RoleStats.Piety) },
            { 25, new ClassJobData("Black Mage", "BLM", false, true, new int[,] { { 1, 0 }, { 52, 15 } }, "Ley Lines", RoleStats.None) },
            { 26, new ClassJobData("Arcanist", "ACN", false, false, null, "", RoleStats.None) },
            { 27, new ClassJobData("Summoner", "SMN", false, false, null, "", RoleStats.None) },
            { 28, new ClassJobData("Scholar", "SCH", false, false, null, "", RoleStats.Piety) },
            { 29, new ClassJobData("Rogue", "ROG", true, false, null, "", RoleStats.None) },
            { 30, new ClassJobData("Ninja", "NIN", true, true, new int[,] { { 1, 0 }, { 45, 15 } }, "Huton", RoleStats.None) },
            { 31, new ClassJobData("Machinist", "MCH", true, false, null, "", RoleStats.None) },
            { 32, new ClassJobData("Dark Knight", "DRK", true, false, null, "", RoleStats.Tenacity) },
            { 33, new ClassJobData("Astrologian", "AST", false, false, null, "", RoleStats.Piety) },
            { 34, new ClassJobData("Samurai", "SAM", true, true, new int[,] { { 1, 0 }, { 18, 10 }, { 78, 13 } }, "Shifu", RoleStats.None) },
            { 35, new ClassJobData("Red Mage", "RDM", false, false, null, "", RoleStats.None) },
            { 36, new ClassJobData("Blue Mage", "BLU", false, false, null, "", RoleStats.None) },
            { 37, new ClassJobData("Gunbreaker", "GNB", true, false, null, "", RoleStats.Tenacity) },
            { 38, new ClassJobData("Dancer", "DNC", true, false, null, "", RoleStats.None) },
            { 39, new ClassJobData("Reaper", "RPR", true, false, null, "", RoleStats.None) },
            { 40, new ClassJobData("Sage", "SGE", false, false, null, "", RoleStats.Piety) }
         };
    }

}
