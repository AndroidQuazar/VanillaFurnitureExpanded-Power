using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace VanillaPowerExpanded
{



    public class VanillaPowerExpanded_Mod : Mod
    {
        public static VanillaPowerExpanded_Settings settings;

        public VanillaPowerExpanded_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<VanillaPowerExpanded_Settings>();
        }
        public override string SettingsCategory() => "Vanilla Furniture Expanded - Power";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoWindowContents(inRect);
        }
    }

   
}
