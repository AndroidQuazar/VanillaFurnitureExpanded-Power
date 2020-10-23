using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace VanillaPowerExpanded
{
    public class PlaceWorker_Tidal : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            foreach (IntVec3 c in GenAdj.CellsOccupiedBy(loc, rot, checkingDef.Size))
            {
                if (!map.terrainGrid.TerrainAt(c).IsWater)
                {
                    return new AcceptanceReport("VPE_NeedsWater".Translate());
                }
            }
            foreach (Thing generator in GenRadial.RadialDistinctThingsAround(loc, map, 40, true))
            {
                Building generatorBuilding = generator as Building;
                if (generatorBuilding != null && generatorBuilding.def.defName == "VFE_TidalGenerator")
                { return new AcceptanceReport("VPE_NeedsDistance".Translate()); }

                Thing generatorBuildingBlueprint = generator as Thing;
                if (generatorBuildingBlueprint != null && (generatorBuildingBlueprint.def.IsBlueprint || generatorBuildingBlueprint.def.IsFrame) && generatorBuildingBlueprint.def.entityDefToBuild.defName == "VFE_TidalGenerator")
                { return new AcceptanceReport("VPE_NeedsDistance".Translate()); }

            }



            return true;
        }

        public override void DrawGhost(ThingDef def, IntVec3 loc, Rot4 rot, Color ghostCol, Thing thing = null)
        {

            Color color2 = new Color(0f, 0.6f, 0f);

            GenDraw.DrawRadiusRing(loc, 40, color2);

        }




    }


}


