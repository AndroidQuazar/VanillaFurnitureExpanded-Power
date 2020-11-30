// Copyright Karel Kroeze, 2020-2020.
// VanillaPowerExpanded/VanillaPowerExpanded/MapComponent_GasDangerGrid.cs

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace GasNetwork
{
    public class MapComponent_GasDanger : MapComponent
    {
        public const int CHECK_INTERVAL = 300;

        protected Dictionary<int, Pair<Danger, int>> _cache = new Dictionary<int, Pair<Danger, int>>();

        private static Map[] maps = new Map[20];
        private static MapComponent_GasDanger[] comps = new MapComponent_GasDanger[20];

        private int[] grid;
        private List<Gas_Spreading>[] things;
        private List<Gas_Spreading>[] cumulativeThings;

        public MapComponent_GasDanger(Map map) : base(map)
        {
            grid = new int[map.cellIndices.NumGridCells];
            things = new List<Gas_Spreading>[map.cellIndices.NumGridCells];
            cumulativeThings = new List<Gas_Spreading>[map.cellIndices.NumGridCells];
            for (int i = 0; i < cumulativeThings.Length; i++)
            {
                cumulativeThings[i] = new List<Gas_Spreading>();
                things[i] = new List<Gas_Spreading>();
            }
            if (map.Index >= 0 && map.Index < 20)
            {
                maps[map.Index] = map;
                comps[map.Index] = this;
            }
        }

        // Used to get MapComponent_GasDanger for the given map
        public static MapComponent_GasDanger GetCachedComp(Map map)
        {
            var index = map.Index;
            if (index >= 0 && index < 20)
            {
                if (maps[index] == map) return comps[index];

                maps[index] = map;
                return comps[index] = map.GetComponent<MapComponent_GasDanger>();
            }
            Log.Warning($"MapComponent_GasDanger not found for map with strange index of {index}");
            return map.GetComponent<MapComponent_GasDanger>();
        }

        public List<Gas_Spreading> GasesAt(IntVec3 cell)
        {
            return GasesAt(map.cellIndices.CellToIndex(cell));
        }

        public List<Gas_Spreading> GasesAt(int index)
        {
            return cumulativeThings[index];
        }

        public void RegisterAt(Gas_Spreading gas, IntVec3 cell)
        {
            RegisterAt(gas, map.cellIndices.CellToIndex(cell));
        }

        public void RegisterAt(Gas_Spreading gas, int index)
        {
            var c = map.cellIndices.IndexToCell(index);
            things[index].Add(gas);
            foreach (var offset in GenAdj.AdjacentCellsAndInside)
            {
                var cell = c + offset;
                var center = cell.Equals(c);
                if (!cell.InBounds(map))
                {
                    continue;
                }
                var i = map.cellIndices.CellToIndex(c + offset);
                cumulativeThings[i].Add(gas);
                grid[i] = cumulativeThings[index].Count;
            }
        }

        public void Deregister(Gas_Spreading gas, IntVec3 cell)
        {
            Deregister(gas, map.cellIndices.CellToIndex(cell));
        }

        public void Deregister(Gas_Spreading gas, int index)
        {
            var c = map.cellIndices.IndexToCell(index);
            things[index].RemoveAll(g => g == gas);
            foreach (var offset in GenAdj.AdjacentCellsAndInside)
            {
                var cell = c + offset;
                var center = cell.Equals(c);
                if (!cell.InBounds(map))
                {
                    continue;
                }
                var i = map.cellIndices.CellToIndex(c + offset);
                cumulativeThings[i].RemoveAll(g => g == gas);
                grid[i] = cumulativeThings[index].Count;
            }
        }

        public Danger DangerIn(Region region)
        {
            if (_cache.TryGetValue(region.id, out var store) && GenTicks.TicksGame - store.Second < CHECK_INTERVAL)
            {
                return store.First;
            }

            // calculate danger, some if any toxic/flammable gas in region,
            // deadly if region has > 25% density of dangerous gasses.
            //var gasses = region.ListerThings.AllThings.OfType<Gas_Spreading>().Where(g => g.Flammable || g.Toxic);
            //var danger = Danger.None;
            //
            //if (gasses.Any())
            //{
            //    var totalDensity = gasses.Sum(g => g.Density) / region.CellCount;
            //    danger = totalDensity > .2 ? Danger.Deadly : totalDensity > .05 ? Danger.Some : Danger.None;
            //}

            var danger = Danger.None;
            var totalDensity = 0f;
            var cells = region.Cells;
            foreach (var cell in cells)
            {
                var index = map.cellIndices.CellToIndex(cell);
                if (things[index].Count > 0)
                {
                    totalDensity += things[index].Sum(g => g.Density) / cells.Count();
                }
            }
            danger = totalDensity > .2 ? Danger.Deadly : totalDensity > .05 ? Danger.Some : Danger.None;
            _cache[region.id] = new Pair<Danger, int>(danger, GenTicks.TicksGame);
            return danger;
        }

#if DEBUG_DANGER
        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();
            foreach (var danger in _danger)
            {
                if (danger.Value > Danger.None)
                {
                    GenDraw.DrawFieldEdges(danger.Key.Cells.ToList(),
                                           danger.Value == Danger.Deadly ? Color.red : Color.yellow);
                }
            }
        }
#endif
    }
}