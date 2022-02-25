using System;

namespace SubstatTiers
{
    public class Calculations
    {
        internal int Level { get; set; } = 90;
        internal int CritHit { get; set; } = 400;
        internal int Determination { get; set; } = 390;
        internal int DirectHit { get; set; } = 400;

        internal int SkillSpeed { get; set; } = 400;
        internal int SpellSpeed { get; set; } = 400;

        internal int Speed { get; set; } = 400;

        internal int Tenacity { get; set; } = 400;
        internal int Piety { get; set; } = 390;

        internal int GetUnits(StatConstants.SubstatType substat)
        {
            int stat = substat switch
            {
                StatConstants.SubstatType.Crit => CritHit,
                StatConstants.SubstatType.Det => Determination,
                StatConstants.SubstatType.Direct => DirectHit,
                StatConstants.SubstatType.SkSpd => SkillSpeed,
                StatConstants.SubstatType.SpSpd => SpellSpeed,
                StatConstants.SubstatType.Ten => Tenacity,
                StatConstants.SubstatType.Piety => Piety,
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

            return Formulas.StatFormula(StatConstants.GetDivAtLevel(this.Level), stat, StatConstants.GetBaseStatAtLevel(this.Level, substat), coeff);
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

            return Formulas.ReverseStatFormula(StatConstants.GetDivAtLevel(this.Level), units, StatConstants.GetBaseStatAtLevel(this.Level, substat), coeff);
        }

        public override string ToString()
        {
            string lv = $"Player Level: {this.Level}\n";
            string crit = $"Crit Hit Rate: {this.GetUnits(StatConstants.SubstatType.Crit) * 0.1 + 5:F1}%%\nCrit Damage: {this.GetUnits(StatConstants.SubstatType.Crit) * 0.1 + 140:F1}%%\n";
            string det = $"Determination Bonus: {this.GetUnits(StatConstants.SubstatType.Det) * 0.1:F1}%%\n";
            string dh = $"Direct Hit Rate: {this.GetUnits(StatConstants.SubstatType.Direct) * 0.1:F1}%%\n";
            string sk = $"Physical GCD: {Formulas.GCDFormula(this.GetUnits(StatConstants.SubstatType.SkSpd), 0):F2}\n";
            string sp = $"Magical GCD: {Formulas.GCDFormula(this.GetUnits(StatConstants.SubstatType.SpSpd), 0):F2}\n";
            string ten = $"Tenacity Bonus: {this.GetUnits(StatConstants.SubstatType.Ten) * 0.1:F1}%%\n";
            string pie = $"MP Recovery: {this.GetUnits(StatConstants.SubstatType.Piety) + 200} MP\n";
            return string.Concat(lv, crit, det, dh, sk, sp, ten, pie);
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
            Piety
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

    internal class Formulas
    {
        // Given the stat, return the number of tiers
        internal static int StatFormula(int div, int stat, int baseStat, int coeff)
        {
            // Master formula (stat -> effect): units = floor(coeff * (stat - [main|sub]) / div)
            return (int)Math.Floor((double)(coeff * (stat - baseStat)) / (double)div);
        }
        // Given a number of tiers, return the minimum stat required
        internal static int ReverseStatFormula(int div, int units, int baseStat, int coeff)
        {
            // Reverse master formula (effect -> stat): stat = ceil((units * div) / coeff) + [main|sub]
            return (int)(Math.Ceiling((double)(units * div) / coeff) + baseStat);
        }

        private static int WeaponDamageFormula(int main, int job, int weapon)
        {
            return (int)(Math.Floor(main * job / 1000.0) + weapon);
        }

        private static int AttackPower(int att, int main)
        {
            double someConstant = 165.0;
            return (int)(Math.Floor(someConstant * (att - main) / main) + 100);
        }

        private static int AutoPower(int main, int job, int weapon, double delay)
        {
            return (int)(Math.Floor(Math.Floor(main * job / 1000.0) + weapon) * delay / 3.00);
        }

        private static int DamageFormula(int pot, int atk, int det, int tnc, int wep, int tr, int crit, int dh, double buffs)
        {
            // Formula (floor after every step): [potency * atk * det] / 100 / 1000 * tnc / 1000 * wep / 100 * trait / 100 * crit / 1000 * dh / 100 * rand[95,105] / 100 * buffs
            //int result = 0;
            int baseCalc = (int)Math.Floor(Math.Floor(Math.Floor(Math.Floor(pot * atk * det / 100000.0) * tnc / 1000.0) * wep / 100.0) * tr / 100.0);
            int procCalc = (int)Math.Floor(Math.Floor(baseCalc * crit / 1000.0) * dh / 100.0);
            int randCalc = (int)Math.Floor(Math.Floor(procCalc * 100.0/*random number*/ / 100.0) * buffs);
            return randCalc;
        }

        private static int DamageOverTimeFormula(int pot, int atk, int det, int tnc, int wep, int tr, int crit, int dh, int buffs, int spd)
        {
            // Formula (floor after every step): ([potency * atk * det] / 100 / 1000 * tnc / 1000 * wep / 100 * spd / 1000 * trait / 100 + 1) * rand[95,105] ... [see damage formula]
            int baseCalc = (int)Math.Floor(Math.Floor(Math.Floor(pot * atk * det / 100000.0) * tnc / 1000.0) * wep / 100.0);
            int speedCalc = (int)Math.Floor(Math.Floor(Math.Floor(baseCalc * spd / 1000.0) * tr / 100.0) + 1);
            int randCalc = (int)Math.Floor(Math.Floor(Math.Floor(Math.Floor(speedCalc * 100.0/*random*/ / 100.0) * crit / 1000.0) * dh / 100.0) * buffs);
            return randCalc;

        }

        private static int AutoAttackFormula(int pot, int atk, int det, int tnc, int autopow, int tr, int crit, int dh, double buffs, int spd)
        {
            // Formula (floor after every step): [same as damage formula but with (autopow * spd / 1000) instead of (wep)
            // note: bard and machinist have pot=100, all others have pot=110
            int baseCalc = (int)Math.Floor(Math.Floor(Math.Floor(Math.Floor(pot * atk * det / 100000.0) * tnc / 1000.0) * autopow * spd / 1000.0) * tr / 100.0);
            int procCalc = (int)Math.Floor(Math.Floor(baseCalc * crit / 1000.0) * dh / 100.0);
            int randCalc = (int)Math.Floor(Math.Floor(procCalc * 100.0/*random*/ / 100.0) * buffs);
            return randCalc;
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

        // Given target GCD and haste percent, return tiers required
        internal static int ReverseGCDFormula(double gcd, int haste)
        {
            int gcdMod = (int)Math.Ceiling((gcd + 0.01) * 100);
            int hasteMod = (int)Math.Ceiling(gcdMod / (100.0 - haste) * 1000.0);
            int result = (int)Math.Floor(-(hasteMod / 2.500 - 1000)) + 1;
            return result;
        }

    }

    
}
