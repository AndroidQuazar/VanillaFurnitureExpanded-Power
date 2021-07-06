// Copyright Karel Kroeze, 2020-2020.
// VanillaPowerExpanded/VanillaPowerExpanded/PlaceWorker_GasVent.cs

using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace GasNetwork
{
    public class PlaceWorker_GasVent : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 pos, Rot4 rot, Map map,
                                                       Thing        thingToIgnore = null,
                                                       Thing        thing         = null)
        {
            var ventingPos = pos + IntVec3.North.RotatedBy(rot);
            if (ventingPos.Impassable(map))
                return "MustPlaceVentWithFreeSpaces".Translate();
            return true;
        }

        public override void DrawGhost(ThingDef def, IntVec3 pos, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            var ventingPos = GenGas.VentingPosition(pos, rot);

            // draw venting cell
            GenDraw.DrawFieldEdges(new List<IntVec3> {ventingPos}, Color.white);

            // draw venting area
            var map = Find.CurrentMap;
            var affectedArea = GenGas.GetGasVentArea(ventingPos, map,
                                                     def.GetCompProperties<CompProperties_GasVent>()?.ventingRadius ?? 0);
            if (affectedArea.NullOrEmpty()) return;

            GenDraw.DrawFieldEdges(affectedArea, Resources.GasGreen);
        }
    }
}