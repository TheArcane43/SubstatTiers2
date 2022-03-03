using System;
using System.Collections.Generic;
using Dalamud;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.GeneratedSheets;

namespace SubstatTiers
{

    // Class that contains all of the relevant job data

    internal class AttributeData
    {

        // Main stats
        internal int Level { get; set; }
        internal JobThreeLetter JobId { get; set; }
        internal int Strength { get; set; }
        internal int Dexterity { get; set; }
        internal int Vitality { get; set; }
        internal int Intelligence { get; set; }
        internal int Mind { get; set; }

        // substats
        internal int CriticalHit { get; set; }
        internal int Determination { get; set; }
        internal int DirectHit { get; set; }
        internal int SkillSpeed { get; set; }
        internal int SpellSpeed { get; set; }
        internal int Tenacity { get; set; }
        internal int Piety { get; set; }

        // other stats
        internal int MaxHP { get; set; }
        internal int MaxMP { get; set; }
        internal int MaxTP { get; set; }
        internal int AutoAttackDelay { get; set; }
        internal int AttackPower { get; set; }
        internal int AttackMagicPotency { get; set; }
        internal int HealingMagicPotency { get; set; }
        internal int Defense { get; set; }
        internal int MagicDefense { get; set; }
        internal int Haste { get; set; }
        internal int PhysicalWeaponDamage { get; set; }
        internal int MagicalWeaponDamage { get; set; }

        // Non-battle specific
        internal int MaxGP { get; set; }
        internal int MaxCP { get; set; }
        internal int Craftsmanship { get; set; }
        internal int Control { get; set; }
        internal int Gathering { get; set; }
        internal int Perception { get; set; }

        // Utility fields
        internal bool IsLoaded { get; set; }

        // default constructor dereferences the instance pointer
        public unsafe AttributeData()
        {
            var aState = UIState.Instance()->PlayerState;
            // aState is an object with the player state
            // it has an array of 74 `int` fields (Attributes), this code links known fields to useful variables
            // and other fields like CurrentLevel, CurrentClassJobId, etc.

            IsLoaded = aState.IsLoaded > 0;

            byte Synced = aState.IsLevelSynced;
            if (Synced > 0)
            {
                Level = aState.SyncedLevel;
            }
            else
            {
                Level = aState.CurrentLevel;
            }
            Strength = aState.Attributes[1];
            Dexterity = aState.Attributes[2];
            Vitality = aState.Attributes[3];
            Intelligence = aState.Attributes[4];
            Mind = aState.Attributes[5];
            Piety = aState.Attributes[6];
            MaxHP = aState.Attributes[7];
            MaxMP = aState.Attributes[8];
            MaxTP = aState.Attributes[9];
            MaxGP = aState.Attributes[10];
            MaxCP = aState.Attributes[11];

            AutoAttackDelay = aState.Attributes[14];

            Tenacity = aState.Attributes[19];
            AttackPower = aState.Attributes[20];
            Defense = aState.Attributes[21];
            DirectHit = aState.Attributes[22];
            // this.BaseAttackSpeed = aState.Attributes[23];
            MagicDefense = aState.Attributes[24];

            CriticalHit = aState.Attributes[27];

            AttackMagicPotency = aState.Attributes[33];
            HealingMagicPotency = aState.Attributes[34];

            Determination = aState.Attributes[44];
            SkillSpeed = aState.Attributes[45];
            SpellSpeed = aState.Attributes[46];
            Haste = aState.Attributes[47];

            Craftsmanship = aState.Attributes[70];
            Control = aState.Attributes[71];
            Gathering = aState.Attributes[72];
            Perception = aState.Attributes[73];

            JobId = (JobThreeLetter)aState.CurrentClassJobId;

            var r = InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->Items[0];
            var w = r.ItemID;

            var p = Service.DataManager.GetExcelSheet<Item>()?.GetRow(w);
            PhysicalWeaponDamage = p?.DamagePhys ?? 0;
            MagicalWeaponDamage = p?.DamageMag ?? 0;

            if (r.Flags.HasFlag(InventoryItem.ItemFlags.HQ))
            {
                PhysicalWeaponDamage = (int)Math.Round(PhysicalWeaponDamage * 1.1117);
                MagicalWeaponDamage = (int)Math.Round(MagicalWeaponDamage * 1.1117);
            }

            // PluginLog.Information($"Rounding check: {PhysicalWeaponDamage}");

        }

        // Get a class/job's three letter identifier from its id (which is unique)
        internal string GetJobTL() => JobId.ToString();
        // Is this a disciple of the hand or land? substat tiers do not apply here
        internal bool IsHandLand()
        {
            return JobId switch
            {
                JobThreeLetter.CRP => true,
                JobThreeLetter.BSM => true,
                JobThreeLetter.ARM => true,
                JobThreeLetter.GSM => true,
                JobThreeLetter.LTW => true,
                JobThreeLetter.WVR => true,
                JobThreeLetter.ALC => true,
                JobThreeLetter.CUL => true,
                JobThreeLetter.MIN => true,
                JobThreeLetter.BTN => true,
                JobThreeLetter.FSH => true,
                _ => false,
            };
        }
        internal bool UsesAttackPower()
        {
            return JobId switch
            {
                JobThreeLetter.GLA => true,
                JobThreeLetter.PGL => true,
                JobThreeLetter.MRD => true,
                JobThreeLetter.LNC => true,
                JobThreeLetter.ARC => true,
                JobThreeLetter.PLD => true,
                JobThreeLetter.MNK => true,
                JobThreeLetter.WAR => true,
                JobThreeLetter.DRG => true,
                JobThreeLetter.BRD => true,
                JobThreeLetter.ROG => true,
                JobThreeLetter.NIN => true,
                JobThreeLetter.MCH => true,
                JobThreeLetter.DRK => true,
                JobThreeLetter.SAM => true,
                JobThreeLetter.GNB => true,
                JobThreeLetter.DNC => true,
                JobThreeLetter.RPR => true,
                _ => false,
            };
        }
        internal int WeaponPower => UsesAttackPower() ? PhysicalWeaponDamage : MagicalWeaponDamage;
        internal StatConstants.SubstatType SpeedType => UsesAttackPower() ? StatConstants.SubstatType.SkSpd : StatConstants.SubstatType.SpSpd;
        internal bool UsesCasterTraits()
        {
            return JobId switch
            {
                JobThreeLetter.CNJ => true,
                JobThreeLetter.THM => true,
                JobThreeLetter.WHM => true,
                JobThreeLetter.BLM => true,
                JobThreeLetter.ACN => true,
                JobThreeLetter.SMN => true,
                JobThreeLetter.SCH => true,
                JobThreeLetter.AST => true,
                JobThreeLetter.RDM => true,
                JobThreeLetter.SGE => true,
                _ => false,
            };
        }
        internal int HasteAmount()
        {
            return (JobId, Level) switch
            {
                (JobThreeLetter.PGL or JobThreeLetter.MNK, < 20) => 5,
                (JobThreeLetter.PGL or JobThreeLetter.MNK, < 40) => 10,
                (JobThreeLetter.PGL or JobThreeLetter.MNK, < 76) => 15,
                (JobThreeLetter.PGL or JobThreeLetter.MNK, >= 76) => 20,
                (JobThreeLetter.BRD, >= 40) => 16,
                (JobThreeLetter.WHM, >= 30) => 20,
                (JobThreeLetter.BLM, >= 52) => 15,
                (JobThreeLetter.NIN, >= 45) => 15,
                (JobThreeLetter.AST, >= 50) => 10,
                (JobThreeLetter.SAM, < 18) => 0,
                (JobThreeLetter.SAM, < 78) => 10,
                (JobThreeLetter.SAM, >= 78) => 13,
                _ => 0,
            };
        }
        internal string HasteName()
        {
            return JobId switch
            {
                JobThreeLetter.PGL or JobThreeLetter.MNK => "Greased Lightning",
                JobThreeLetter.BRD => "Army's Paeon [MAX]",
                JobThreeLetter.WHM => "Presence of Mind",
                JobThreeLetter.BLM => "Ley Lines",
                JobThreeLetter.NIN => "Huton",
                JobThreeLetter.AST => "Astrodyne",
                JobThreeLetter.SAM => "Shifu",
                _ => "",
            };
        }

        internal double TraitAmount()
        {
            // check for blue mage specifically, as it has a different trait system
            if (JobId == JobThreeLetter.BLU)
            {
                return Level switch
                {
                    >= 50 => 0.50,
                    >= 40 => 0.40,
                    >= 30 => 0.30,
                    >= 20 => 0.20,
                    >= 10 => 0.10,
                    _ => 0.00,
                };
            }
            // check casters with maim and mend
            if (UsesCasterTraits())
            {
                return Level switch
                {
                    >= 40 => 0.30,
                    >= 20 => 0.10,
                    _ => 0.00,
                };
            }
            // check ranged class/jobs
            return (JobId, Level) switch
            {
                (JobThreeLetter.ARC or JobThreeLetter.BRD or JobThreeLetter.MCH, >= 40) => 0.20,
                (JobThreeLetter.ARC or JobThreeLetter.BRD or JobThreeLetter.MCH, >= 20) => 0.10,
                (JobThreeLetter.DNC, >= 60) => 0.20,
                (JobThreeLetter.DNC, >= 50) => 0.10,
                _ => 0.00, // default to 0
            };
        }
        internal int AttackMod()
        {
            return JobId switch
            {
                JobThreeLetter.GLA => 95,
                JobThreeLetter.PGL => 100,
                JobThreeLetter.MRD => 100,
                JobThreeLetter.LNC => 105,
                JobThreeLetter.ARC => 105,
                JobThreeLetter.CNJ => 105,
                JobThreeLetter.THM => 105,
                JobThreeLetter.PLD => 100,
                JobThreeLetter.MNK => 110,
                JobThreeLetter.WAR => 105,
                JobThreeLetter.DRG => 115,
                JobThreeLetter.BRD => 115,
                JobThreeLetter.WHM => 115,
                JobThreeLetter.BLM => 115,
                JobThreeLetter.ACN => 105,
                JobThreeLetter.SMN => 115,
                JobThreeLetter.SCH => 115,
                JobThreeLetter.ROG => 100,
                JobThreeLetter.NIN => 110,
                JobThreeLetter.MCH => 115,
                JobThreeLetter.DRK => 105,
                JobThreeLetter.AST => 115,
                JobThreeLetter.SAM => 112,
                JobThreeLetter.RDM => 115,
                JobThreeLetter.BLU => 115,
                JobThreeLetter.GNB => 100,
                JobThreeLetter.DNC => 115,
                JobThreeLetter.RPR => 115,
                JobThreeLetter.SGE => 115,
                _ => 0,
            };
        }
        internal bool IsTank()
        {
            return JobId switch
            {
                JobThreeLetter.GLA => true,
                JobThreeLetter.MRD => true,
                JobThreeLetter.PLD => true,
                JobThreeLetter.WAR => true,
                JobThreeLetter.DRK => true,
                JobThreeLetter.GNB => true,
                _ => false,
            };
        }
        internal bool IsHealer()
        {
            return JobId switch
            {
                JobThreeLetter.CNJ => true,
                JobThreeLetter.WHM => true,
                JobThreeLetter.SCH => true,
                JobThreeLetter.AST => true,
                JobThreeLetter.SGE => true,
                _ => false,
            };
        }

    }

    internal enum JobThreeLetter
    {
        GLA = 1,
        PGL = 2,
        MRD = 3,
        LNC = 4,
        ARC = 5,
        CNJ = 6,
        THM = 7,
        CRP = 8,
        BSM = 9,
        ARM = 10,
        GSM = 11,
        LTW = 12,
        WVR = 13,
        ALC = 14,
        CUL = 15,
        MIN = 16,
        BTN = 17,
        FSH = 18,
        PLD = 19,
        MNK = 20,
        WAR = 21,
        DRG = 22,
        BRD = 23,
        WHM = 24,
        BLM = 25,
        ACN = 26,
        SMN = 27,
        SCH = 28,
        ROG = 29,
        NIN = 30,
        MCH = 31,
        DRK = 32,
        AST = 33,
        SAM = 34,
        RDM = 35,
        BLU = 36,
        GNB = 37,
        DNC = 38,
        RPR = 39,
        SGE = 40,

    }

}
