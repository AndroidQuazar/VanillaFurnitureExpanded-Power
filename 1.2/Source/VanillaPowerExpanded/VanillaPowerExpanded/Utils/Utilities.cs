// Copyright Karel Kroeze, 2020-2020.
// VanillaPowerExpanded/VanillaPowerExpanded/Utilities.cs

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace GasNetwork
{
    public static class Utilities
    {
        public static IEnumerable<Building> AllBuildingsColonistWithComp<T>(this ListerBuildings lister)
            where T : ThingComp
        {
            return lister.allBuildingsColonist.Where(b => b.GetComp<T>() != null);
        }

        // todo: see about caching comps - what is a sane cache invalidation?
        public static IEnumerable<T> TryGetComps<T>(this IEnumerable<Thing> things) where T : ThingComp
        {
            return things.OfType<ThingWithComps>().GetComps<T>();
        }

        public static IEnumerable<T> GetComps<T>(this IEnumerable<ThingWithComps> things) where T : ThingComp
        {
            return things.Select(t => t.GetComp<T>()).Where(c => c != null);
        }

        public static int HashOffsetTicks(this GasNet network)
        {
            return Find.TickManager.TicksGame + network.GetHashCode().HashOffset();
        }

        public static bool IsHashIntervalTick(this GasNet network, int interval)
        {
            return network.HashOffsetTicks() % interval == 0;
        }

        public static List<CompGas> GetAdjacentGasComps(this CompGas gas, Map map, bool includeSelf = false)
        {
            var cells = GenAdj.CellsAdjacentCardinal(gas.parent).ToList();
            if (includeSelf) cells.AddRange(gas.parent.OccupiedRect().Cells);
            return cells.SelectMany(c => c.GetThingList(map))
                .Distinct()
                .TryGetComps<CompGas>()
                .ToList();
        }

        public static List<CompGas> GetAdjacentGasComps(this CompGas gas, bool includeSelf = false)
        {
            if (gas.parent.Map == null)
                Log.Error(
                    $"cannot get map for {gas.parent.Label}, it is {(gas.parent.Spawned ? "" : "NOT")} spawned.");
            return GetAdjacentGasComps(gas, gas.parent.Map, includeSelf);
        }

        public static bool TryGetComp<T>(this ThingWithComps thing, out T comp) where T : ThingComp
        {
            comp = thing.GetComp<T>();
            return comp != null;
        }

        public static bool TryGetComp<T>(this Thing thing, out T comp) where T : ThingComp
        {
            if (thing is ThingWithComps twc)
                return TryGetComp(twc, out comp);
            comp = null;
            return false;
        }

        public static bool WantsAndCanBeOn(this ThingWithComps thing)
        {
            if (thing.TryGetComp<CompBreakdownable>(out var breakdownable))
                if (breakdownable.BrokenDown)
                    return false;

            if (thing.TryGetComp<CompFlickable>(out var flickable))
                if (!flickable.SwitchIsOn)
                    return false;

            if (thing.TryGetComp<CompSchedule>(out var schedule))
                if (!schedule.Allowed)
                    return false;

            // no reason not to want to be on.
            return true;
        }

        public static bool HasPower(this ThingWithComps thing)
        {
            // powered by electricity, and no juice
            if (thing.TryGetComp<CompPowerTrader>(out var power)
                && !power.PowerOn
                && power.Props.basePowerConsumption > 0)
                return false;
            return true;
        }

        public static bool EverTransmitsGas(this ThingDef thingDef)
        {
            return thingDef.comps?.Any(c => c is CompProperties_Gas) ?? false;
        }

        public static bool BuildingFrameOrBlueprintEverTransmitsGas(this ThingDef def)
        {
            return def.EverTransmitsGas()
                   || def.entityDefToBuild is ThingDef defToBuild
                   && defToBuild.EverTransmitsGas();
        }

        public static bool IsDefFrameOfBlueprintFor(this ThingDef def, Def other)
        {
            return def != other && def.entityDefToBuild != other;
        }
    }
}