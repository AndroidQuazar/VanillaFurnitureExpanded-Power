using System;
using RimWorld;
using Verse;

namespace VanillaPowerExpanded
{
    public class CompVariableStack : ThingComp
    {
        public int tickCounter = 0;
        public int tickCounterInterval = 2400;

        public override void CompTick()
        {
            base.CompTick();

            tickCounter++;

            if (tickCounter > tickCounterInterval)
            {

                try
                {
                    Building building = (Building)parent.Map.thingGrid.ThingAt(parent.Position, ThingDef.Named("VPE_GasTank"));
                    if (building != null)
                    {
                        this.parent.def.stackLimit = 1000;
                    }
                }
                catch { }
                tickCounter = 0;

            }





            
        }


    }


}

