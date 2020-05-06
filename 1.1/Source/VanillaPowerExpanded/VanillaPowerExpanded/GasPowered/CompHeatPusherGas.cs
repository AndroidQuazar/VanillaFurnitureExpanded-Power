using System;
using RimWorld;
using Verse;

namespace VanillaPowerExpanded
{
    public class CompHeatPusherGas : CompHeatPusher
    {
        protected override bool ShouldPushHeatNow
        {
            get
            {
                return base.ShouldPushHeatNow && FlickUtility.WantsToBeOn(this.parent)  && (this.refuelableComp == null || this.refuelableComp.HasFuel) && (this.breakdownableComp == null || !this.breakdownableComp.BrokenDown);
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
           
            this.refuelableComp = this.parent.GetComp<CompRefuelable>();
            this.breakdownableComp = this.parent.GetComp<CompBreakdownable>();
        }

       

        protected CompRefuelable refuelableComp;

        protected CompBreakdownable breakdownableComp;
    }
}
