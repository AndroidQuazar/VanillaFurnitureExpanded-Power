using System;
using Verse;

namespace VanillaPowerExpanded
{
    public class CompProperties_VariableHeatPusher : CompProperties
    {
        public CompProperties_VariableHeatPusher()
        {
            this.compClass = typeof(CompVariableHeatPusher);
        }

        public float heatPerSecond;

        public float heatPushMaxTemperature = 99999f;

        public float heatPushMinTemperature = -99999f;
    }
}