using System;

namespace SubstatTiers
{
    public class Calculations : ICloneable
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

        internal StatConstants.SubstatType SpeedType = 0;
        internal int HasteAmount = 0;

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

        internal Calculations()
        {
            // Use default values
        }

        internal Calculations(AttributeData a)
        {
            Level = a.Level;
            CritHit = a.CriticalHit;
            Determination = a.Determination;
            DirectHit = a.DirectHit;
            SkillSpeed = a.SkillSpeed;
            SpellSpeed = a.SpellSpeed;
            Tenacity = a.Tenacity;
            Piety = a.Piety;

            SpeedType = a.UsesAttackPower() ? StatConstants.SubstatType.SkSpd : StatConstants.SubstatType.SpSpd;
            Speed = a.UsesAttackPower() ? SkillSpeed : SpellSpeed;
            HasteAmount = a.HasteAmount();

        }

        public object Clone()
        {
            return Clone(0);
        }

        public object Clone(int modifier)
        {
            Calculations c = new();
            c.Level = this.Level;
            c.CritHit = this.CritHit + modifier;
            c.Determination = this.Determination + modifier;
            c.DirectHit = this.DirectHit + modifier;
            c.SkillSpeed = this.SkillSpeed + modifier;
            c.SpellSpeed = this.SpellSpeed + modifier;
            c.Tenacity = this.Tenacity + modifier;
            c.Piety = this.Piety + modifier;
            c.Speed = this.Speed + modifier;
            c.SpeedType = this.SpeedType;
            c.HasteAmount = this.HasteAmount;
            return c;
        }

        internal double GetGCDbase() => Formulas.GCDFormula(GetUnits(SpeedType), 0);
        internal double GetGCDmodified() => Formulas.GCDFormula(GetUnits(SpeedType), HasteAmount);
        internal int GetSpeedUnitsOfGCDbase() => Formulas.ReverseGCDFormula(GetGCDbase(), 0);
        internal int GetSpeedUnitsOfGCDmodified() => Formulas.ReverseGCDFormula(GetGCDmodified(), HasteAmount);
        internal int GetSpeedUnitsOfNextGCDbase() => Formulas.ReverseGCDFormula(GetGCDbase() - 0.010000001, 0);
        internal int GetSpeedUnitsOfNextGCDmodified() => Formulas.ReverseGCDFormula(GetGCDmodified() - 0.010000001, HasteAmount);
        // NOTE: due to rounding and/or float precision, the 0.010000001 is necessary to actually move to the next GCD
        //   don't know which one it is, but it doesn't seem to break anything else

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

    internal class Formulas
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
