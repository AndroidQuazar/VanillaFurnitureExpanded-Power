using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace GasNetwork
{
    public class WorkGiver_GasExcavator : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForDef(ThingDef.Named("VPE_GasDrill"));
            }
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.InteractionCell;
            }
        }

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            List<Building> allBuildingsColonist = pawn.Map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < allBuildingsColonist.Count; i++)
            {
                Building building = allBuildingsColonist[i];
                if (building.def == ThingDef.Named("VPE_GasDrill"))
                {
                    CompGasTrader comp = building.GetComp<CompGasTrader>();
                    if ((comp == null || comp.GasOn) && building.Map.designationManager.DesignationOn(building, DesignationDefOf.Uninstall) == null)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t.Faction != pawn.Faction)
            {
                return false;
            }
            Building building = t as Building;
            return building != null && !building.IsForbidden(pawn) && pawn.CanReserve(building, 1, -1, null, forced) && building.TryGetComp<CompDeepDrill>().CanDrillNow() && building.Map.designationManager.DesignationOn(building, DesignationDefOf.Uninstall) == null && !building.IsBurning();
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(JobDefOf.OperateDeepDrill, t, 1500, true);
        }
    }
}