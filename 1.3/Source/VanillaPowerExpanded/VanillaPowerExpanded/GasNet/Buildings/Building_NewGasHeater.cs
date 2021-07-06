// Building_GasHeater.cs
// Copyright Karel Kroeze, 2020-2020

using RimWorld;
using UnityEngine;
using Verse;

namespace GasNetwork
{
    public class Building_NewGasHeater : Building_Heater
    {
        public const float MAGIC_NUMBER_ALPHA = 4.16666651f;
        public override void TickRare()
        {
            if (this.TryGetComp<CompGasTrader>(out var gas))
            {
                if (!gas.GasOn)
                    return;

                float ambient = AmbientTemperature;
                float efficiency = Mathf.InverseLerp(120, 20, ambient);
                float maxEnergy = compTempControl.Props.energyPerSecond * efficiency * MAGIC_NUMBER_ALPHA;
                float energyUsed = GenTemperature.ControlTemperatureTempChange(Position, Map, maxEnergy,
                                                                                compTempControl.targetTemperature);
                if (Mathf.Approximately(energyUsed, 0))
                {
                    gas.GasConsumption = gas.Props.gasConsumption * compTempControl.Props.lowPowerConsumptionFactor;
                }
                else
                {
                    gas.GasConsumption = gas.Props.gasConsumption;
                    this.GetRoom().Temperature += energyUsed;
                }
            }
            else
            {
                // TODO: why is base.TickRare() conditional?
                base.TickRare();
            }
        }
    }
}