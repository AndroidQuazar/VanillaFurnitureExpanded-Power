using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VanillaPowerExpanded
{
    public static class PipeConnectionMaker
    {
        public static void ConnectAllConnectorsToTransmitter(CompPipe newTransmitter)
        {
            foreach (CompPipe compPipe in PipeConnectionMaker.PotentialConnectorsForTransmitter(newTransmitter))
            {
                if (compPipe.connectParent == null)
                {
                    compPipe.ConnectToTransmitter(newTransmitter, false);
                }
            }
        }

        public static void DisconnectAllFromTransmitterAndSetWantConnect(CompPipe deadPc, Map map)
        {
            if (deadPc.connectChildren == null)
            {
                return;
            }
            for (int i = 0; i < deadPc.connectChildren.Count; i++)
            {
                CompPipe compPipe = deadPc.connectChildren[i];
                compPipe.connectParent = null;
                CompPipeTrader compPipeTrader = compPipe as CompPipeTrader;
                if (compPipeTrader != null)
                {
                    compPipeTrader.PowerOn = false;
                }
                map.GetComponent<PipeMapComponent>().Notify_ConnectorWantsConnect(compPipe);
            }
        }

        public static void TryConnectToAnyPowerNet(CompPipe pc, List<GasPipeNet> disallowedNets = null)
        {
            if (pc.connectParent != null)
            {
                return;
            }
            if (!pc.parent.Spawned)
            {
                return;
            }
            CompPipe compPipe = BestTransmitterForConnector(pc.parent.Position, pc.parent.Map, disallowedNets);
            if (compPipe != null)
            {
                pc.ConnectToTransmitter(compPipe, false);
            }
            else
            {
                pc.connectParent = null;
            }
        }

        public static void DisconnectFromPowerNet(CompPipe pc)
        {
            if (pc.connectParent == null)
            {
                return;
            }
            if (pc.GasPipeNet != null)
            {
                pc.GasPipeNet.DeregisterConnector(pc);
            }
            if (pc.connectParent.connectChildren != null)
            {
                pc.connectParent.connectChildren.Remove(pc);
                if (pc.connectParent.connectChildren.Count == 0)
                {
                    pc.connectParent.connectChildren = null;
                }
            }
            pc.connectParent = null;
        }

        private static IEnumerable<CompPipe> PotentialConnectorsForTransmitter(CompPipe b)
        {
            if (!b.parent.Spawned)
            {
                Log.Warning("Can't check potential connectors for " + b + " because it's unspawned.", false);
                yield break;
            }
            CellRect rect = b.parent.OccupiedRect().ExpandedBy(6).ClipInsideMap(b.parent.Map);
            for (int z = rect.minZ; z <= rect.maxZ; z++)
            {
                for (int x = rect.minX; x <= rect.maxX; x++)
                {
                    IntVec3 c = new IntVec3(x, 0, z);
                    List<Thing> thingList = b.parent.Map.thingGrid.ThingsListAt(c);
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        if (ConnectToGas(thingList[i]))
                        {
                            yield return ((Building)thingList[i]).GetComp<CompPipe>();
                        }
                    }
                }
            }
            yield break;
        }

        public static bool ConnectToGas(Thing thing)
        {
            if (EverTransmitsGas(thing))
            {
                return false;
            }

            for (int i = 0; i < thing.def.comps.Count; i++)
                {
                    if (thing.def.comps[i].compClass == typeof(CompPipeTank))
                    {
                        return true;
                    }
                    if (thing.def.comps[i].compClass == typeof(CompPipeTrader))
                    {
                        return true;
                    }
                }
                return false;
            
        }

        public static bool EverTransmitsGas(Thing thing)
        {
           
                for (int i = 0; i < thing.def.comps.Count; i++)
                {
                    CompProperties_Pipe compProperties_Pipe = thing.def.comps[i] as CompProperties_Pipe;
                    if (compProperties_Pipe != null && compProperties_Pipe.transmitsGas)
                    {
                        return true;
                    }
                }
                return false;
            
        }

        public static CompPipe BestTransmitterForConnector(IntVec3 connectorPos, Map map, List<GasPipeNet> disallowedNets = null)
        {
            CellRect cellRect = CellRect.SingleCell(connectorPos).ExpandedBy(1).ClipInsideMap(map);
            cellRect.ClipInsideMap(map);
            float num = 999999f;
            CompPipe result = null;
            for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
            {
                for (int j = cellRect.minX; j <= cellRect.maxX; j++)
                {
                    IntVec3 c = new IntVec3(j, 0, i);
                    Building transmitter = c.GetTransmitter(map);
                    if (transmitter != null && !transmitter.Destroyed)
                    {
                        CompPipe pipeComp = transmitter.GetComp<CompPipe>();
                        if (pipeComp != null && pipeComp.TransmitsGasNow && (transmitter.def.building == null || transmitter.def.building.allowWireConnection))
                        {
                            if (disallowedNets == null || !disallowedNets.Contains(pipeComp.transNet))
                            {
                                float num2 = (float)(transmitter.Position - connectorPos).LengthHorizontalSquared;
                                if (num2 < num)
                                {
                                    num = num2;
                                    result = pipeComp;
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        private const int ConnectMaxDist = 6;
    }
}

