using System;
using Dalamud.Data;
using Lumina.Excel.GeneratedSheets;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace SubstatTiers
{
    public class Calculations : ICloneable
    {

        internal AttributeData Data { get; set; }

        internal int Speed { get; set; } = 400;

        internal int GetUnits(StatConstants.SubstatType substat)
        {
            if (substat == StatConstants.SubstatType.GCDbase)
            {
                return GetSpeedUnitsOfGCDbase();
            }
            if (substat == StatConstants.SubstatType.GCDmodified)
            {
                return GetSpeedUnitsOfGCDmodified();
            }
            int stat = substat switch
            {
                StatConstants.SubstatType.Crit => Data.CriticalHit,
                StatConstants.SubstatType.Det => Data.Determination,
                StatConstants.SubstatType.Direct => Data.DirectHit,
                StatConstants.SubstatType.SkSpd => Data.SkillSpeed,
                StatConstants.SubstatType.SpSpd => Data.SpellSpeed,
                StatConstants.SubstatType.Ten => Data.Tenacity,
                StatConstants.SubstatType.Piety => Data.Piety,
                _ => 1,
            };
            int coeff = substat switch
            {
                StatConstants.SubstatType.Crit => StatConstants.CritCoeff,
                StatConstants.SubstatType.Det => StatConstants.DeterminationCoeff,
                StatConstants.SubstatType.Direct => StatConstants.DirectCoeff,
                StatConstants.SubstatType.SkSpd => StatConstants.SkillCoeff,
                StatConstants.SubstatType.SpSpd => StatConstants.SpellCoeff,
                StatConstants.SubstatType.Ten => StatConstants.TenacityCoeff,
                StatConstants.SubstatType.Piety => StatConstants.PietyCoeff,
                _ => 1,
            };

            return Formulas.StatFormula(StatConstants.GetDivAtLevel(Data.Level), stat, StatConstants.GetBaseStatAtLevel(Data.Level, substat), coeff);
        }

        internal int GetStatsFromUnits(StatConstants.SubstatType substat, int units)
        {
            int coeff = substat switch
            {
                StatConstants.SubstatType.Crit => StatConstants.CritCoeff,
                StatConstants.SubstatType.Det => StatConstants.DeterminationCoeff,
                StatConstants.SubstatType.Direct => StatConstants.DirectCoeff,
                StatConstants.SubstatType.SkSpd => StatConstants.SkillCoeff,
                StatConstants.SubstatType.SpSpd => StatConstants.SpellCoeff,
                StatConstants.SubstatType.Ten => StatConstants.TenacityCoeff,
                StatConstants.SubstatType.Piety => StatConstants.PietyCoeff,
                _ => 1,
            };

            return Formulas.ReverseStatFormula(StatConstants.GetDivAtLevel(Data.Level), units, StatConstants.GetBaseStatAtLevel(Data.Level, substat), coeff);
        }

        internal Calculations(AttributeData a)
        {
            Data = a;
        }

        public object Clone()
        {
            return Clone(0);
        }

        public object Clone(int modifier)
        {

            AttributeData a = new()
            {
                Level = Data.Level,
                CriticalHit = Data.CriticalHit + modifier,
                Determination = Data.Determination + modifier,
                DirectHit = Data.DirectHit + modifier,
                SkillSpeed = Data.SkillSpeed + modifier,
                SpellSpeed = Data.SpellSpeed + modifier,
                Tenacity = Data.Tenacity + modifier,
                Piety = Data.Piety + modifier
            };

            Calculations c = new(a);

            return c;
        }

        internal double GetGCDbase() => Formulas.GCDFormula(GetUnits(Data.SpeedType), 0);
        internal double GetGCDmodified() => Formulas.GCDFormula(GetUnits(Data.SpeedType), Data.HasteAmount());
        internal int GetSpeedUnitsOfGCDbase() => Formulas.ReverseGCDFormula(GetGCDbase(), 0);
        internal int GetSpeedUnitsOfGCDmodified() => Formulas.ReverseGCDFormula(GetGCDmodified(), Data.HasteAmount());
        internal int GetSpeedUnitsOfNextGCDbase() => Formulas.ReverseGCDFormula(GetGCDbase() - 0.010000001, 0);
        internal int GetSpeedUnitsOfNextGCDmodified() => Formulas.ReverseGCDFormula(GetGCDmodified() - 0.010000001, Data.HasteAmount());
        // NOTE: due to rounding and/or float precision, the 0.010000001 is necessary to actually move to the next GCD
        //   don't know which one it is, but it doesn't seem to break anything else

        internal int FunctionWD()
        {
            int job = Data.AttackMod();
            int main = StatTiers.GetStat(Data.Level, StatTiers.DataType.Main);
            int weapon = Data.WeaponPower;

            return (int)(Math.Floor(main * job / 1000.0) + weapon);
        }
        private static int AttackConstant(bool isTank, int level)
        {
            int c = level switch
            {
                <= 50 => 75,
                <= 70 => (int)Math.Floor(2.5 * (level - 50)) + 75,
                <= 80 => 4 * (level - 70) + 125,
                <= 90 => 3 * (level - 80) + 165,
                _ => 195,
            };
            if (isTank)
            {
                c *= 1; // Tank correction not confirmed yet, believed to be 0.7x
            }
            return c;
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
            int coeff = StatConstants.DeterminationCoeff;

            return (int)(Math.Floor(coeff * (det - main) / (double)div) + 1000);
        }

        // Returns 1000 + tiers of tenacity
        private int FunctionTEN()
        {
            int ten = Data.Tenacity;
            int sub = StatTiers.GetStat(Data.Level, StatTiers.DataType.Sub);
            int div = StatTiers.GetStat(Data.Level, StatTiers.DataType.Div);
            int coeff = StatConstants.TenacityCoeff;

            return (int)(Math.Floor(coeff * (ten - sub) / (double)div) + 1000);
        }

        // Returns 1000 + tiers of speed
        private int FunctionSPD()
        {
            int spd = Data.UsesAttackPower() ? Data.SkillSpeed : Data.SpellSpeed;
            int sub = StatTiers.GetStat(Data.Level, StatTiers.DataType.Sub);
            int div = StatTiers.GetStat(Data.Level, StatTiers.DataType.Div);
            int coeff = Data.UsesAttackPower() ? StatConstants.SkillCoeff : StatConstants.SpellCoeff;

            return (int)(Math.Floor(coeff * (spd - sub) / (double)div) + 1000);
        }

        // Returns 1400 + tiers of critical hit (base crit damage is 40.0%)
        private int FunctionCRIT()
        {
            int crit = Data.CriticalHit;
            int sub = StatTiers.GetStat(Data.Level, StatTiers.DataType.Sub);
            int div = StatTiers.GetStat(Data.Level, StatTiers.DataType.Div);
            int coeff = StatConstants.CritCoeff;

            return (int)(Math.Floor(coeff * (crit - sub) / (double)div) + 1400);
        }

        internal int DamageFormula(bool isCrit, bool isDirect)
        {
            int pot = 100;
            int atk = FunctionAP();
            int det = FunctionDET();
            int tnc = Data.IsTank() ? FunctionTEN() : 1000;
            int wep = FunctionWD();
            int tr = (int)Data.TraitAmount() * 100;
            int crit = isCrit ? FunctionCRIT() : 1000;
            int dh = isDirect ? 125 : 100;

            // Formula (floor after every step): [potency * atk * det] / 100 / 1000 * tnc / 1000 * wep / 100 * trait / 100 * crit / 1000 * dh / 100
            int baseCalc = (int)Math.Floor(Math.Floor(Math.Floor(Math.Floor(pot * atk * det / 100000.0) * tnc / 1000.0) * wep / 100.0) * tr / 100.0);
            int procCalc = (int)Math.Floor(Math.Floor(baseCalc * crit / 1000.0) * dh / 100.0);
            return procCalc;
        }

        internal int DamageAverage()
        {
            int baseDamage = DamageFormula(false, false);
            int critDamage = DamageFormula(true, false);
            int dirDamage  = DamageFormula(false, true);
            int dcDamage = DamageFormula(true, true);

            double critRate = GetUnits(StatConstants.SubstatType.Crit) / 1000 + 5.0;
            double dirRate = GetUnits(StatConstants.SubstatType.Direct) / 1000;
            double dcRate = critRate * dirRate;
            double trueCritRate = critRate - dcRate;
            double trueDirRate = dirRate - dcRate;
            double noneRate = 1 - trueCritRate - trueDirRate - dcRate;
            
            return (int)(noneRate * baseDamage + trueCritRate * critDamage + trueDirRate * dirDamage + dcRate * dcDamage);
        }

        internal int DamageOverTimeFormula(bool isCrit, bool isDirect)
        {
            int pot = 100;
            int atk = FunctionAP();
            int det = FunctionDET();
            int tnc = Data.IsTank() ? FunctionTEN() : 1000;
            int wep = FunctionWD();
            int tr = (int)Data.TraitAmount() * 100;
            int crit = isCrit ? FunctionCRIT() : 1000;
            int dh = isDirect ? 125 : 100;
            int spd = FunctionSPD();

            // Formula (floor after every step): [see calculations below]
            int baseCalc = (int)Math.Floor(Math.Floor(Math.Floor(pot * atk * det / 100000.0) * tnc / 1000.0) * wep / 100.0);
            int speedCalc = (int)Math.Floor(Math.Floor(Math.Floor(baseCalc * spd / 1000.0) * tr / 100.0) + 1);
            int randCalc = (int)Math.Floor(Math.Floor(Math.Floor(speedCalc * crit / 1000.0) * dh / 100.0));
            return randCalc;
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
            GCDmodified
        }


        internal const int MaxLevel = 90;

        internal const int CritCoeff = 200;
        internal const int DeterminationCoeff = 140;
        internal const int DirectCoeff = 550;
        internal const int SkillCoeff = 130;
        internal const int SpellCoeff = 130;
        internal const int TenacityCoeff = 100;
        internal const int PietyCoeff = 150;

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
            else return GetSubAtLevel(level);
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
