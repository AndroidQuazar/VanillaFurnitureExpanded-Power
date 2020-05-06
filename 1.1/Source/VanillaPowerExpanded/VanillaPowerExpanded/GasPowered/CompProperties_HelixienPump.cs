using System;
using Verse;

namespace VanillaPowerExpanded
{
    public class CompProperties_HelixienPump : CompProperties
    {
        public CompProperties_HelixienPump()
        {
            this.compClass = typeof(CompHelixienPump);
        }

        public int fuelProduced = 1;

        public float fuelInterval = 1f;

      
    }
}