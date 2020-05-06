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
    public class Building_ChemfuelPond : Building
    {
        public int fuelLeft = 750;

        public bool HoleNeedsPluggingSir = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.fuelLeft, "fuelLeft", 0, false);
            Scribe_Values.Look<bool>(ref this.HoleNeedsPluggingSir, "HoleNeedsPluggingSir", false, false);

        }

       

        public override string GetInspectString()
        {
            base.GetInspectString();
            string text = string.Concat(new string[]{});
            if (this.HasFuel())
            {
               
                text += "VPE_PondHasFuel".Translate(fuelLeft);
            }
            else text += "VPE_PondNoFuel".Translate();


            return text;
        }

        public bool HasFuel() {
            if (fuelLeft > 0)
            {
                return true;
            }
            else return false;
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

            }
            else
            {

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
