using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using RimWorld;


namespace VanillaPowerExpanded
{
    public class JobDriver_PlugHole : JobDriver
    {
       

      

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A).Thing, this.job, 1, -1, null);
        }

       

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            Building buildingGeyser = (Building)this.job.GetTarget(TargetIndex.A).Thing;
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
           
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(1200).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            yield return new Toil
            {
                initAction = delegate
                {

                    
                    buildingGeyser.DeSpawn();
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
           

        }
    }
}
