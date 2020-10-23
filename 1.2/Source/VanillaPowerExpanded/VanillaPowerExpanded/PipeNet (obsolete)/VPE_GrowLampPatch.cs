// Copyright Karel Kroeze, 2020-2020.
// VanillaPowerExpanded/VanillaPowerExpanded/VPE_GrowLampPatch.cs

using HarmonyLib;
using Verse;

// So, let's comment this code, since it uses Harmony and has moderate complexity

namespace VanillaPowerExpanded
{
    //Setting the Harmony instance
    [StaticConstructorOnStartup]
    public class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.vanillapowerexpanded");

            // duplicate PatchAll
            // harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }


    /*This Harmony Postfix modifies the growth lamp so it checks for helixien gas fuel too without the lighting 
   */
    [HarmonyPatch(typeof(CompGlower))]
    [HarmonyPatch("ShouldBeLitNow", MethodType.Getter)]
    public static class VPE_CompGlower_ShouldBeLitNow_Patch
    {
        [HarmonyPostfix]
        public static void NotLitIfNoGas(ref bool __result, ref CompGlower __instance)

        {
            var compPipeTrader = __instance.parent.TryGetComp<CompPipeTrader>();
            if (compPipeTrader != null && !compPipeTrader.PowerOn)
            {
                __result = false;
            }
        }
    }
}