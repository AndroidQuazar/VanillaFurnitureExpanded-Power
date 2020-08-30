using System;
using Verse;

namespace VanillaPowerExpanded
{
    public class CompProperties_Pipe : CompProperties
    {
       
        public bool transmitsGas = true;
        public float basePowerConsumption;
        public bool shortCircuitInRain;
        public SoundDef soundPowerOn;
        public SoundDef soundPowerOff;
        public SoundDef soundAmbientPowered;
        public bool needsGasToWork = false;
    }
}
