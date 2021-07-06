// CompGasHeatPusher.cs
// Copyright Karel Kroeze, 2020-2020

using Verse;

namespace GasNetwork
{
    public class CompGasHeatPusher: CompHeatPusherPowered
    {
        protected CompGasTrader gasComp;

        public override void PostSpawnSetup( bool respawningAfterLoad )
        {
            base.PostSpawnSetup( respawningAfterLoad );

            gasComp = parent.GetComp<CompGasTrader>();
        }

        protected override bool ShouldPushHeatNow => base.ShouldPushHeatNow && ( gasComp == null || gasComp.GasOn );
    }
}