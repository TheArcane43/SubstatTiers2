using System;

namespace SubstatTiers
{
    internal static class StatTiers
    {
        internal enum DataType
        {
            Main,
            Sub,
            Div
        }
        internal static int[,] StatTable =
        {
            // main, sub, div (lv is implied by location)
            {20,55,55}, // lv "0"
            {20,56,56},
            {21,57,57},
            {22,60,60},
            {24,62,62},
            {26,65,65},
            {27,68,68},
            {29,70,70},
            {31,73,73},
            {33,76,76},
            {35,78,78},
            {36,82,82},
            {38,85,85},
            {41,89,89},
            {44,93,93},
            {46,96,96},
            {49,100,100},
            {52,104,104},
            {54,109,109},
            {57,113,113},
            {60,116,116},
            {63,122,122},
            {67,127,127},
            {71,133,133},
            {74,138,138},
            {78,144,144},
            {81,150,150},
            {85,155,155},
            {89,162,162},
            {92,168,168},
            {97,173,173},
            {101,181,181},
            {106,188,188},
            {110,194,194},
            {115,202,202},
            {119,209,209},
            {124,215,215},
            {128,223,223},
            {134,229,229},
            {139,236,236},
            {144,244,244},
            {150,253,253},
            {155,263,263},
            {161,272,272},
            {166,283,283},
            {171,292,292},
            {177,302,302},
            {183,311,311},
            {189,322,322},
            {196,331,331},
            {202,341,341}, // lv 50
            {204,342,366},
            {205,344,392},
            {207,345,418},
            {209,346,444},
            {210,347,470},
            {212,349,496},
            {214,350,522},
            {215,351,548},
            {217,352,574},
            {218,354,600}, // lv 60
            {224,355,630},
            {228,356,660},
            {236,357,690},
            {244,358,720},
            {252,359,750},
            {260,360,780},
            {268,361,810},
            {276,362,840},
            {284,363,870},
            {292,364,900}, // lv 70
            {296,365,940},
            {300,366,980},
            {305,367,1020},
            {310,368,1060},
            {315,370,1100},
            {320,372,1140},
            {325,374,1180},
            {330,376,1220},
            {335,378,1260},
            {340,380,1300}, // lv 80
            {345,382,1360},
            {350,384,1420},
            {355,386,1480},
            {360,388,1540},
            {365,390,1600},
            {370,392,1660},
            {375,394,1720},
            {380,396,1780},
            {385,398,1840},
            {390,400,1900} // lv 90
        };

        internal static int GetStat(int lv, DataType type)
        {
            if (lv < 1 || lv > 90) return -1;
            int column = type switch
            {
                DataType.Main => 0,
                DataType.Sub => 1,
                DataType.Div => 2,
                _ => 0,
            };
            return StatTable[lv, column];
        }
    }

    internal class VisibleInfo
    {
        internal string Name { get; set; }
        internal string Stat { get; set; }
        internal string Prev { get; set; }
        internal string Next { get; set; }

        internal VisibleInfo(string name, int s, int p, int n)
        {
            Name = name;
            Stat = $"{s}";
            Prev = $"{p}";
            Next = $"{n:+0}";
        }
        internal VisibleInfo(string name, double f, int p, int n)
        {
            Name = name;
            Stat = $"{f:F2}";
            Prev = $"{p}";
            Next = $"{n:+0}";
        }


    }

    internal class VisibleEffect
    {
        internal string EffectName { get; set; }
        internal string EffectAmount { get; set; }
        internal string EffectTooltip { get; set; }


        internal VisibleEffect(string name, string unitFormat, string tooltip)
        {
            EffectName = name;
            EffectAmount = unitFormat;
            EffectTooltip = tooltip;
        }
    }

    internal class VisibleMateria
    {
        internal string EffectName { get; set; }
        internal string[] EffectTiers { get; set; }
        internal string EffectTooltip { get; set; }

        internal static int[] MateriaTiersAt(int level)
        {
            return level switch
            {
                < 30 => new int[] { 1, 2, 3, 4 }, // lv 15: only +1 materia (I)
                < 45 => new int[] { 2, 4, 6, 8 }, // lv 30: only +2 materia (II)
                < 50 => new int[] { 3, 6, 9, 12 }, // lv 45: only +3 materia (III)
                < 60 => new int[] { 4, 8, 12, 16 }, // lv 50: only +4 materia (IV)
                < 70 => new int[] { 6, 12, 18, 24 }, // lv 60: only +6 materia (V)
                < 80 => new int[] { 6, 12, 16, 32 }, // lv 70: +6 and +16 available (V and VI)
                < 90 => new int[] { 8, 16, 24, 48 }, // lv 80: +8 and +24 available (VII and VIII)
                _ => new int[] { 12, 24, 36, 72 }, // lv 90: +12 and +36 available (IX and X)
            };
        }

        internal static string[] GetTiers(Calculations calc, StatConstants.SubstatType type)
        {
            int level = calc.Data.Level;
            int[] tiers = MateriaTiersAt(level);
            Calculations calcOne = (Calculations)calc.Clone(tiers[0]);
            Calculations calcTwo = (Calculations)calc.Clone(tiers[1]);
            Calculations calcThree = (Calculations)calc.Clone(tiers[2]);
            Calculations calcFour = (Calculations)calc.Clone(tiers[3]);
            int[] bonusTiers = new int[]
            {
                calcOne.GetUnits(type) - calc.GetUnits(type),
                calcTwo.GetUnits(type) - calc.GetUnits(type),
                calcThree.GetUnits(type) - calc.GetUnits(type),
                calcFour.GetUnits(type) - calc.GetUnits(type)
            };
            

            string[] result = new string[4];
            for (int i = 0; i < bonusTiers.Length; i++)
            {
                // Correction for GCD
                if (type == StatConstants.SubstatType.GCDbase || type == StatConstants.SubstatType.GCDmodified)
                {
                    // divide all values by 4 (4 speed tiers = 1 gcd tier)
                    // technically not accurate for modified gcd (tiers vary between 4 and (5 or 6) based on job)
                    bonusTiers[i] = bonusTiers[i] / 4;
                }
                
                result[i] = bonusTiers[i].ToString("+0");
                
            }

            return result;
        }

        internal VisibleMateria(Calculations calc, StatConstants.SubstatType type)
        {
            EffectName = type.VisibleName();
            EffectTiers = GetTiers(calc, type);
            EffectTooltip = type switch
            {
                StatConstants.SubstatType.Crit => "Each tier is +0.1%% Critical Rate and Critical Damage",
                StatConstants.SubstatType.Det => "Each tier is +0.1%% Determination Bonus",
                StatConstants.SubstatType.Direct => "Each tier is +0.1%% Direct Hit Rate",
                StatConstants.SubstatType.SkSpd => "Each tier is +0.1%% DoT Bonus",
                StatConstants.SubstatType.SpSpd => "Each tier is +0.1%% DoT Bonus",
                StatConstants.SubstatType.Ten => "Each tier is +0.1%% Tenacity Bonus",
                StatConstants.SubstatType.Piety => "Each tier is +1 MP Regen per tick",
                StatConstants.SubstatType.GCDbase => "Each tier is -0.01s GCD at base",
                StatConstants.SubstatType.GCDmodified => "Each tier is -0.01s GCD with speed bonuses",
                _ => throw new NotImplementedException()
            };
        }
    }

    internal class VisibleDamage
    {
        internal string DamageName { get; set; }
        internal string DamageNumber { get; set; }

        internal VisibleDamage(string name, int number)
        {
            DamageName = name;
            DamageNumber = number.ToString("N0");
        }
    }

}
