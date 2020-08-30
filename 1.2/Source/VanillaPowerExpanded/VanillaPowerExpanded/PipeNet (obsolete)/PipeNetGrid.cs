﻿using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VanillaPowerExpanded
{
    public class PipeNetGrid : MapComponent
    {
        new public Map map;

        private GasPipeNet[] netGrid;

        private Dictionary<GasPipeNet, List<IntVec3>> powerNetCells = new Dictionary<GasPipeNet, List<IntVec3>>();

        public PipeNetGrid(Map map) : base(map)
        {
            this.map = map;
            this.netGrid = new GasPipeNet[map.cellIndices.NumGridCells];

        }

     

        public GasPipeNet TransmittedPowerNetAt(IntVec3 c)
        {
            return this.netGrid[this.map.cellIndices.CellToIndex(c)];
        }

        public void Notify_PowerNetCreated(GasPipeNet newNet)
        {
            if (this.powerNetCells.ContainsKey(newNet))
            {
                Log.Warning("Net " + newNet + " is already registered in PowerNetGrid.", false);
                this.powerNetCells.Remove(newNet);
            }
            List<IntVec3> list = new List<IntVec3>();
            this.powerNetCells.Add(newNet, list);
            for (int i = 0; i < newNet.transmitters.Count; i++)
            {
                CellRect cellRect = newNet.transmitters[i].parent.OccupiedRect();
                for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
                {
                    for (int k = cellRect.minX; k <= cellRect.maxX; k++)
                    {
                        int num = this.map.cellIndices.CellToIndex(k, j);
                        if (this.netGrid[num] != null)
                        {
                            /*Log.Warning(string.Concat(new object[]
                            {
                                "Two power nets on the same cell (",
                                k,
                                ", ",
                                j,
                                "). First transmitters: ",
                                newNet.transmitters[0].parent.LabelCap,
                                " and ",
                                (!this.netGrid[num].transmitters.NullOrEmpty<CompPipe>()) ? this.netGrid[num].transmitters[0].parent.LabelCap : "[none]",
                                "."
                            }), false);*/
                        }
                        this.netGrid[num] = newNet;
                        list.Add(new IntVec3(k, 0, j));
                    }
                }
            }
        }

        public void Notify_PowerNetDeleted(GasPipeNet deadNet)
        {
            List<IntVec3> list;
            if (!this.powerNetCells.TryGetValue(deadNet, out list))
            {
                Log.Warning("Net " + deadNet + " does not exist in PowerNetGrid's dictionary.", false);
                return;
            }
            for (int i = 0; i < list.Count; i++)
            {
                int num = this.map.cellIndices.CellToIndex(list[i]);
                if (this.netGrid[num] == deadNet)
                {
                    this.netGrid[num] = null;
                }
                else
                {
                    //Log.Warning("Multiple nets on the same cell " + list[i] + ". This is probably a result of an earlier error.", false);
                }
            }
            this.powerNetCells.Remove(deadNet);
        }

        public void DrawDebugPowerNetGrid()
        {
            if (!DebugViewSettings.drawPowerNetGrid)
            {
                return;
            }
            if (Current.ProgramState != ProgramState.Playing)
            {
                return;
            }
            if (this.map != Find.CurrentMap)
            {
                return;
            }
            Rand.PushState();
            foreach (IntVec3 current in Find.CameraDriver.CurrentViewRect.ClipInsideMap(this.map))
            {
                GasPipeNet powerNet = this.netGrid[this.map.cellIndices.CellToIndex(current)];
                if (powerNet != null)
                {
                    Rand.Seed = powerNet.GetHashCode();
                    CellRenderer.RenderCell(current, Rand.Value);
                }
            }
            Rand.PopState();
        }
    }
}
