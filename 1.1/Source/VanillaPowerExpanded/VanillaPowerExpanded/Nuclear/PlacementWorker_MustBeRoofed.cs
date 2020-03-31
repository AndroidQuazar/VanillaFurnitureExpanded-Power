using System;
using Verse;
using RimWorld;

namespace VanillaPowerExpanded
{
    public class PlaceWorker_MustBeRoofed : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {

            Room room = loc.GetRoom(map,RegionType.Set_All);
            if (room != null)
            {
                if (room.OutdoorsForWork || (!map.roofGrid.Roofed(loc))) {
                    return new AcceptanceReport("VPE_MustPlaceRoofed".Translate());
                }
            }



           
            return true;
        }
    }
}