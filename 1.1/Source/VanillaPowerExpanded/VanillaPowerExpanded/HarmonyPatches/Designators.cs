// Designator_Build_SelectedUpdate.cs
// Copyright Karel Kroeze, 2020-2020

using GasNetwork.Overlay;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GasNetwork
{
    [HarmonyPatch(typeof( Designator_Build ), nameof( Designator_Build.SelectedUpdate ) )]
    public class Designator_Build_SelectedUpdate
    {
        public static void Postfix( BuildableDef ___entDef )
        {
            if (___entDef is ThingDef thingDef && thingDef.EverTransmitsGas())
                SectionLayer_GasNetwork.DrawGasGridOverlayThisFrame();
        }
    }

    [HarmonyPatch( typeof( Designator_Install ), nameof(Designator_Install.SelectedUpdate ) )]
    public class Designator_Install_SelectedUpdate
    {
        public static void Postfix( Designator_Install __instance )
        {
            if (__instance.PlacingDef is ThingDef thingDef && thingDef.EverTransmitsGas() )
                SectionLayer_GasNetwork.DrawGasGridOverlayThisFrame();
        }
    }
}