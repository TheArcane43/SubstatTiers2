﻿using System;
using Dalamud.Data;
using Lumina.Excel.GeneratedSheets;
using FFXIVClientStructs.FFXIV.Client.Game;
using Dalamud.Logging;

namespace SubstatTiers
{
    public class Calculations
    {

        internal AttributeData Data { get; set; }

        internal int Speed { get; set; } = 400;

        internal int GetUnits(StatConstants.SubstatType substat, int extraAmount = 0)
        {
            if (substat == StatConstants.SubstatType.GCDbase)
            {
                return GetSpeedUnitsOfGCDbase();
            }
            if (substat == StatConstants.SubstatType.GCDmodified)
            {
                return GetSpeedUnitsOfGCDmodified();
            }
            int div = StatConstants.GetDivAtLevel(Data.Level);
            int stat = substat switch
            {
                StatConstants.SubstatType.Crit => Data.CriticalHit,
                StatConstants.SubstatType.Det => Data.Determination,
                StatConstants.SubstatType.Direct => Data.DirectHit,
                StatConstants.SubstatType.SkSpd => Data.SkillSpeed,
                StatConstants.SubstatType.SpSpd => Data.SpellSpeed,
                StatConstants.SubstatType.Ten => Data.Tenacity,
                StatConstants.SubstatType.Piety => Data.Piety,
                StatConstants.SubstatType.Defense => Data.Defense,
                StatConstants.SubstatType.MagicDefense => Data.MagicDefense,
                _ => 1,
            };
            int baseStat = StatConstants.GetBaseStatAtLevel(Data.Level, substat);
            int coeff = StatConstants.GetCoefficient(substat);

            return Formulas.StatFormula(div, stat + extraAmount, baseStat, coeff);
        }

        // Given units, return the amount of stat needed
        internal int GetStatsFromUnits(StatConstants.SubstatType substat, int units)
        {
            int div = StatConstants.GetDivAtLevel(Data.Level);
            int baseStat = StatConstants.GetBaseStatAtLevel(Data.Level, substat);
            int coeff = StatConstants.GetCoefficient(substat);

            return Formulas.ReverseStatFormula(div, units, baseStat, coeff);
        }

        internal Calculations(AttributeData a)
        {
            Data = a;
            Speed = Data.UsesAttackPower() ? Data.SkillSpeed : Data.SpellSpeed;
        }


        internal double GetGCDbase() => Formulas.GCDFormula(GetUnits(Data.SpeedType), 0);
        internal double GetGCDmodified() => Formulas.GCDFormula(GetUnits(Data.SpeedType), Data.HasteAmount());
        internal int GetSpeedUnitsOfGCDbase() => Formulas.ReverseGCDFormula(GetGCDbase(), 0);
        internal int GetSpeedUnitsOfGCDmodified() => Formulas.ReverseGCDFormula(GetGCDmodified(), Data.HasteAmount());
        internal int GetSpeedUnitsOfNextGCDbase() => Formulas.ReverseGCDFormula(GetGCDbase() - 0.010000001, 0);
        internal int GetSpeedUnitsOfNextGCDmodified() => Formulas.ReverseGCDFormula(GetGCDmodified() - 0.010000001, Data.HasteAmount());
        // NOTE: due to rounding and/or float precision, the 0.010000001 is necessary to actually move to the next GCD
        //   don't know which one it is, but it seems to work with every test

        internal int FunctionWD()
        {
            int job = Data.AttackMod();
            int main = StatTiers.GetStat(Data.Level, StatTiers.DataType.Main);
            int weapon = Data.WeaponPower;

            return (int)(Math.Floor(main * job / 1000.0) + weapon);
        }
        private static int AttackConstant(bool isTank, int level)
        {
            if (isTank)
            {
                return level switch
                {
                    <= 50 => 52,
                    <= 60 => (int)Math.Floor(1.75 * (level - 50)) + 53,
                    <= 70 => (int)Math.Floor(2.7 * (level - 60)) + 78,
                    <= 80 => level + 35, // 1 * (level - 70) + 105
                    <= 90 => 4 * (level - 80) + 115,
                    _ => 156,
                };
                // Tank attack constant is tested for 60, 70-80, 90
            }
            else
            {
                return level switch
                {
                    <= 50 => 75,
                    <= 70 => (int)Math.Floor(2.5 * (level - 50)) + 75,
                    <= 80 => 4 * (level - 70) + 125,
                    <= 90 => 3 * (level - 80) + 165,
                    _ => 195,
                };
            }
            
        }

        private int FunctionAP()
        {
            int atkConstant = AttackConstant(Data.IsTank(), Data.Level);
            int att = Data.UsesAttackPower() ? Data.AttackPower : Data.AttackMagicPotency;
            int main = StatTiers.GetStat(Data.Level, StatTiers.DataType.Main);

            return (int)(Math.Floor(atkConstant * (att - main) / (double)main) + 100);
        }

        // Returns 1000 + tiers of determination
        private int FunctionDET()
        {
            int det = Data.Determination;
            int main = StatTiers.GetStat(Data.Level, StatTiers.DataType.Main);
            int div = StatTiers.GetStat(Data.Level, StatTiers.DataType.Div);
            int coeff = StatConstants.GetCoefficient(StatConstants.SubstatType.Det);
            return Formulas.StatFormula(div, det, main, coeff) + 1000;
        }

        // Returns 1000 + tiers of tenacity
        private int FunctionTEN()
        {
            int ten = Data.Tenacity;
            int sub = StatTiers.GetStat(Data.Level, StatTiers.DataType.Sub);
            int div = StatTiers.GetStat(Data.Level, StatTiers.DataType.Div);
            int coeff = StatConstants.GetCoefficient(StatConstants.SubstatType.Ten);

            return Formulas.StatFormula(div, ten, sub, coeff) + 1000;
        }

        // Returns 1000 + tiers of speed
        private int FunctionSPD()
        {
            int spd = Data.UsesAttackPower() ? Data.SkillSpeed : Data.SpellSpeed;
            int sub = StatTiers.GetStat(Data.Level, StatTiers.DataType.Sub);
            int div = StatTiers.GetStat(Data.Level, StatTiers.DataType.Div);
            int coeff = StatConstants.GetCoefficient(Data.UsesAttackPower() ? StatConstants.SubstatType.SkSpd : StatConstants.SubstatType.SpSpd);

            return Formulas.StatFormula(div, spd, sub, coeff) + 1000;
        }

        // Returns 1400 + tiers of critical hit (base crit damage is 40.0%)
        private int FunctionCRIT()
        {
            int crit = Data.CriticalHit;
            int sub = StatTiers.GetStat(Data.Level, StatTiers.DataType.Sub);
            int div = StatTiers.GetStat(Data.Level, StatTiers.DataType.Div);
            int coeff = StatConstants.GetCoefficient(StatConstants.SubstatType.Crit);

            return Formulas.StatFormula(div, crit, sub, coeff) + 1400;
        }

        internal int DamageFormula(bool isCrit, bool isDirect, int potency = 100)
        {
            int pot = potency;
            int atk = FunctionAP();
            int det = FunctionDET();
            int tnc = Data.IsTank() ? FunctionTEN() : 1000;
            int wep = FunctionWD();
            int tr = 100 + (int)(Data.TraitAmount() * 100);
            int crit = isCrit ? FunctionCRIT() : 1000;
            int dh = isDirect ? 125 : 100;

            // Formula (floor after every step): [potency * atk * det / 100] / 1000 * tnc / 1000 * wep / 100 * trait / 100 * crit / 1000 * dh / 100
            long baseCalc = (long)pot * atk * det / 100;
            int flatCalc = (int)Math.Floor(Math.Floor(Math.Floor(Math.Floor(baseCalc / 1000.0) * tnc / 1000.0) * wep / 100.0) * tr / 100.0);
            int procCalc = (int)Math.Floor(Math.Floor(flatCalc * crit / 1000.0) * dh / 100.0);

            // Testing!
            // PluginLog.Information($"Test calculations: {procCalc}");

            return procCalc;
        }

        internal int DamageAverage(int potency = 100)
        {
            int baseDamage = DamageFormula(false, false, potency);
            int critDamage = DamageFormula(true, false, potency);
            int dirDamage  = DamageFormula(false, true, potency);
            int dcDamage = DamageFormula(true, true, potency);

            double critRate = GetUnits(StatConstants.SubstatType.Crit) / 1000.0 + 0.05;
            double dirRate = GetUnits(StatConstants.SubstatType.Direct) / 1000.0;
            double dcRate = critRate * dirRate;
            double trueCritRate = critRate - dcRate;
            double trueDirRate = dirRate - dcRate;
            double noneRate = 1 - trueCritRate - trueDirRate - dcRate;

            return (int)(noneRate * baseDamage + trueCritRate * critDamage + trueDirRate * dirDamage + dcRate * dcDamage);
        }

        internal int DamageOverTimeFormula(bool isCrit, bool isDirect, int potency = 100)
        {
            int pot = potency;
            int atk = FunctionAP();
            int det = FunctionDET();
            int tnc = Data.IsTank() ? FunctionTEN() : 1000;
            int wep = FunctionWD();
            int tr = 100 + (int)(Data.TraitAmount() * 100);
            int crit = isCrit ? FunctionCRIT() : 1000;
            int dh = isDirect ? 125 : 100;
            int spd = FunctionSPD();

            // Formula (floor after every step): [see calculations below]
            long baseCalc = (long)pot * atk * det / 100;
            int flatCalc = (int)Math.Floor(Math.Floor(Math.Floor(baseCalc / 1000.0) * tnc / 1000.0) * wep / 100.0);
            int speedCalc = (int)Math.Floor(Math.Floor(Math.Floor(flatCalc * spd / 1000.0) * tr / 100.0) + 1);
            int randCalc = (int)Math.Floor(Math.Floor(Math.Floor(speedCalc * crit / 1000.0) * dh / 100.0));
            return randCalc;
        }

        internal int DamageOverTimeAverage(int potency = 100)
        {
            int baseDamage = DamageOverTimeFormula(false, false, potency);
            int critDamage = DamageOverTimeFormula(true, false, potency);
            int dirDamage = DamageOverTimeFormula(false, true, potency);
            int dcDamage = DamageOverTimeFormula(true, true, potency);

            double critRate = GetUnits(StatConstants.SubstatType.Crit) / 1000.0 + 0.05;
            double dirRate = GetUnits(StatConstants.SubstatType.Direct) / 1000.0;
            double dcRate = critRate * dirRate;
            double trueCritRate = critRate - dcRate;
            double trueDirRate = dirRate - dcRate;
            double noneRate = 1 - trueCritRate - trueDirRate - dcRate;

            return (int)(noneRate * baseDamage + trueCritRate * critDamage + trueDirRate * dirDamage + dcRate * dcDamage);
        }
    }

    internal static class Extensions
    {
        internal static string VisibleName(this StatConstants.SubstatType type)
        {
            return type switch
            {
                StatConstants.SubstatType.Crit => "Critical Hit",
                StatConstants.SubstatType.Det => "Determination",
                StatConstants.SubstatType.Direct => "Direct Hit Rate",
                StatConstants.SubstatType.SkSpd => "Skill Speed",
                StatConstants.SubstatType.SpSpd => "Spell Speed",
                StatConstants.SubstatType.Ten => "Tenacity",
                StatConstants.SubstatType.Piety => "Piety",
                StatConstants.SubstatType.GCDbase => "GCD(Base)",
                StatConstants.SubstatType.GCDmodified => "GCD +",
                _ => "",
            };
        }
    }
    internal static class StatConstants
    {
        internal enum SubstatType
        {
            Crit,
            Det,
            Direct,
            SkSpd,
            SpSpd,
            Ten,
            Piety,
            GCDbase,
            GCDmodified,
            Defense,
            MagicDefense
        }

        private static int GetMainAtLevel(int level)
        {
            int result = StatTiers.GetStat(level, StatTiers.DataType.Main);
            if (result > 0) return result;
            else return 1;
        }
        private static int GetSubAtLevel(int level)
        {
            int result = StatTiers.GetStat(level, StatTiers.DataType.Sub);
            if (result > 0) return result;
            else return 1;
        }
        internal static int GetDivAtLevel(int level)
        {
            int result = StatTiers.GetStat(level, StatTiers.DataType.Div);
            if (result > 0) return result;
            else return 10;
        }
        internal static int GetBaseStatAtLevel(int level, SubstatType substat)
        {
            if (substat == SubstatType.Det || substat == SubstatType.Piety) return GetMainAtLevel(level);
            else if (substat == SubstatType.Defense || substat == SubstatType.MagicDefense) return 0;
            else return GetSubAtLevel(level);
        }
        internal static int GetCoefficient(SubstatType substat)
        {
            return substat switch
            {
                SubstatType.Crit => 200,
                SubstatType.Det => 140,
                SubstatType.Direct => 550,
                SubstatType.SkSpd => 130,
                SubstatType.SpSpd => 130,
                SubstatType.Ten => 100,
                SubstatType.Piety => 150,
                SubstatType.Defense => 15,
                SubstatType.MagicDefense => 15,
                _ => 1,
            };
        }

    }

    internal static class Formulas
    {
        // Given the stat, return the number of tiers
        internal static int StatFormula(int div, int stat, int baseStat, int coeff)
        {
            // Master formula (stat -> effect): units = floor(coeff * (stat - [main|sub]) / div)
            return (int)Math.Floor(coeff * (stat - baseStat) / (double)div);
        }
        // Given a number of tiers, return the minimum stat required
        internal static int ReverseStatFormula(int div, int units, int baseStat, int coeff)
        {
            // Reverse master formula (effect -> stat): stat = ceil((units * div) / coeff) + [main|sub]
            return (int)(Math.Ceiling((double)(units * div) / coeff) + baseStat);
        }


        // Given units of speed and haste percent, return GCD
        internal static double GCDFormula(int units, int haste)
        {
            // Assumes all actions have a 2500ms base delay.
            // Formula: floor(floor(100 * (100 - haste) / 100) * floor((1000 - spd) * 2.500) / 1000) / 100
            int spdMod = (int)Math.Floor((1000.0 - units) * 2.500);
            double hasteMod = 100.0 - haste; /* multiply by 100 and divide by 100 cancel */
            double result = Math.Floor(hasteMod * spdMod / 1000.0) / 100;
            return result;
        }

        // Given target GCD and haste percent, return units of speed
        internal static int ReverseGCDFormula(double gcd, int haste)
        {
            int gcdMod = (int)Math.Ceiling((gcd + 0.01) * 100);
            int hasteMod = (int)Math.Ceiling(gcdMod / (100.0 - haste) * 1000.0);
            int result = (int)Math.Floor(-(hasteMod / 2.500 - 1000)) + 1;
            return result;
        }

    }

    
}
