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
            List<Thing> thingList;
            foreach (IntVec3 c in GenAdj.OccupiedRect(loc, rot, checkingDef.Size))
            {

                thingList = c.GetThingList(map);
                for (int i = 0; i < thingList.Count; i++)
                {

                    if (thingList[i].def.HasComp(typeof(CompPipe)) || thingList[i].def.HasComp(typeof(CompPipeTank)) || thingList[i].def.HasComp(typeof(CompPipeTrader)) || thingList[i].def.HasComp(typeof(CompPipeTransmitter)) || thingList[i].def.HasComp(typeof(CompPipePlant)))
                    {
                        return false;
                    }

                }


            }
            Thing thing2 = map.thingGrid.ThingAt(loc, ThingDef.Named("VPE_HelixienGeyser"));
            if (thing2 == null || thing2.Position != loc)
            {
                return "VPE_MustPlaceOnGasGeyser".Translate();
            }
          
            return true;

           
        }
    }
}
