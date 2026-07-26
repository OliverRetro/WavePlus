using System;
using System.Collections.Generic;
using System.Linq;

namespace Plus.HabboHotel.Roleplay.Level
{
    public static class LevelManager
    {
        public static readonly Dictionary<int, int> Levels = new()
    {
        {0, 0},
        {1, 100},
        {2, 250},
        {3, 500},
        {4, 750},
        {5, 1000},
        {6, 1250},
        {7, 1500},
        {8, 1750},
        {9, 2000},
        {10, 2500},
        {11, 2750},
        {12, 3000},
        {13, 3250},
        {14, 3500},
        {15, 3750},
        {16, 4000},
        {17, 4250},
        {18, 4500},
        {19, 4750},
        {20, 5000}
    };

        public static (int Level, int NextLevelXP, int XPRemaining) GetXPInfo(int xp)
        {
            int level = 0;
            int nextLevelXP = Levels[Levels.Keys.Max()];

            foreach (var entry in Levels.OrderBy(l => l.Key)) {
                if (xp >= entry.Value) {
                    level = entry.Key;
                } else {
                    nextLevelXP = entry.Value;
                    break;
                }
            }

            return (level, nextLevelXP, Math.Max(0, nextLevelXP - xp));
        }
    }
}