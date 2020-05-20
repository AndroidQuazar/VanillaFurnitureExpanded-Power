using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using System.Collections.Generic;
using RimWorld.Planet;
using System.Linq;
using System;

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
            harmony.PatchAll(Assembly.GetExecutingAssembly());
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

            CompPipeTrader compPipeTrader = __instance.parent.TryGetComp<CompPipeTrader>();
            if (compPipeTrader != null && !compPipeTrader.PowerOn)
            {
                __result = false;
            }
            



          




        }
    }


 

    }













