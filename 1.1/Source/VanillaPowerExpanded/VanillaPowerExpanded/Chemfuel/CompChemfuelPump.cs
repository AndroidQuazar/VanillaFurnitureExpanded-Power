using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using System.Text;

namespace VanillaPowerExpanded
{
    public class CompChemfuelPump : ThingComp
    {

        public Building_ChemfuelPond chemfuelPond;
        public float ticksInADay = 60000f;
        public int ticksCounter = 0;

        public CompProperties_ChemfuelPump Props
        {
            get
            {
                return (CompProperties_ChemfuelPump)this.props;
            }
        }





        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.chemfuelPond = (Building_ChemfuelPond)parent.Map.thingGrid.ThingAt(parent.Position, ThingDef.Named("VPE_ChemfuelPond"));
        }



        public override void CompTick()
        {
            base.CompTick();
            ticksCounter++;
            if (ticksCounter > ticksInADay * Props.fuelInterval)
            {
                this.chemfuelPond = (Building_ChemfuelPond)parent.Map.thingGrid.ThingAt(parent.Position, ThingDef.Named("VPE_ChemfuelPond"));
                if (chemfuelPond.fuelLeft > 0)
                {

                    chemfuelPond.fuelLeft -= Props.fuelProduced;
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.Chemfuel, null);
                    thing.stackCount = this.Props.fuelProduced;
                    GenPlace.TryPlaceThing(thing, this.parent.Position, this.parent.Map, ThingPlaceMode.Near, null, null, default(Rot4));
                    ticksCounter = 0;

                }

            }





        }




        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            chemfuelPond = (Building_ChemfuelPond)parent.Map.thingGrid.ThingAt(parent.Position, ThingDef.Named("VPE_ChemfuelPond"));
            if (chemfuelPond != null && chemfuelPond.fuelLeft > 0)
            {
                stringBuilder.Append("VPE_PondHasFuel".Translate(chemfuelPond.fuelLeft));
                stringBuilder.AppendLine();
                int ticks = (int)(ticksInADay * Props.fuelInterval) - ticksCounter;
                stringBuilder.Append("VPE_PumpProducing".Translate(Props.fuelProduced, ticks.ToStringTicksToPeriod(true, false, true, true)));

                return stringBuilder.ToString();

            }
            else
            {
                stringBuilder.Append("VPE_PondNoFuel".Translate());

                return stringBuilder.ToString();

            }


        }


    }
}
