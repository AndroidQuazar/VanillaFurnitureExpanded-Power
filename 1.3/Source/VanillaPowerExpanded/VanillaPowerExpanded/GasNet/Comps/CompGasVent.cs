// Copyright Karel Kroeze, 2020-2020.
// VanillaPowerExpanded/VanillaPowerExpanded/CompGasVent.cs

using System.Collections.Generic;
using RimWorld;
using Verse;

namespace GasNetwork
{
    public class CompGasVent : CompGasTrader
    {
        protected CompBreakdownable breakdownable;
        protected CompFlickable     flickable;
        protected CompSchedule      schedule;
        protected IntVec3           ventPos;

        public new CompProperties_GasVent Props => props as CompProperties_GasVent;

        protected virtual bool ShouldVentNow => GasOn
                                             && parent.SpawnedOrAnyParentSpawned
                                             && (breakdownable == null || !breakdownable.BrokenDown)
                                             && (flickable     == null || flickable.SwitchIsOn)
                                             && (schedule      == null || schedule.Allowed);

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            flickable     = parent.GetComp<CompFlickable>();
            breakdownable = parent.GetComp<CompBreakdownable>();
            schedule      = parent.GetComp<CompSchedule>();

            // set flickable to false, to avoid immediately flooding the area with 
            // deadly toxins.

            if (!respawningAfterLoad && flickable != null)
            {
                flickable.SwitchIsOn = false;
            }

            // get ventPos
            ventPos = GenGas.VentingPosition(parent);
        }

        public override void CompTick()
        {
            base.CompTick();
            // ReSharper disable once InvertIf I LIKE MY NESTED IFS!
            if (ShouldVentNow)
            {
                Notify_UsedThisTick();
                if (parent.IsHashIntervalTick(30))
                {
                    GenGas.AddGas(ventPos, parent.Map, Props.ventingGas, Props.gasConsumptionWhenUsed);
                }
            }
        }
    }

    public class CompProperties_GasVent : CompProperties_GasTrader
    {
        public ThingDef ventingGas;
        public float    ventingRadius = GenGas.DEFAULT_GAS_RADIUS;

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var configError in base.ConfigErrors(parentDef))
            {
                yield return configError;
            }

            if (ventingGas == null)
            {
                yield return "CompGasVent should define a venting gas";
            }
            else if (!typeof(Gas).IsAssignableFrom(ventingGas.thingClass))
            {
                yield return
                    "venting gas should have a thingClass derived from RimWorld.Gas (or GasNetwork.Gas_Spreading).";
            }
        }
    }
}