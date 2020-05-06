using System;
using System.Linq;
using Verse;
using RimWorld;

namespace VanillaPowerExpanded
{
    public class IncidentWorker_GasExplosion : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return GasExplosionUtility.GetExplodableGasConduits((Map)parms.target).Any<Building>();
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Building culprit;
            if (!GasExplosionUtility.GetExplodableGasConduits((Map)parms.target).TryRandomElement(out culprit))
            {
                return false;
            }
            GasExplosionUtility.DoExplosion(culprit);
            return true;
        }
    }
}