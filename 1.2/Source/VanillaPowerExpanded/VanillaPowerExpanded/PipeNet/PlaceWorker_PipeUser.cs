using System;
using System.Collections.Generic;
using Verse;

namespace VanillaPowerExpanded
{
    public class PlaceWorker_PipeUser : PlaceWorker
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
                        return "VPE_MustPlaceAdjacentToPipe".Translate();
                    }

                }


            }
            return true;

           
        }
    }
}
