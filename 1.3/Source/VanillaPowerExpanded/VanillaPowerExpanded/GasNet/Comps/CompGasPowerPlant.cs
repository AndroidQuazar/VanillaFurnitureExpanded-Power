// CompGasPowerPlant.cs
// Copyright Karel Kroeze, 2020-2020

using RimWorld;

namespace GasNetwork
{
    public class CompGasPowerPlant : CompPowerPlant
    {
        private CompGasTrader gasComp;

        public override void PostSpawnSetup( bool respawningAfterLoad )
        {
            base.PostSpawnSetup( respawningAfterLoad );
            gasComp = parent.GetComp<CompGasTrader>();
        }

        public override void CompTick()
        {
            if ( !PowerOn
              || ( breakdownableComp?.BrokenDown ?? false )
              || ( !gasComp?.GasOn               ?? false )
              || ( !flickableComp?.SwitchIsOn    ?? false ) )
                PowerOutput = 0f;
            else
                PowerOutput = DesiredPowerOutput;
        }
    }
}