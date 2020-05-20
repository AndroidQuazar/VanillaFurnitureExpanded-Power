using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace VanillaPowerExpanded
{
    public class PlaceWorker_PipeUserOnGasGeyser : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            
            Thing thing2 = map.thingGrid.ThingAt(loc, ThingDef.Named("VPE_HelixienGeyser"));
            if (thing2 == null || thing2.Position != loc)
            {
                return "VPE_MustPlaceOnGasGeyser".Translate();
            }
          
            return true;

           
        }
    }
}
