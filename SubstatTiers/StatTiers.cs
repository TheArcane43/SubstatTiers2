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
            {218,354,600},
            {224,355,630},
            {228,356,660},
            {236,357,690},
            {244,358,720},
            {252,359,750},
            {260,360,780},
            {268,361,810},
            {276,362,840},
            {284,363,870},
            {292,364,900},
            {296,365,940},
            {300,366,980},
            {305,367,1020},
            {310,368,1060},
            {315,370,1100},
            {320,372,1140},
            {325,374,1180},
            {330,376,1220},
            {335,378,1260},
            {340,380,1300},
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
        internal VisibleInfo(string name, double f)
        {
            Name = name;
            Stat = $"{f:F2}";
            Prev = "";
            Next = "";
        }

    }

    internal class VisibleEffect
    {
        internal string EffectName { get; set; }
        internal string EffectAmount { get; set; }

        internal VisibleEffect(string name, string unitFormat)
        {
            EffectName = name;
            EffectAmount = unitFormat;
        }
    }
}
