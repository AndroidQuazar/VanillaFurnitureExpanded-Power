﻿using GasNetwork.Overlay;
using RimWorld;
using UnityEngine;
using Verse;

namespace GasNetwork
{
    public class Designator_DeconstructGasNetwork : Designator_Deconstruct
    {
        private bool CheckDef(ThingDef gasPipe)
        {
            if (gasPipe?.defName == null) return false;

            return gasPipe.defName.Equals("VPE_GasPipe") || gasPipe.defName.Equals("VPE_GasPipeSub");
        }

        public override AcceptanceReport CanDesignateThing(Thing gasPipe)
        {
            return (base.CanDesignateThing(gasPipe).Accepted && this.CheckDef(gasPipe.def)) || (gasPipe is Blueprint_Build && this.CheckDef(gasPipe.def.entityDefToBuild as ThingDef));
        }

        public override void SelectedUpdate()
        {
            base.SelectedUpdate();
            SectionLayer_GasNetwork.DrawGasGridOverlayThisFrame();
        }

        public Designator_DeconstructGasNetwork()
        {
            this.defaultLabel = "VPE_DeconstructGasPipes".Translate();
            this.defaultDesc = "VPE_DeconstructGasPipesDesc".Translate();
            this.icon = ContentFinder<Texture2D>.Get("Things/Building/Linked/GasPipe_MenuIcon", true);
            this.hotKey = null;
        }
    }
}
