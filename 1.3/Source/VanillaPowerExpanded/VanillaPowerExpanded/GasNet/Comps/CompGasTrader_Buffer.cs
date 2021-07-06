// Copyright Karel Kroeze, 2020-2020.
// VanillaPowerExpanded/VanillaPowerExpanded/CompGasTrader_Buffer.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace GasNetwork
{
    public class CompGasTrader_Buffer : CompGasTrader
    {
        private static readonly StringBuilder sb = new StringBuilder();
        private                 bool          bufferOff;
        private                 float         desired;
        private                 float         stored;

        public new CompProperties_GasTrader_Buffer Props => props as CompProperties_GasTrader_Buffer;

        public bool  HasFuel => stored > 0;
        public float Fuel    => stored;

        public virtual Vector2 BarSize => new Vector2(1f, .16f);

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                desired = Props.maxBuffer;
            }
        }

        public void ConsumeFuel(float amount)
        {
            stored -= amount;
            if (stored < 0)
            {
                stored = 0;
            }
        }

        public override void CompTick()
        {
            base.CompTick();

            if (GasOn)
            {
                stored += GasConsumptionPerTick;
                if (stored > Props.maxBuffer)
                {
                    stored = Props.maxBuffer;
                }
            }

            if (parent.IsHashIntervalTick(GenTicks.TicksPerRealSecond))
            {
                if (bufferOff || stored >= desired)
                {
                    GasConsumption = 0;
                }
                else
                {
                    GasConsumption = Props.gasConsumption;
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            sb.Clear();
            sb.AppendLine(base.CompInspectStringExtra());
            sb.AppendLine(I18n.Stored(stored, Mathf.RoundToInt(Props.maxBuffer)));
            if (bufferOff)
            {
                sb.AppendLine(I18n.BufferOff);
            }
            else if (Math.Abs(desired - Props.maxBuffer) > Mathf.Epsilon)
            {
                sb.AppendLine(I18n.DesiredBuffer(desired, Props.maxBuffer));
            }

            return sb.ToString().Trim();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            // TODO: Add buffer toggle gizmo.
            // TODO: Add buffer size gizmo.
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref stored, "stored");
            Scribe_Values.Look(ref desired, "desired");
            Scribe_Values.Look(ref bufferOff, "bufferOff");
        }

        public override void PostDraw()
        {
            base.PostDraw();
            var barRequest = new GenDraw.FillableBarRequest
            {
                center      = parent.DrawPos + Vector3.up * .2f + Vector3.forward * .25f,
                size        = BarSize,
                fillPercent = stored / Props.maxBuffer,
                filledMat   = Resources.GasBarFilledMaterial,
                unfilledMat = Resources.GasBarUnfilledMaterial,
                margin      = .1f,
                rotation    = parent.Rotation.Rotated(RotationDirection.Clockwise)
            };
            GenDraw.DrawFillableBar(barRequest);
        }
    }

    public class Command_Buffer : Command
    {
    }

    public class CompProperties_GasTrader_Buffer : CompProperties_GasTrader
    {
        public float maxBuffer        = 125;
        public bool  showBufferSlider = true;
        public bool  showBufferToggle = true;
    }

    [HarmonyPatch(typeof(CompLaunchable), nameof(CompLaunchable.FuelingPortSourceHasAnyFuel), MethodType.Getter)]
    public static class CompLaunchable_FuelingPortSourceHasAnyFuel
    {
        public static bool Prefix(ref bool __result, CompLaunchable __instance)
        {
            __result = false;
            if (!__instance.Props.requireFuel)
            {
                __result = true;
            }
            else
            {
                var fuelingPort = __instance.FuelingPortSource;
                if (fuelingPort != null)
                {
                    if (fuelingPort.TryGetComp<CompRefuelable>(out var refuelable))
                    {
                        __result = refuelable.HasFuel;
                    }

                    if (fuelingPort.TryGetComp<CompGasTrader_Buffer>(out var buffer))
                    {
                        __result |= buffer.HasFuel;
                    }
                }
            }

            // the vanilla method assumes that CompRefuelable exists.
            return false;
        }
    }

    [HarmonyPatch(typeof(CompLaunchable), nameof(CompLaunchable.FuelingPortSourceFuel), MethodType.Getter)]
    public static class CompLaunchable_FuelingPortSourceFuel
    {
        public static bool Prefix(ref float __result, CompLaunchable __instance)
        {
            __result = 0f;
            if (__instance.ConnectedToFuelingPort)
            {
                var fuelingPort = __instance.FuelingPortSource;
                if (fuelingPort.TryGetComp<CompRefuelable>(out var refuelable))
                {
                    __result = refuelable.Fuel;
                }
                else if (fuelingPort.TryGetComp<CompGasTrader_Buffer>(out var buffer))
                {
                    __result = buffer.Fuel;
                }
            }

            // the vanilla method assumes that CompRefuelable exists.
            return false;
        }
    }

    [HarmonyPatch(typeof(CompLaunchable), nameof(CompLaunchable.TryLaunch))]
    public static class CompLaunchable_TryLaunch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> _instructions,
                                                              ILGenerator                  generator)
        {
            var tryGetComp_MI = AccessTools.Method(typeof(ThingCompUtility), nameof(ThingCompUtility.TryGetComp));
            var tryGetComp_CompRefuelable_MI = tryGetComp_MI.MakeGenericMethod(typeof(CompRefuelable));
            var tryGetComp_CompGasTrader_Buffer_MI = tryGetComp_MI.MakeGenericMethod(typeof(CompGasTrader_Buffer));
            var consumeFuel_CompRefuelable_MI =
                AccessTools.Method(typeof(CompRefuelable), nameof(CompRefuelable.ConsumeFuel));
            var consumeFuel_CompGasTrader_Buffer_MI =
                AccessTools.Method(typeof(CompGasTrader_Buffer), nameof(CompGasTrader_Buffer.ConsumeFuel));

            var instructions = _instructions.ToList();
            for (var i = 0; i < instructions.Count; i++)
            {
                var instruction = instructions[i];
                yield return instruction;

                if (instruction.Calls(tryGetComp_CompRefuelable_MI) &&
                    instructions[i + 2].Calls(consumeFuel_CompRefuelable_MI))
                {
                    // Add null check for vanilla CompRefuelable.ConsumeFuel call, because vanilla assumes compRefuelable exists on fuelingPortSource. 
                    var compRefuelableIsNull = generator.DefineLabel();
                    var refuelableFinished   = generator.DefineLabel();
                    yield return new CodeInstruction(OpCodes.Dup);                             // duplicate compRefuelable on the stack
                    yield return new CodeInstruction(OpCodes.Brfalse_S, compRefuelableIsNull); // break to beyond ConsumeFuel call if null
                    yield return instructions[i + 1];                                               // load fuel amount onto the stack
                    yield return instructions[i + 2];                                               // call CompRefuelable.ConsumeFuel
                    yield return new CodeInstruction(OpCodes.Br, refuelableFinished);          // stack is OK
                    yield return new CodeInstruction(OpCodes.Pop).WithLabels(compRefuelableIsNull);

                    // Add conditional call to CompGasTrader_Buffer.ConsumeFuel
                    var compBufferIsNull = generator.DefineLabel();
                    var bufferFinished   = generator.DefineLabel();
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 11).WithLabels(refuelableFinished); // (Building) FuelingPortSource
                    yield return new CodeInstruction(OpCodes.Call, tryGetComp_CompGasTrader_Buffer_MI);  // compBuffer
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, compBufferIsNull);
                    yield return new CodeInstruction(OpCodes.Ldloc_S,   4);                             // fuel amount
                    yield return new CodeInstruction(OpCodes.Callvirt,  consumeFuel_CompGasTrader_Buffer_MI); // consume fuel
                    yield return new CodeInstruction(OpCodes.Br,        bufferFinished);
                    yield return new CodeInstruction(OpCodes.Pop).WithLabels(compBufferIsNull);

                    // done!
                    yield return instructions[i + 3].WithLabels(bufferFinished);
                    i += 3;
                }


                //TODO; Add conditional call for CompGasTrader_Buffer.ConsumeFuel, if exists.
            }
        }
    }
}