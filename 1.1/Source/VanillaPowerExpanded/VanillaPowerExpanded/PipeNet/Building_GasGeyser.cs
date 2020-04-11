using System;
using Verse;
using Verse.Sound;
using RimWorld;

namespace VanillaPowerExpanded
{
    public class Building_GasGeyser : Building
    {
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
                Log.Message("Geyser spray sustainer still playing after 1000 ticks. Force-ending.", false);
                this.spraySustainer.End();
                this.spraySustainer = null;
            }
        }

        private IntermittentGasSprayer steamSprayer;

        public Building harvester;

        private Sustainer spraySustainer;

        private int spraySustainerStartTick = -999;
    }
}
