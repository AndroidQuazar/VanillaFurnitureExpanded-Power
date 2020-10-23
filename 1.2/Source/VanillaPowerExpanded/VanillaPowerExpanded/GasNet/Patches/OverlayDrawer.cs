// OverlayDrawer.cs
// Copyright Karel Kroeze, 2020-2020

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace GasNetwork.Patches
{
    public static class GasOverlays
    {
        // add overlays for our stuff by introducing new enum values. Note that if anyone else has the same
        // idea, this will cause issues. It will also only work in places where it is expected (e.g. switch
        // statements need to account for it explicitly, or it will go through default).
        public static OverlayTypes GasOff = (OverlayTypes) 512;
        public static OverlayTypes NeedsGas = (OverlayTypes) 1024;
    }

    public static partial class Helpers
    {
        public static MethodInfo RenderPowerOffOverlay_MI =
            AccessTools.Method( typeof( OverlayDrawer ), "RenderPowerOffOverlay" );

        public static MethodInfo RenderGasOffOverlay_MI =
            AccessTools.Method( typeof( Helpers ), nameof( RenderGasOffOverlay ) );
        public static void RenderGasOffOverlay( OverlayDrawer drawer, Thing thing )
        {
            RenderPulsingOverlay_MI.Invoke( drawer, new object[] {thing, Resources.GasOff, 3, true} );
        }

        public static MethodInfo RenderNeedsGasOverlay_MI =
            AccessTools.Method( typeof( Helpers ), nameof(RenderNeedsGasOverlay) );

        public static void RenderNeedsGasOverlay( OverlayDrawer drawer, Thing thing )
        {
            RenderPulsingOverlay_MI.Invoke( drawer, new object[] {thing, Resources.NeedsGas, 2, true} );
        }

        private static MethodInfo RenderPulsingOverlay_MI =
            AccessTools.Method( typeof( OverlayDrawer ), "RenderPulsingOverlay",
                                new[] { typeof( Thing ), typeof( Material ), typeof( int ), typeof( bool ) } );
    }

    [HarmonyPatch( typeof( OverlayDrawer ), nameof( OverlayDrawer.DrawAllOverlays ))]
    public static class OverlayDrawer_DrawAllOverlays
    {
        enum State
        {
            Start,
            EnumDone,
            RenderDone
        }

        public static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> _instructions, ILGenerator generator )
        {
            var instructions = _instructions.ToList();
            var state = State.Start;
            
            /**
             * We'll want to replace all instances of `OverlayTypes.NeedsPower | OverlayTypes.PowerOff`
             * with one that also includes `NeedsGas | GasOff`, which do not yet exist. Enums are treated
             * as ints (binary flags, actually), so we can just swap in a different integer.
             *
             */

            for ( int i = 0; i < instructions.Count; i++)
            {
                var instruction = instructions[i];

                /** 
                * Here's to hoping that this method never starts hardcoding a reference to a 3rd list element!
                * ( we assume that the first encounter of loading the integer 3 is the one we want.
                */
                if ( state == State.Start
                  && instruction.LoadsConstant( 3L ) )
                {
                    state = State.EnumDone;
                    yield return new CodeInstruction( OpCodes.Ldc_I4,
                                                        (int) ( OverlayTypes.NeedsPower |
                                                                OverlayTypes.PowerOff   |
                                                                GasOverlays.GasOff      |
                                                                GasOverlays.NeedsGas ) ) 
                        // note that the original instruction was the target of at least one branch, and has a label.
                        {labels = instruction.labels};
                    

                    continue; // do not yield the current instruction, as we're replacing it.
                }

                /**
                 * The below block checks the overlay type (in this case PowerOff), and if it matches calls the
                 * corresponding render function. We will do the same here, for our own overlay types.
                 *
                 * IL_00D1: ldloc.3
                 * IL_00D2: ldc.i4.2
                 * IL_00D3: and
                 * IL_00D4: brfalse.s IL_00DD
                 * 
                 * IL_00D6: ldarg.0
                 * IL_00D7: ldloc.2
                 * IL_00D8: call      instance void RimWorld.OverlayDrawer::RenderPowerOffOverlay(class Verse.Thing)
                 *  
                 * IL_00DD: ldloc.3
                 */
                if ( state == State.EnumDone
                     && instructions[i-2].Calls( Helpers.RenderPowerOffOverlay_MI )
                     && instructions[i-1].opcode == OpCodes.Ldloc_3 )
                {
                    // note that we're borrowing IL_00DD's ldloc.3 here, so that we don't have to reroute the previous
                    // blocks' labels. We'll need to put this back on the stack before the next block. 
                    yield return new CodeInstruction( OpCodes.Ldc_I4, (int)GasOverlays.GasOff ); // push (int)GasOff [512]
                    yield return new CodeInstruction( OpCodes.And );                        // pop  loc3, pop 512, push loc3 & 512
                    var next = generator.DefineLabel();
                    yield return new CodeInstruction( OpCodes.Brfalse, next );              // jump to next if not 512
                    yield return new CodeInstruction( OpCodes.Ldarg_0 );                    // instance of this overlay drawer
                    yield return new CodeInstruction( OpCodes.Ldloc_2 );                    // thing it is drawn on
                    yield return new CodeInstruction( OpCodes.Call, Helpers.RenderGasOffOverlay_MI ); // render overlay
                    yield return new CodeInstruction( OpCodes.Ldloc_3 ) {labels = new List<Label>( new[] {next} )}; // next

                    // same again for NeedsGas
                    yield return new CodeInstruction( OpCodes.Ldc_I4, (int)GasOverlays.NeedsGas );
                    yield return new CodeInstruction( OpCodes.And );
                    next = generator.DefineLabel();
                    yield return new CodeInstruction( OpCodes.Brfalse, next );
                    yield return new CodeInstruction( OpCodes.Ldarg_0 ); 
                    yield return new CodeInstruction( OpCodes.Ldloc_2 ); 
                    yield return new CodeInstruction( OpCodes.Call, Helpers.RenderNeedsGasOverlay_MI );
                    yield return new CodeInstruction( OpCodes.Ldloc_3 ) {labels = new List<Label>( new[] {next} )};

                    // done!
                    state = State.RenderDone;
                }

                // yield original instructions
                yield return instruction;
            }
        }
    }
}