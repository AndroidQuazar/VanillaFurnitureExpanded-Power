using System;
using UnityEngine;
using Verse;
using RimWorld;
using System.Collections.Generic;

namespace VanillaPowerExpanded
{
    [StaticConstructorOnStartup]
    public class CompLightningPowerPlant : CompPowerPlant
    {

        public int tickCounter = 0;
        public int tickCounterInterval = 6000;
      
        
        private float fuel = 0f;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref this.fuel, "fuel", 0f, false);
            Scribe_Values.Look<int>(ref this.tickCounter, "tickCounter", 0, false);

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
                text = text + "VPE_LightningTime".Translate() + numTicks.ToStringTicksToPeriod(true, false, true, true) + "\n";
                text += "VPE_Producing".Translate(base.DesiredPowerOutput);
            }
            else text += "VPE_NotLightningProducing".Translate();


            return text;
        }


        public override void CompTick()
        {
            base.CompTick();
            ConsumeFuel(ConsumptionRatePerTick);
            tickCounter++;

            if (tickCounter > tickCounterInterval)
            {
                if (this.parent.Map!=null && this.parent.Map.weatherManager.curWeather.eventMakers.Count > 0)
                {
                    this.parent.Map.weatherManager.eventHandler.AddEvent(new WeatherEvent_CustomLightningStrike(this.parent.Map, this.parent.Position));
                    fuel += 24;
                }


                tickCounter = 0;
            }




                
        }
    }
}

