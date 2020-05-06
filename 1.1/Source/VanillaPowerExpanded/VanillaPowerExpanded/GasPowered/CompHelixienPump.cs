using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using System.Text;

namespace VanillaPowerExpanded
{
    public class CompHelixienPump : ThingComp
    {

      
        public float ticksInADay = 60000f;
        public int ticksCounter = 0;

        public CompProperties_HelixienPump Props
        {
            get
            {
                return (CompProperties_HelixienPump)this.props;
            }
        }

       






        public override void CompTick()
        {
            base.CompTick();
            ticksCounter++;
            if (ticksCounter > ticksInADay * Props.fuelInterval)
            {
                
                   
                    Thing thing = ThingMaker.MakeThing(ThingDef.Named("VFE_HelixienCanister"), null);
                    thing.stackCount = this.Props.fuelProduced;
                    GenPlace.TryPlaceThing(thing, this.parent.Position, this.parent.Map, ThingPlaceMode.Near, null, null, default(Rot4));
                    ticksCounter = 0;

                
                
            }





        }

      
      

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            
               
                int ticks = (int)(ticksInADay * Props.fuelInterval) - ticksCounter;
                stringBuilder.Append("VPE_HelixienPumpProducing".Translate(Props.fuelProduced, ticks.ToStringTicksToPeriod(true, false, true, true)));

                return stringBuilder.ToString();

            


        }


    }
}
