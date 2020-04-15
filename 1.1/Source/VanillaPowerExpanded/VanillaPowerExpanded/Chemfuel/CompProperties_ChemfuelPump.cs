using System;
using Verse;

namespace VanillaPowerExpanded
{
    public class CompProperties_ChemfuelPump : CompProperties
    {
        public CompProperties_ChemfuelPump()
        {
            this.compClass = typeof(CompChemfuelPump);
        }

        public int fuelProduced = 1;

        public float fuelInterval = 1f;

      
    }
}