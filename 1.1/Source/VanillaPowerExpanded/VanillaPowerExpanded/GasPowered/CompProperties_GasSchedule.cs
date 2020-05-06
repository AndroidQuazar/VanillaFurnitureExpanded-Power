using System;
using Verse;
using RimWorld;

namespace VanillaPowerExpanded
{
    public class CompProperties_GasSchedule : CompProperties
    {
        public CompProperties_GasSchedule()
        {
            this.compClass = typeof(CompGasSchedule);
        }

        public float startTime;

        public float endTime = 1f;

        public string offMessage;
    }
}
