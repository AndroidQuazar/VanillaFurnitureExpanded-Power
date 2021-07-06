using RimWorld;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace VanillaPowerExpanded
{
    public class CompPipeTank : CompPipe
    {
        private float storedEnergy;

        private const float SelfDischargingWatts = 5f;

        public float AmountCanAccept
        {
            get
            {
                if (this.parent.IsBrokenDown())
                {
                    return 0f;
                }
                CompProperties_PipeTank props = this.Props;
                return (props.storedEnergyMax - this.storedEnergy) / props.efficiency;
            }
        }

        public float StoredEnergy
        {
            get
            {
                return this.storedEnergy;
            }
        }

        public float StoredEnergyPct
        {
            get
            {
                return this.storedEnergy / this.Props.storedEnergyMax;
            }
        }

        public new CompProperties_PipeTank Props
        {
            get
            {
                return (CompProperties_PipeTank)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref this.storedEnergy, "storedPower", 0f, false);
            CompProperties_PipeTank props = this.Props;
            if (this.storedEnergy > props.storedEnergyMax)
            {
                this.storedEnergy = props.storedEnergyMax;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            this.DrawPower(Mathf.Min(5f * CompPipe.WattsToWattDaysPerTick, this.storedEnergy));
        }

        public void AddEnergy(float amount)
        {
            if (amount < 0f)
            {
                Log.Error("Cannot add negative energy " + amount, false);
                return;
            }
            if (amount > this.AmountCanAccept)
            {
                amount = this.AmountCanAccept;
            }
            amount *= this.Props.efficiency;
            this.storedEnergy += amount;
        }

        public void DrawPower(float amount)
        {
            this.storedEnergy -= amount;
            if (this.storedEnergy < 0f)
            {
                Log.Error("Drawing power we don't have from " + this.parent, false);
                this.storedEnergy = 0f;
            }
        }

        public void SetStoredEnergyPct(float pct)
        {
            pct = Mathf.Clamp01(pct);
            this.storedEnergy = this.Props.storedEnergyMax * pct;
        }

        public override void ReceiveCompSignal(string signal)
        {
            if (signal == "Breakdown")
            {
                this.DrawPower(this.StoredEnergy);
            }
        }

        public override string CompInspectStringExtra()
        {
            CompProperties_PipeTank props = this.Props;
            string text = string.Concat(new string[]
            {
                "VPE_LiquidTankStored".Translate(),
                ": ",
                this.storedEnergy.ToString("F0"),
                " / ",
                props.storedEnergyMax.ToString("F0"),
                " ml-d"
            });
            string text2 = text;
            text = string.Concat(new string[]
            {
                text2,
                "\n",
                "VPE_LiquidTankEfficiency".Translate(),
                ": ",
                (props.efficiency * 100f).ToString("F0"),
                "%"
            });
            if (this.storedEnergy > 0f)
            {
                text2 = text;
                text = string.Concat(new string[]
                {
                    text2,
                    "\n",
                    "VPE_TankLeakage".Translate(),
                    ": ",
                    5f.ToString("F0"),
                    " ml"
                });
            }
            return text + "\n" + base.CompInspectStringExtra();
        }

        [DebuggerHidden]
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo c in base.CompGetGizmosExtra())
            {
                yield return c;
            }
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Fill",
                    action = delegate
                    {
                        this.SetStoredEnergyPct(1f);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Empty",
                    action = delegate
                    {
                        this.SetStoredEnergyPct(0f);
                    }
                };
            }
        }
    }
}
