// Copyright Karel Kroeze, 2020-2020.
// VanillaPowerExpanded/VanillaPowerExpanded/PathGrid.cs

using System.Linq;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace GasNetwork.HarmonyPatches
{
    [HarmonyPatch(typeof(Verse.AI.PathGrid), nameof(Verse.AI.PathGrid.CalculatedCostAt))]
    public static class PathGrid
    {
        // public int CalculatedCostAt(IntVec3 c, bool perceivedStatic, IntVec3 prevCell)
        public static void Postfix(IntVec3 c, bool perceivedStatic, ref int __result, Map ___map)
        {
            // add extra pathing cost for tiles on or close to toxic or flammable gas. 
            // this is analogous to the vanilla code for fire.
            if (perceivedStatic)
            {
                var before = __result;
                foreach (var offset in GenAdj.AdjacentCellsAndInside)
                {
                    var cell   = c + offset;
                    var center = cell.Equals(c);
                    if (!cell.InBounds(___map))
                    {
                        continue;
                    }

                    var gases = ___map.thingGrid.ThingsListAtFast(c + offset).OfType<Gas_Spreading>();
                    foreach (var gas in gases)
                    {
                        if (gas.Flammable)
                        {
                            __result += Mathf.CeilToInt(Mathf.Clamp01(gas.Density) * (center ? 1000 : 150));
                        }

                        if (gas.Toxic)
                        {
                            __result += Mathf.CeilToInt(Mathf.Clamp01(gas.Density) * (center ? 500 : 75));
                        }
                    }
                }
            }
        }
    }
}