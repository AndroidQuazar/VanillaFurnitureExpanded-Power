using GasNetwork.Overlay;
using RimWorld;
using UnityEngine;
using Verse;

namespace GasNetwork
{
    public class Designator_DeconstructGasNetwork : Designator_Deconstruct
    {
        public Designator_DeconstructGasNetwork()
        {
            defaultLabel = "VPE_DeconstructGasPipes".Translate();
            defaultDesc  = "VPE_DeconstructGasPipesDesc".Translate();
            icon         = ContentFinder<Texture2D>.Get("UI/Commands/GasPipeDeconstruct_MenuIcon");
            hotKey       = null;
        }

        private bool CheckDef(ThingDef gasPipe)
        {
            if (gasPipe?.defName == null)
            {
                return false;
            }

            return gasPipe.defName.Equals("VPE_GasPipe") || gasPipe.defName.Equals("VPE_GasPipeSub");
        }

        public override AcceptanceReport CanDesignateThing(Thing gasPipe)
        {
            return base.CanDesignateThing(gasPipe).Accepted && CheckDef(gasPipe.def) ||
                   gasPipe is Blueprint_Build               && CheckDef(gasPipe.def.entityDefToBuild as ThingDef);
        }

        public override void SelectedUpdate()
        {
            base.SelectedUpdate();
            SectionLayer_GasNetwork.DrawGasGridOverlayThisFrame();
        }
    }
}