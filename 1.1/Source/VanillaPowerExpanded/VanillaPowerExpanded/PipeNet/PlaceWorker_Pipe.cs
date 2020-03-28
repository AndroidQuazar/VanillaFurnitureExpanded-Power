using System;
using System.Collections.Generic;
using Verse;

namespace VanillaPowerExpanded
{
    public class PlaceWorker_Pipe : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            List<Thing> thingList = loc.GetThingList(map);
            for (int i = 0; i < thingList.Count; i++)
            {

                if (thingList[i].def.HasComp(typeof(CompPipe))|| thingList[i].def.HasComp(typeof(CompPipeTank)) || thingList[i].def.HasComp(typeof(CompPipeTrader)) || thingList[i].def.HasComp(typeof(CompPipeTransmitter)) || thingList[i].def.HasComp(typeof(CompPipePlant)))
                {
                    return false;
                }
                if (thingList[i].def.EverTransmitsPower)
                {
                    return false;
                }
                if (thingList[i].def.entityDefToBuild != null)
                {
                    ThingDef thingDef = thingList[i].def.entityDefToBuild as ThingDef;
                    if (thingDef != null && thingDef.EverTransmitsPower)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
