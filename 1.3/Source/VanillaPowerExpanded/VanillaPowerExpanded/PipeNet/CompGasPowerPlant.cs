using System;
using RimWorld;

namespace VanillaPowerExpanded
{
    public class CompGasPowerPlant : CompPowerTrader
    {
        protected virtual float DesiredPowerOutput
        {
            get
            {
                return -base.Props.basePowerConsumption;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.breakdownableComp = this.parent.GetComp<CompBreakdownable>();
            this.pipeComp = this.parent.GetComp<CompPipeTrader>();

            if (base.Props.basePowerConsumption < 0f && !this.parent.IsBrokenDown() && FlickUtility.WantsToBeOn(this.parent))
            {
                base.PowerOn = true;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            this.UpdateDesiredPowerOutput();
        }

        public void UpdateDesiredPowerOutput()
        {
            if ((this.breakdownableComp != null && this.breakdownableComp.BrokenDown) || !this.pipeComp.PowerOn || (this.flickableComp != null && !this.flickableComp.SwitchIsOn) || !base.PowerOn)
            {
                base.PowerOutput = 0f;
                return;
            }
            base.PowerOutput = this.DesiredPowerOutput;
        }

     

        protected CompBreakdownable breakdownableComp;
        protected CompPipeTrader pipeComp;

    }
}