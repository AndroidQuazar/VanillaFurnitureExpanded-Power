using System;
using System.Text;
using Verse;
using RimWorld;

namespace VanillaPowerExpanded
{
    [StaticConstructorOnStartup]
    public class Building_GasValve : Building
    {
        public override bool TransmitsPowerNow
        {
            get
            {
                return FlickUtility.WantsToBeOn(this);
            }
        }

        public override Graphic Graphic
        {
            get
            {
                return this.flickableComp.CurrentGraphic;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.flickableComp = base.GetComp<CompFlickable>();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (this.flickableComp == null)
                {
                    this.flickableComp = base.GetComp<CompFlickable>();
                }
                this.wantsOnOld = !FlickUtility.WantsToBeOn(this);
                this.UpdateGasGrid();
            }
        }

        protected override void ReceiveCompSignal(string signal)
        {
            if (signal == "FlickedOff" || signal == "FlickedOn" || signal == "ScheduledOn" || signal == "ScheduledOff")
            {
                this.UpdateGasGrid();
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine();
            }
            stringBuilder.Append("PowerSwitch_Power".Translate() + ": ");
            if (FlickUtility.WantsToBeOn(this))
            {
                stringBuilder.Append("On".Translate().ToLower());
            }
            else
            {
                stringBuilder.Append("Off".Translate().ToLower());
            }
            return stringBuilder.ToString();
        }

        private void UpdateGasGrid()
        {
            if (FlickUtility.WantsToBeOn(this) != this.wantsOnOld)
            {
                if (base.Spawned)
                {
                    this.Map.GetComponent<PipeMapComponent>().Notfiy_TransmitterTransmitsPowerNowChanged(this.TryGetComp<CompPipe>());
                }
                this.wantsOnOld = FlickUtility.WantsToBeOn(this);
                
            }
        }

        private bool wantsOnOld = true;

        private CompFlickable flickableComp;
    }
}
