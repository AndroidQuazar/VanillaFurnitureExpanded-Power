// CompGlower.cs
// Copyright Karel Kroeze, 2020-2020

using HarmonyLib;
using Verse;

namespace GasNetwork
{
    [HarmonyPatch( typeof( CompGlower ), "ShouldBeLitNow", MethodType.Getter )]
    public static class CompGlower_ShouldBeLit
    {
        public static void Postfix( ThingWithComps ___parent, ref bool __result )
        {
            // should not be on if parent is gas powered and not on.
            if ( __result && ___parent.TryGetComp<CompGasTrader>( out var gas ) && !gas.GasOn )
                __result = false;
        }
    }

    [HarmonyPatch( typeof( CompGlower ), nameof( CompGlower.ReceiveCompSignal ) )]
    public static class CompGlower_ReceiveCompSignal
    {
        public static void Prefix( string signal, CompGlower __instance )
        {
            // should react to gas on/off signals
            if ( signal == CompGasTrader.Signal_GasOn || signal == CompGasTrader.Signal_GasOff )
                __instance.UpdateLit( __instance.parent.Map );
        }
    }
}