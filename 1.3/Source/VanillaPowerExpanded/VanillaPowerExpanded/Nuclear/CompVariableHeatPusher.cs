using System;
using Verse;
using RimWorld;

namespace VanillaPowerExpanded
{
    public class CompVariableHeatPusher : ThingComp
    {

        protected CompPowerTrader powerComp;

        protected CompRefuelable refuelableComp;

        protected CompBreakdownable breakdownableComp;

        public int HeatPushInterval = 60;

        public float HeatPerSecondVariable;

        public CompProperties_VariableHeatPusher Props
        {
            get
            {
                return (CompProperties_VariableHeatPusher)this.props;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.powerComp = this.parent.GetComp<CompPowerTrader>();
            this.refuelableComp = this.parent.GetComp<CompRefuelable>();
            this.breakdownableComp = this.parent.GetComp<CompBreakdownable>();
            HeatPerSecondVariable = this.Props.heatPerSecond;
        }

        protected virtual bool ShouldPushHeatNow
        {
            get
            {
                if (!this.parent.SpawnedOrAnyParentSpawned)
                {
                    return false;
                }
                CompProperties_VariableHeatPusher props = this.Props;
                float ambientTemperature = this.parent.AmbientTemperature;
                return (ambientTemperature < props.heatPushMaxTemperature && ambientTemperature > props.heatPushMinTemperature) && FlickUtility.WantsToBeOn(this.parent) && 
                    (this.powerComp == null || this.powerComp.PowerOn) && (this.refuelableComp == null || this.refuelableComp.HasFuel) && 
                    (this.breakdownableComp == null || !this.breakdownableComp.BrokenDown); 
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.parent.IsHashIntervalTick(60) && this.ShouldPushHeatNow)
            {
                GenTemperature.PushHeat(this.parent.PositionHeld, this.parent.MapHeld, HeatPerSecondVariable);
            }
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (this.ShouldPushHeatNow)
            {
                GenTemperature.PushHeat(this.parent.PositionHeld, this.parent.MapHeld, HeatPerSecondVariable * 4.16666651f);
            }
        }

       
    }
}
