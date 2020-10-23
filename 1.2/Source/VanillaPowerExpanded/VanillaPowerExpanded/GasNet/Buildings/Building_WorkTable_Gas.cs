// Building_GasWorkTable.cs
// Copyright Karel Kroeze, -2020

using RimWorld;
using Verse;

namespace GasNetwork
{
    public class Building_WorkTable_Gas : Building_WorkTable_HeatPush, IBillGiver, IBillGiverWithTickAction
    {
        public CompGasTrader gasComp;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            gasComp = GetComp<CompGasTrader>();
        }

        public bool CanWorkWithoutGas => gasComp == null || def.building.unpoweredWorkTableWorkSpeedFactor > 0f;

        // note; the below 'Usable' functions are IBillGiver interface implementations,
        // and not defined as virtual in Building_WorkTable. As long as the instance they
        // are called on is cast to IBillGiver they should work, but when cast to 
        // Building_WorkTable they will not be used. In vanilla, they are always cast to 
        // IBillGiver when used. 
        // The alternative is to patch Building_WorkTable with a few Postfixes. 
        public new bool CurrentlyUsableForBills()
        {
            // todo; I'm afraid this might cause 'flickering'.
            // Consider a case where no gas is available.
            // When not used, consumption is zero - table will be turned on. 
            // When used, consumption is x, table can be turned off.
            return base.CurrentlyUsableForBills() && (CanWorkWithoutGas || gasComp != null && gasComp.GasOn);
        }

        public new bool UsableForBillsAfterFueling()
        {
            return base.UsableForBillsAfterFueling() && (CanWorkWithoutGas || (gasComp?.GasOn ?? false));
        }

        // note; same as above, but for UsedThisTick and IBillGiverWithTickAction. This 
        // method _is_ actually virtual, because... these keywords are applied at random?
        public override void UsedThisTick()
        {
            base.UsedThisTick();
            gasComp?.Notify_UsedThisTick();
        }
    }
}