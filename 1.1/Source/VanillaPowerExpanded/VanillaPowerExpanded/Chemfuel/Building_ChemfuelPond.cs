using System;
using Verse;
using Verse.Sound;
using RimWorld;

namespace VanillaPowerExpanded
{
    public class Building_ChemfuelPond : Building
    {
        public int fuelLeft = 750;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.fuelLeft, "fuelLeft", 0, false);

        }

        public override string GetInspectString()
        {
            base.GetInspectString();
            string text = string.Concat(new string[]{});
            if (this.HasFuel())
            {
               
                text += "VPE_PondHasFuel".Translate(fuelLeft);
            }
            else text += "VPE_PondNoFuel".Translate();


            return text;
        }

        public bool HasFuel() {
            if (fuelLeft > 0)
            {
                return true;
            }
            else return false;
        }

       

      
    }
}
