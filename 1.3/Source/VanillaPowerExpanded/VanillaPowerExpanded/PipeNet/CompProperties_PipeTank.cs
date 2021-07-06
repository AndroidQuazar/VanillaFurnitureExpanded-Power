using System;
using RimWorld;

namespace VanillaPowerExpanded
{
    public class CompProperties_PipeTank : CompProperties_Pipe
    {
        public float storedEnergyMax = 1000f;

        public float efficiency = 0.5f;

        public CompProperties_PipeTank()
        {
            this.compClass = typeof(CompPipeTank);
        }
    }
}
