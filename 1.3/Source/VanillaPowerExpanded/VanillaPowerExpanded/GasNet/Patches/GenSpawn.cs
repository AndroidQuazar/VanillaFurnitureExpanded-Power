// GenSpawn.cs
// Copyright Karel Kroeze, 2020-2020

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace GasNetwork.Patches
{
    public static partial class Helpers
    {
        public static FieldInfo EntityDefToBuild_FI = AccessTools.Field( typeof( ThingDef ), nameof( ThingDef.entityDefToBuild ) );
        public static MethodInfo EverTransmitsPower_MI = AccessTools.PropertyGetter( typeof( ThingDef ), nameof( ThingDef.EverTransmitsPower ) );
        public static MethodInfo GasUserOverPipe_MI = AccessTools.Method( typeof( Helpers ), nameof( GasUserOverPipe ) );
        public static bool GasUserOverPipe( ThingDef constructible, ThingDef target )
        {
            return constructible.EverTransmitsGas()
                && ( target == DefOf.VPE_GasPipe || target == DefOf.VPE_GasPipeSub );
        }

        public static MethodInfo GasUserOverPipe_Blueprint_MI = AccessTools.Method( typeof( Helpers ), nameof( GasUserOverPipe_Blueprint ) );
        public static bool GasUserOverPipe_Blueprint( ThingDef constructibleBlueprint, ThingDef targetBlueprint )
        {
            return constructibleBlueprint.entityDefToBuild is ThingDef constructible
                && targetBlueprint.entityDefToBuild is ThingDef target
                && GasUserOverPipe( constructible, target );
        }
    }

    [HarmonyPatch(typeof(GenSpawn), nameof(GenSpawn.SpawningWipes ))]
    public class GenSpawn_SpawningWipes
    {
		/**
		 * There are two checks in here that we need to add. The first is for spawning
		 * _Things_, and simply removes any pipes underneath spawned in pipe users.
		 * Primary use for this is God Mode. The second patch checks for blueprints,
		 * and is used in normal play to remove blueprints for pipes when placing a
		 * blueprint for a gas user.
         */
        public static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> _instructions, ILGenerator generator )
        {
            var instructions = _instructions.ToList();
            var firstPatchCompleted = false;
            Label next;

            for ( int i = 0; i < instructions.Count; i++ )
            {
                yield return instructions[i];

                /**
                 * This patch mirrors the below instructions for conduits.
                 *
                 * IL_0081: ldloc.0
                 * IL_0082: callvirt  instance bool Verse.ThingDef::get_EverTransmitsPower()
                 * IL_0087: brfalse.s IL_0093
                 * 
                 * IL_0089: ldloc.1
                 * IL_008A: ldsfld    class Verse.ThingDef RimWorld.ThingDefOf::PowerConduit
                 * IL_008F: bne.un.s  IL_0093
                 * 
                 * IL_0091: ldc.i4.1
                 * IL_0092: ret
                 * 
                 * IL_0093: ldloc.0
                 */

                if ( i > 4 && !firstPatchCompleted
                  && instructions[i - 4].LoadsField( Helpers.PowerConduit_FI )
                  && instructions[i - 3].Branches( out _ )
                  && instructions[i - 2].LoadsConstant( 1L )
                  && instructions[i - 1].opcode == OpCodes.Ret
                  && instructions[i - 0].opcode == OpCodes.Ldloc_0 )
                {
                    firstPatchCompleted = true;
                    yield return new CodeInstruction( OpCodes.Ldloc_1 );
                    yield return new CodeInstruction( OpCodes.Call, Helpers.GasUserOverPipe_MI );
                    next = generator.DefineLabel();
                    yield return new CodeInstruction( OpCodes.Brfalse, next );
                    yield return new CodeInstruction( OpCodes.Ldc_I4_1 );
                    yield return new CodeInstruction( OpCodes.Ret );
                    yield return new CodeInstruction( OpCodes.Ldloc_0 ) {labels = new List<Label>( new[] {next} )};
                }

                /**
                 * The second patch is essentially the same, but applies to _Blueprints_, and thus
                 * uses entityDefToBuild. The value for the conduit variant is also directly returned,
                 * so we have to get a bit fancy with labels.
                 *
                 * IL_015A: ldloc.1
                 * IL_015B: ldfld     class Verse.BuildableDef Verse.ThingDef::entityDefToBuild
                 * IL_0160: ldsfld    class Verse.ThingDef RimWorld.ThingDefOf::PowerConduit
                 * IL_0165: bne.un.s  IL_0188
                 * 
                 * IL_0167: ldloc.0
                 * IL_0168: ldfld     class Verse.BuildableDef Verse.ThingDef::entityDefToBuild
                 * IL_016D: isinst    Verse.ThingDef
                 * IL_0172: brfalse.s IL_0188
                 * 
                 * IL_0174: ldloc.0
                 * IL_0175: ldfld     class Verse.BuildableDef Verse.ThingDef::entityDefToBuild
                 * IL_017A: isinst    Verse.ThingDef
                 * IL_017F: callvirt  instance bool Verse.ThingDef::get_EverTransmitsPower()
                 * IL_0184: brfalse.s IL_0188
                 * 
                 * IL_0186: ldc.i4.1
                 * IL_0187: ret
                 * 
                 * IL_0188: ldc.i4.0
                 * IL_0189: ret
                 */

                if ( firstPatchCompleted
                  && instructions[i - 11].Branches( out var label1 )
                  && instructions[i - 7].Branches( out var label2 )
                  && instructions[i - 5].LoadsField( Helpers.EntityDefToBuild_FI )
                  && instructions[i - 4].opcode == OpCodes.Isinst
                  && instructions[i - 3].Calls( Helpers.EverTransmitsPower_MI )
                  && instructions[i - 2].Branches( out var label3 )
                  && instructions[i - 1].LoadsConstant( 1L )
                  && instructions[i + 0].opcode == OpCodes.Ret )
                {
                    // we will want to skip IL_0188 and IL_189
                    // note that the branch instructions pointed to IL_0188, so we don't need 
                    // to remove the original labels.
                    i += 2;

                    // add labels so that fail conditions of conduit check point here
                    yield return new CodeInstruction( OpCodes.Ldloc_0 )
                    {
                        labels = new List<Label>( new[] {label1.Value, label2.Value, label3.Value} )
                    }; 
                    yield return new CodeInstruction( OpCodes.Ldloc_1 );
                    yield return new CodeInstruction( OpCodes.Call, Helpers.GasUserOverPipe_Blueprint_MI );

                    // not entirely sure why we can't just return the bool, but fuck it
                    next = generator.DefineLabel();
                    yield return new CodeInstruction( OpCodes.Brfalse, next );
                    yield return new CodeInstruction( OpCodes.Ldc_I4_1 );
                    yield return new CodeInstruction( OpCodes.Ret );
                    yield return new CodeInstruction( OpCodes.Ldc_I4_0 ){ labels = new List<Label>( new [] { next })};
                    yield return new CodeInstruction( OpCodes.Ret );
                }
            }
        }
	}
}