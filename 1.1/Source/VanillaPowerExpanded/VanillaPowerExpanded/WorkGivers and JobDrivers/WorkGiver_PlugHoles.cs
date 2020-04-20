using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace VanillaPowerExpanded
{
    public class WorkGiver_PlugHoles : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForDef(DefDatabase<ThingDef>.GetNamed("VPE_HelixienGeyser", true));
            }
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_GasGeyser building_geyser = t as Building_GasGeyser;
            bool result;
            if (building_geyser == null || !building_geyser.HoleNeedsPluggingSir)
            {
                result = false;
            }
           
            else
            {
                if (!t.IsForbidden(pawn))
                {
                    LocalTargetInfo target = t;
                    if (pawn.CanReserve(target, 1, -1, null, forced))
                    {
                        result = true;
                        return result;
                    }
                }
                result = false;
            }
            return result;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return new Job(DefDatabase<JobDef>.GetNamed("VPE_JobPlugHole", true), t);
        }
    }
}
