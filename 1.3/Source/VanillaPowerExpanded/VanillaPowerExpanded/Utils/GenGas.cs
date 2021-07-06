// Copyright Karel Kroeze, 2020-2020.
// VanillaPowerExpanded/VanillaPowerExpanded/GenGas.cs

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace GasNetwork
{
    public static class GenGas
    {
        public const  float            DEFAULT_GAS_RADIUS = 5.4f;
        public static Queue<IntVec3>   _queue             = new Queue<IntVec3>();
        public static HashSet<IntVec3> _cells             = new HashSet<IntVec3>();
        public static HashSet<IntVec3> _checked           = new HashSet<IntVec3>();

        public static List<IntVec3> GetGasVentArea(IntVec3 ventPos, Map map, float radius)
        {
            // simple flood fill implementation. 
            // Note that we use this over GenRadial because that set of functions does 
            // not and cannot take blockers into account (e.g. walls).
            // A FIFO flood fill will produce what is essentially a diamond pattern, if 
            // unconstrained by walls. Ideally, we'd want to use a priority queue for 
            // distance from the center, but that becomes tricky in a constrained sit-
            // uation, as you'd have to take line-of-sight into account. 

            // return number of cells based on radius.
            var cellCount = GenRadial.NumCellsInRadius(radius);
            var room      = ventPos.GetRoom(map);

            // prep data structures
            _queue.Clear();
            _cells.Clear();
            _checked.Clear();

            // start the process
            _queue.Enqueue(ventPos);
            _checked.Add(ventPos);
            while (_queue.Count > 0 && _cells.Count < cellCount)
            {
                var cell = _queue.Dequeue();
                _cells.Add(cell);

                foreach (var adj in GenAdjFast.AdjacentCellsCardinal(cell)
                                              .Where(adj => !adj.Impassable(map)
                                                         && !_checked.Contains(adj)
                                                         && adj.GetRoom(map) == room))
                {
                    _queue.Enqueue(adj);
                    _checked.Add(adj);
                }
            }

            return _cells.ToList();
        }

        public static IntVec3 VentingPosition(ThingWithComps parent)
        {
            return VentingPosition(parent.PositionHeld, parent.Rotation);
        }

        public static IntVec3 VentingPosition(IntVec3 pos, Rot4 rot)
        {
            return pos + IntVec3.North.RotatedBy(rot);
        }

        public static float AddGas(IntVec3  pos,
                                   Map      map,
                                   ThingDef gasDef,
                                   float    amount              = -1,
                                   bool     spread              = true,
                                   bool     allowOverSaturation = false)
        {
            // check if there is already a gas here, create if needed.
            var startingAmount = amount;
            var gas            = pos.GetFirstThing(map, gasDef);
            if (gas == null)
            {
                gas = ThingMaker.MakeThing(gasDef) as Gas;
                GenSpawn.Spawn(gas, pos, map);
            }

            // add specific amount if relevant
            if (gas is Gas_Spreading spreadingGas)
            {
                if (amount < 0)
                {
                    Log.Error(
                        $"Cannot add {amount} to {spreadingGas} at {spreadingGas.Position}. AddGas on a spreading gas should have a positive amount of gas.");
                    spreadingGas.Destroy();
                    return amount;
                }

                if (spread)
                {
                    AddGas_FloodFill(pos, map, gasDef, ref amount);
                }
                else
                {
                    AddGas_Cell(pos, map, gasDef, ref amount, allowOverSaturation);
                }
            }

            return startingAmount - amount;
        }

        private static void AddGas_FloodFill(IntVec3 pos, Map map, ThingDef gasDef, ref float amount)
        {
            // flood-fill gas into target cell and surrounding area
            // prep data structures
            var room = pos.GetRoom(map);
            _queue.Clear();
            _checked.Clear();

            // start the process
            _queue.Enqueue(pos);
            _checked.Add(pos);
            while (_queue.Count > 0 && amount > Mathf.Epsilon)
            {
                var cell = _queue.Dequeue();
                AddGas_Cell(cell, map, gasDef, ref amount);
                foreach (var adj in GenAdjFast.AdjacentCellsCardinal(cell)
                                              .Where(adj => !_checked.Contains(adj)
                                                         && !adj.Impassable(map)
                                                         && adj.GetRoom(map) == room))
                {
                    _queue.Enqueue(adj);
                    _checked.Add(adj);
                }
            }
        }

        private static void AddGas_Cell(IntVec3   cell,
                                        Map       map,
                                        ThingDef  gasDef,
                                        ref float amount,
                                        bool      allowOverSaturation = false)
        {
            var spreadingGas = cell.GetFirstThing(map, gasDef) as Gas_Spreading;
            if (spreadingGas == null)
            {
                spreadingGas = ThingMaker.MakeThing(gasDef) as Gas_Spreading;
                GenSpawn.Spawn(spreadingGas, cell, map);
            }

            spreadingGas.AddGas(ref amount, allowOverSaturation);
        }
    }
}