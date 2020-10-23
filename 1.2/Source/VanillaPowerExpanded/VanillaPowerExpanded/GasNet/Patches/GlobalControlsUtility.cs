// Copyright Karel Kroeze, 2020-2020.
// VanillaPowerExpanded/VanillaPowerExpanded/GlobalControlsUtility.cs

using HarmonyLib;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace GasNetwork.Patches
{
    /**
     * Add a wind readout to the status list thingy on the bottom right.
     * Postfixing DoDate because that's a reasonable place (with the other weather),
     * and because that has a ref curBaseY, which means we don't need to do
     * a transpiler to insert our row.
     */
    [HarmonyPatch(typeof(RimWorld.GlobalControlsUtility), "DoDate")]
    public static class GlobalControlsUtility
    {
        public static void Postfix(float leftX, float width, ref float curBaseY)
        {
            if (Event.current.type != EventType.Repaint
             || WorldRendererUtility.WorldRenderedNow
             || Find.CurrentMap == null)
            {
                return;
            }

            curBaseY -= 24;
            var rect = new Rect(leftX, curBaseY, width, 24);

            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(rect.LeftPartPixels(width - 26), Find.CurrentMap.windSpeedString(Mod.Settings.unit));
            Text.Anchor = TextAnchor.UpperLeft;

            WindExtensions.DrawWindVector(Find.CurrentMap, rect.RightPartPixels(24));
        }
    }
}