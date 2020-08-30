using System;
using UnityEngine;
using Verse;
using RimWorld;
using System.Collections.Generic;

namespace VanillaPowerExpanded
{
    [StaticConstructorOnStartup]
    public class CompSoulsPowerPlant : CompPowerPlant
    {

        public int tickCounter = 0;
        public int tickCounterInterval = 300;
        public int radius = 25;
        public bool flagOnce = false;
        private float fuel;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref this.fuel, "fuel", 0f, false);
           
        }

        protected override float DesiredPowerOutput
        {
            get
            {
                if (HasFuel)
                {
                    return base.DesiredPowerOutput;
                }
                else return 0f;
            }
        }

        private float ConsumptionRatePerTick
        {
            get
            {
                return 24f / 60000f;
            }
        }

        public bool HasFuel
        {
            get
            {
                return this.fuel > 0f;
            }
        }

        public void ConsumeFuel(float amount)
        {
            if (this.fuel <= 0f)
            {
                return;
            }
            this.fuel -= amount;
            if (this.fuel <= 0f)
            {
                this.fuel = 0f;
                this.parent.BroadcastCompSignal("RanOutOfFuel");
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (!this.HasFuel)
            {
                this.parent.Map.overlayDrawer.DrawOverlay(this.parent, OverlayTypes.OutOfFuel);
            }
            
        }

        public override string CompInspectStringExtra()
        {
            base.CompInspectStringExtra();
            string text = string.Concat(new string[]
            {
               
            });
            if (this.HasFuel)
            {
                int numTicks = (int)(this.fuel / 24f * 60000f);
                text = text + "VPE_SoulsTime".Translate() + numTicks.ToStringTicksToPeriod(true, false, true, true) + "\n";
                text += "VPE_Producing".Translate(base.DesiredPowerOutput);
            }
            else text += "VPE_NotProducing".Translate();


            return text;
        }


        public override void CompTick()
        {
            base.CompTick();
            ConsumeFuel(ConsumptionRatePerTick);
            tickCounter++;
           
            if (tickCounter > tickCounterInterval)
            {

                Building building = this.parent as Building;
                if (building.Map != null)
                {
                    foreach (Thing thing in GenRadial.RadialDistinctThingsAround(building.Position, building.Map, radius, true))
                    {
                        Corpse corpse = thing as Corpse;
                        if (corpse != null)
                        {
                            // Log.Message(corpse.def.defName);
                            if (corpse.InnerPawn.def.race.Humanlike)
                            {


                                CompRottable compRottable = corpse.TryGetComp<CompRottable>();
                                if (compRottable.Stage == RotStage.Fresh)
                                {
                                    //Log.Message("Found coprse named "+ corpse.def.defName);
                                    this.fuel += 2;
                                    //Log.Message(fuel.ToString());
                                    compRottable.RotProgress += 1000000;
                                    flagOnce = true;
                                }




                            }

                            if (flagOnce) { flagOnce = false; break; }

                        }
                    }

                   


                }




                tickCounter = 0;
            }
        }
    }
}
