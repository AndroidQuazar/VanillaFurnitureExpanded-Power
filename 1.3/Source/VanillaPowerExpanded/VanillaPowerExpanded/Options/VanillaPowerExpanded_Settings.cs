using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;


namespace VanillaPowerExpanded
{


    public class VanillaPowerExpanded_Settings : ModSettings

    {


        public bool disableGasPathCalculations = false;
    



        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref disableGasPathCalculations, "disableGasPathCalculations", false, true);
       

        }

        public void DoWindowContents(Rect inRect)
        {
            Listing_Standard ls = new Listing_Standard();


            ls.Begin(inRect);
            ls.Gap(10f);


            ls.CheckboxLabeled("VPE_DisableGasPathCalculations".Translate(), ref disableGasPathCalculations, null);



            ls.End();
        }



    }










}
