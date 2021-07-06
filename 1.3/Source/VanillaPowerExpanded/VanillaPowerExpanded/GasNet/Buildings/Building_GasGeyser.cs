using System;
using Verse;
using Verse.Sound;
using RimWorld;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
namespace VanillaPowerExpanded
{
    public class Building_GasGeyser : Building
    {

        private IntermittentGasSprayer steamSprayer;

        public Building harvester;

        private Sustainer spraySustainer;

        private int spraySustainerStartTick = -999;
        public bool HoleNeedsPluggingSir = false;

     

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.HoleNeedsPluggingSir, "HoleNeedsPluggingSir", false, false);
         

        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.steamSprayer = new IntermittentGasSprayer(this);
            this.steamSprayer.startSprayCallback = new Action(this.StartSpray);
            this.steamSprayer.endSprayCallback = new Action(this.EndSpray);
        }

        private void StartSpray()
        {
            SnowUtility.AddSnowRadial(this.OccupiedRect().RandomCell, base.Map, 4f, -0.06f);
            this.spraySustainer = SoundDefOf.GeyserSpray.TrySpawnSustainer(new TargetInfo(base.Position, base.Map, false));
            this.spraySustainerStartTick = Find.TickManager.TicksGame;
        }

        private void EndSpray()
        {
            if (this.spraySustainer != null)
            {
                this.spraySustainer.End();
                this.spraySustainer = null;
            }
        }

        public override void Tick()
        {
            if (this.harvester == null)
            {
                this.steamSprayer.SteamSprayerTick();
            }
            if (this.spraySustainer != null && Find.TickManager.TicksGame > this.spraySustainerStartTick + 1000)
            {
                Log.Message("Geyser spray sustainer still playing after 1000 ticks. Force-ending.");
                this.spraySustainer.End();
                this.spraySustainer = null;
            }
        }

        [DebuggerHidden]
        public override IEnumerable<Gizmo> GetGizmos()
        {
           
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            
            if (HoleNeedsPluggingSir)
            {
                yield return new Command_Action
                {
                    action = new Action(this.CancelHoleForPlugging),
                    hotKey = KeyBindingDefOf.Misc2,
                    defaultDesc = "VPE_CancelPlugHoleDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/VPE_CancelPlugHole", true),
                    defaultLabel = "VPE_CancelPlugHole".Translate()
                };

            } else {

                yield return new Command_Action
                {
                    action = new Action(this.SetHoleForPlugging),
                    hotKey = KeyBindingDefOf.Misc2,
                    defaultDesc = "VPE_PlugHoleDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/VPE_PlugHole", true),
                    defaultLabel = "VPE_PlugHole".Translate()
                };
              
            }
                
            
        }

        private void SetHoleForPlugging()
        {
            HoleNeedsPluggingSir = true;
          
        }

        private void CancelHoleForPlugging()
        {
            HoleNeedsPluggingSir = false;
           
        }


    }
}
