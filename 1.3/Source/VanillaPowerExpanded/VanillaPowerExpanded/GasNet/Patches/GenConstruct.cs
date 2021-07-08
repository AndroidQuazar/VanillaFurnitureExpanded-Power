// GenConstruct.cs
// Copyright Karel Kroeze, 2020-2020

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GasNetwork.Patches
{
    public static partial class Helpers
    {
        public static FieldInfo PowerConduit_FI =
            AccessTools.Field(typeof(ThingDefOf), nameof(ThingDefOf.PowerConduit));

        public static bool GasCheck(ThingDef constructible, ThingDef target)
        {
            Log.Debug($"c: {constructible.defName}, t: {target.defName}");
            if (!constructible.EverTransmitsGas()) return false;

            if (target != DefOf.VPE_GasPipe && target != DefOf.VPE_GasPipeSub) return false;

            if (constructible == DefOf.VPE_GasPipe || constructible == DefOf.VPE_GasPipeSub) return false;

            return true;
        }

        public static bool GasCheck2(ThingDef target, ThingDef constructible)
        {
            return GasCheck(constructible, target);
        }
    }

    [HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.CanPlaceBlueprintOver))]
    public class GenConstruct_CanPlaceBlueprintOver
    {
        /**
         * IL_016B: ldarg.0
         * IL_016C: ldsfld    class Verse.ThingDef RimWorld.ThingDefOf::PowerConduit
         * IL_0171: beq.s     IL_017D
         *  
         * IL_0173: ldloc.3
         * IL_0174: ldsfld    class Verse.ThingDef RimWorld.ThingDefOf::PowerConduit
         * IL_0179: bne.un.s  IL_017D
         *  
         * IL_017B: ldc.i4.1
         * IL_017C: ret
         *  
         * IL_017D: ldarg.0
         * IL_017E: isinst    Verse.TerrainDef
         * IL_0183: brfalse.s IL_019C
         */
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> _instructions,
            ILGenerator generator)
        {
            var instructions = _instructions.ToList();
            for (var i = 0; i < instructions.Count; i++)
            {
                yield return instructions[i];

                if (i > 4
                    && instructions[i - 4].LoadsField(Helpers.PowerConduit_FI)
                    && instructions[i - 3].Branches(out _)
                    && instructions[i - 2].LoadsConstant(1L)
                    && instructions[i - 1].opcode == OpCodes.Ret
                    && instructions[i - 0].opcode == OpCodes.Ldarg_0)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Ldfld,
                        AccessTools.Field(AccessTools.Inner(typeof(GenConstruct), "<>c__DisplayClass16_0"),
                            "oldDefBuilt"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(
                        typeof(Helpers),
                        nameof(Helpers.GasCheck)));

                    var label = generator.DefineLabel();
                    yield return new CodeInstruction(OpCodes.Brfalse, label);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    yield return new CodeInstruction(OpCodes.Ret);
                    yield return new CodeInstruction(OpCodes.Ldarg_0) {labels = new List<Label>(new[] {label})};
                }
            }
        }
    }

    [HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.BlocksConstruction))]
    public class GenConstruct_BlocksConstruction
    {
        /**
         * The below block checks for powerConduits. We want to add another check for
         * pipes right below it.
         * 
         * Note that the logic is odd; we check all the 'negative' conditions, and skip over
         * returning false if any are met. Only if all are met do we allow control to pass to
         * returning false.
         * 
         * Checks that constructible.def.EverTransmitsPower() returns true.
         * IL_0102: ldloc.1 // holds constructible.def
         * IL_0103: callvirt  instance bool Verse.ThingDef::get_EverTransmitsPower()
         * IL_0108: brfalse.s IL_0121
         * 
         * Checks that target.def == ThingDefOf.PowerConduit.
         * IL_010A: ldarg.1
         * IL_010B: ldfld     class Verse.ThingDef Verse.Thing::def
         * IL_0110: ldsfld    class Verse.ThingDef RimWorld.ThingDefOf::PowerConduit
         * IL_0115: bne.un.s  IL_0121
         * 
         * Checks that constructible.def != ThingDefOf.PowerConduit.
         * IL_0117: ldloc.1
         * IL_0118: ldsfld    class Verse.ThingDef RimWorld.ThingDefOf::PowerConduit
         * IL_011D: beq.s     IL_0121
         * 
         * IL_011F: ldc.i4.0
         * IL_0120: ret
         */
        public static FieldInfo def_FI =
            AccessTools.Field(typeof(Thing), nameof(Thing.def));

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> _instructions,
            ILGenerator generator)
        {
            var instructions = _instructions.ToList();
            for (var i = 0; i < instructions.Count; i++)
            {
                yield return instructions[i];

                if (i > 5
                    && instructions[i - 5].LoadsField(Helpers.PowerConduit_FI)
                    && instructions[i - 4].Branches(out var _dump)
                    && instructions[i - 3].LoadsConstant(0L)
                    && instructions[i - 2].opcode == OpCodes.Ret
                    && instructions[i - 1].opcode == OpCodes.Ldarg_1
                    && instructions[i - 0].LoadsField(def_FI))

                {
                    // push constructible.def
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    // pop target.def & constructible.def, push bool
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(
                            typeof(Helpers),
                            nameof(Helpers.GasCheck2)));

                    // on false, skip to next check.
                    // on true control passes to returning false.
                    var next = generator.DefineLabel();
                    yield return new CodeInstruction(OpCodes.Brfalse, next);

                    // push 1L (TRUE) 
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);

                    // return
                    yield return new CodeInstruction(OpCodes.Ret);

                    // push target
                    yield return new CodeInstruction(OpCodes.Ldarg_1) {labels = new List<Label>(new[] {next})};

                    // pop target, push target.def
                    yield return new CodeInstruction(OpCodes.Ldfld, def_FI);
                }
            }
        }
    }
}