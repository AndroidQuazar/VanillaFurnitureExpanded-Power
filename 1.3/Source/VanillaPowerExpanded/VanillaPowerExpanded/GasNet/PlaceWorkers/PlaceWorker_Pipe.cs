// PlaceWorker_Pipe.cs
// Copyright Karel Kroeze, 2020-2020

using Verse;
using System.Linq;

namespace GasNetwork
{
    public class PlaceWorker_Pipe: PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing( BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null,
                                                        Thing thing = null )
        {
            // don't allow building pipes on top of piped buildings (blueprints)
            return !loc.GetThingList( map ).Where(t => t != thingToIgnore).Any( t => t.def.BuildingFrameOrBlueprintEverTransmitsGas() );
        }
    }
}