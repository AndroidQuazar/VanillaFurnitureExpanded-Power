using System;
using Verse;
using RimWorld;
using System.Text;

namespace VanillaPowerExpanded
{
    public class CompPlantHarmRadiusIfBroken : ThingComp
    {
        public int ticksInADay = 60000;
        public int ticksActive = 0;

        public CompProperties_PlantHarmRadiusIfBroken PropsPlantHarmRadius
        {
            get
            {
                return (CompProperties_PlantHarmRadiusIfBroken)this.props;
            }
        }

       

        public float CurrentRadius
        {
            get
            {
                return this.PropsPlantHarmRadius.radiusPerDayCurve.Evaluate(ticksActive/ ticksInADay);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.ticksActive, "ticksActive", 0, false);
            Scribe_Values.Look<int>(ref this.ticksToPlantHarm, "ticksToPlantHarm", 0, false);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostPostMake();
           
        }

       

        public override void CompTick()
        {
            if (!this.parent.Spawned)
            {
                return;
            }
            if (!this.parent.GetComp<CompBreakdownable>().BrokenDown)
            {
                ticksActive=0;
                return;
            }
            if (this.parent.Map.gameConditionManager.ElectricityDisabled)
            {
                ticksActive = 0;
                return;
            }


            ticksActive++;
           
            this.ticksToPlantHarm--;
            if (this.ticksToPlantHarm <= 0)
            {
                float currentRadius = this.CurrentRadius;
                float num = 3.14159274f * currentRadius * currentRadius * this.PropsPlantHarmRadius.harmFrequencyPerArea;
                float num2 = 60f / num;
                int num3;
                if (num2 >= 1f)
                {
                    this.ticksToPlantHarm = GenMath.RoundRandom(num2);
                    num3 = 1;
                }
                else
                {
                    this.ticksToPlantHarm = 1;
                    num3 = GenMath.RoundRandom(1f / num2);
                }
                for (int i = 0; i < num3; i++)
                {
                    this.HarmRandomPlantInRadius(currentRadius);
                }
            }
        }

        private void HarmRandomPlantInRadius(float radius)
        {
            IntVec3 c = this.parent.Position + (Rand.InsideUnitCircleVec3 * radius).ToIntVec3();
            if (!c.InBounds(this.parent.Map))
            {
                return;
            }
            Plant plant = c.GetPlant(this.parent.Map);
            if (plant != null)
            {
                if (plant.LeaflessNow)
                {
                    if (Rand.Value < this.PropsPlantHarmRadius.leaflessPlantKillChance)
                    {
                        plant.Kill(null, null);
                        return;
                    }
                }
                else
                {
                    plant.MakeLeafless(Plant.LeaflessCause.Poison);
                }
            }
        }

      

        private int ticksToPlantHarm;

       
    }
}
