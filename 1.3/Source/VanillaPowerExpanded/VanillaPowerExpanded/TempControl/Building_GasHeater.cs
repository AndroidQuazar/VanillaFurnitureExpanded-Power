using System;
using UnityEngine;
using Verse;
using RimWorld;

namespace VanillaPowerExpanded
{
    public class Building_GasHeater : Building_TempControl
    {
        public override void TickRare()
        {
            CompPipeTrader compPipeTrader = this.GetComp<CompPipeTrader>();
            if (compPipeTrader.PowerOn)
            {
                float ambientTemperature = base.AmbientTemperature;
                float num;
                if (ambientTemperature < 20f)
                {
                    num = 1f;
                }
                else if (ambientTemperature > 120f)
                {
                    num = 0f;
                }
                else
                {
                    num = Mathf.InverseLerp(120f, 20f, ambientTemperature);
                }
                float energyLimit = this.compTempControl.Props.energyPerSecond * num * 4.16666651f;
                float num2 = GenTemperature.ControlTemperatureTempChange(base.Position, base.Map, energyLimit, this.compTempControl.targetTemperature);
                bool flag = !Mathf.Approximately(num2, 0f);
                CompProperties_Pipe props = compPipeTrader.Props;
                if (flag)
                {
                    this.GetRoomGroup().Temperature += num2;
                    compPipeTrader.PowerOutput = -props.basePowerConsumption;
                }
                else
                {
                    compPipeTrader.PowerOutput = -props.basePowerConsumption * this.compTempControl.Props.lowPowerConsumptionFactor;
                }
                this.compTempControl.operatingAtHighPower = flag;
            }
        }

        private const float EfficiencyFalloffSpan = 100f;
    }
}
