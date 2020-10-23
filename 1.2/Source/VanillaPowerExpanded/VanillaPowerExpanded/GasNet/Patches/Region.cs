// Copyright Karel Kroeze, 2020-2020.
// VanillaPowerExpanded/VanillaPowerExpanded/Region.cs

using HarmonyLib;
using Verse;

namespace GasNetwork.Patches
{
    /**
     * Add a region danger check for Gas.
     *
     * We either have to create a transpiler to inject region gas calculations
     * and provide a per-frame cache, or we can just build our own gas danger cache
     * and keep that up-to-date in our own time.
     *
     * There's no real reason to recalculate region danger on every tick.
     *
     * public Danger DangerFor(Pawn p)
     */
    [HarmonyPatch(typeof(Verse.Region), nameof(Verse.Region.DangerFor))]
    public static class Region
    {
        public static void Postfix(ref Danger __result, Verse.Region __instance)
        {
            var danger = __instance.Map.GetComponent<MapComponent_GasDanger>()?.DangerIn(__instance) ?? Danger.None;

            //  result is highest level of danger.
            __result = danger > __result ? danger : __result;
        }
    }
}