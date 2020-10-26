﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace Whydoisuck.ViewModels.DataStructures
{
    public class LevelPartStatistics
    {
        public Range PercentRange { get; set; }
        public int ReachCount { get; set; }
        public int DeathCount { get; set; }
        public float PassRate
        {
            get
            {
                if (ReachCount == 0) return 0;
                if (PercentRange.Start >= 100) return 100;
                return 100 * (1 - (float)DeathCount / (float)ReachCount);
            }
        }

        public LevelPartStatistics(Range range, int reachCount, int deathCount)
        {
            PercentRange = range;
            ReachCount = reachCount;
            DeathCount = deathCount;
        }
    }
}