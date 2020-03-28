using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VanillaPowerExpanded
{
    public class PipeMapComponent : MapComponent
    {
        public enum DelayedActionType
        {
            RegisterTransmitter,
            DeregisterTransmitter,
            RegisterConnector,
            DeregisterConnector
        }

        public struct DelayedAction
        {
            public DelayedActionType type;

            public CompPipe compPipe;

            public IntVec3 position;

            public Rot4 rotation;

            public DelayedAction(DelayedActionType type, CompPipe compPipe)
            {
                this.type = type;
                this.compPipe = compPipe;
                this.position = compPipe.parent.Position;
                this.rotation = compPipe.parent.Rotation;
            }
        }

       

        public PipeMapComponent(Map map) : base(map)
        {

        }


        private List<GasPipeNet> allNets = new List<GasPipeNet>();

        private List<DelayedAction> delayedActions = new List<DelayedAction>();

        public List<GasPipeNet> AllNetsListForReading
        {
            get
            {
                return this.allNets;
            }
        }

        public override void FinalizeInit()
        {

            base.FinalizeInit();
          
        }

        public override void MapComponentTick()
        {
            this.UpdatePipeNetsAndConnections_First();
            this.PowerNetsTick();
            base.MapComponentTick();
            
        }

        public void Notify_TransmitterSpawned(CompPipe newTransmitter)
        {
           
            this.delayedActions.Add(new DelayedAction(DelayedActionType.RegisterTransmitter, newTransmitter));
            this.NotifyDrawersForWireUpdate(newTransmitter.parent.Position);
        }

        public void Notify_TransmitterDespawned(CompPipe oldTransmitter)
        {
            this.delayedActions.Add(new DelayedAction(DelayedActionType.DeregisterTransmitter, oldTransmitter));
            this.NotifyDrawersForWireUpdate(oldTransmitter.parent.Position);
        }

        public void Notfiy_TransmitterTransmitsPowerNowChanged(CompPipe transmitter)
        {
            if (!transmitter.parent.Spawned)
            {
                return;
            }
            this.delayedActions.Add(new DelayedAction(DelayedActionType.DeregisterTransmitter, transmitter));
            this.delayedActions.Add(new DelayedAction(DelayedActionType.RegisterTransmitter, transmitter));
            this.NotifyDrawersForWireUpdate(transmitter.parent.Position);
        }

        public void Notify_ConnectorWantsConnect(CompPipe wantingCon)
        {
            if (Scribe.mode == LoadSaveMode.Inactive && !this.HasRegisterConnectorDuplicate(wantingCon))
            {
                this.delayedActions.Add(new DelayedAction(DelayedActionType.RegisterConnector, wantingCon));
            }
            this.NotifyDrawersForWireUpdate(wantingCon.parent.Position);
        }

        public void Notify_ConnectorDespawned(CompPipe oldCon)
        {
            this.delayedActions.Add(new DelayedAction(DelayedActionType.DeregisterConnector, oldCon));
            this.NotifyDrawersForWireUpdate(oldCon.parent.Position);
        }

        public void NotifyDrawersForWireUpdate(IntVec3 root)
        {
            //this.map.mapDrawer.MapMeshDirty(root, MapMeshFlag.Things, true, false);
            //this.map.mapDrawer.MapMeshDirty(root, MapMeshFlag.PowerGrid, true, false);
        }

        public void RegisterPowerNet(GasPipeNet newNet)
        {
            this.allNets.Add(newNet);
            newNet.pipeNetManager = this;
            this.map.GetComponent<PipeNetGrid>().Notify_PowerNetCreated(newNet);
            PipeNetMaker.UpdateVisualLinkagesFor(newNet);
        }

        public void DeletePowerNet(GasPipeNet oldNet)
        {
            this.allNets.Remove(oldNet);
            this.map.GetComponent<PipeNetGrid>().Notify_PowerNetDeleted(oldNet);
        }

        public void PowerNetsTick()
        {
            for (int i = 0; i < this.allNets.Count; i++)
            {
                this.allNets[i].PowerNetTick();
            }
        }

        public void UpdatePipeNetsAndConnections_First()
        {
           
            int count = this.delayedActions.Count;
            
            for (int i = 0; i < count; i++)
            {
                DelayedAction delayedAction = this.delayedActions[i];
                DelayedActionType type = this.delayedActions[i].type;
                if (type != DelayedActionType.RegisterTransmitter)
                {
                    if (type == DelayedActionType.DeregisterTransmitter)
                    {
                        this.TryDestroyNetAt(delayedAction.position);
                        PipeConnectionMaker.DisconnectAllFromTransmitterAndSetWantConnect(delayedAction.compPipe, this.map);
                        delayedAction.compPipe.ResetPowerVars();
                    }
                }
                else if (delayedAction.position == delayedAction.compPipe.parent.Position)
                {
                    ThingWithComps parent = delayedAction.compPipe.parent;
                    if (this.map.GetComponent<PipeNetGrid>().TransmittedPowerNetAt(parent.Position) != null)
                    {
                        Log.Warning(string.Concat(new object[]
                        {
                            "Tried to register trasmitter ",
                            parent,
                            " at ",
                            parent.Position,
                            ", but there is already a power net here. There can't be two transmitters on the same cell."
                        }), false);
                    }
                    delayedAction.compPipe.SetUpPowerVars();
                    foreach (IntVec3 current in GenAdj.CellsAdjacentCardinal(parent))
                    {
                        this.TryDestroyNetAt(current);
                    }
                }
            }
            for (int j = 0; j < count; j++)
            {
                DelayedAction delayedAction2 = this.delayedActions[j];
                if ((delayedAction2.type == DelayedActionType.RegisterTransmitter && delayedAction2.position == delayedAction2.compPipe.parent.Position) || delayedAction2.type == DelayedActionType.DeregisterTransmitter)
                {
                    this.TryCreateNetAt(delayedAction2.position);
                    foreach (IntVec3 current2 in GenAdj.CellsAdjacentCardinal(delayedAction2.position, delayedAction2.rotation, delayedAction2.compPipe.parent.def.size))
                    {
                        this.TryCreateNetAt(current2);
                    }
                }
            }
            for (int k = 0; k < count; k++)
            {
                DelayedAction delayedAction3 = this.delayedActions[k];
                DelayedActionType type2 = this.delayedActions[k].type;
                if (type2 != DelayedActionType.RegisterConnector)
                {
                    if (type2 == DelayedActionType.DeregisterConnector)
                    {
                        PipeConnectionMaker.DisconnectFromPowerNet(delayedAction3.compPipe);
                        delayedAction3.compPipe.ResetPowerVars();
                    }
                }
                else if (delayedAction3.position == delayedAction3.compPipe.parent.Position)
                {
                    delayedAction3.compPipe.SetUpPowerVars();
                    PipeConnectionMaker.TryConnectToAnyPowerNet(delayedAction3.compPipe, null);
                }
            }
            this.delayedActions.RemoveRange(0, count);
            if (DebugViewSettings.drawPower)
            {
                this.DrawDebugPowerNets();
            }
        }

        private bool HasRegisterConnectorDuplicate(CompPipe compPipe)
        {
            for (int i = this.delayedActions.Count - 1; i >= 0; i--)
            {
                if (this.delayedActions[i].compPipe == compPipe)
                {
                    if (this.delayedActions[i].type == DelayedActionType.DeregisterConnector)
                    {
                        return false;
                    }
                    if (this.delayedActions[i].type == DelayedActionType.RegisterConnector)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void TryCreateNetAt(IntVec3 cell)
        {
            
            if (!cell.InBounds(this.map))
            {
                return;
            }
            if (this.map.GetComponent<PipeNetGrid>().TransmittedPowerNetAt(cell) == null)
            {
                Building transmitter = GetPipeTransmitter(cell,this.map);
                //Log.Message(transmitter.ToString());
                if (transmitter != null && GetPipeTransmission(transmitter))
                {
                    GasPipeNet pipeNet = PipeNetMaker.NewPowerNetStartingFrom(transmitter);
                    this.RegisterPowerNet(pipeNet);
                    for (int i = 0; i < pipeNet.transmitters.Count; i++)
                    {
                        PipeConnectionMaker.ConnectAllConnectorsToTransmitter(pipeNet.transmitters[i]);
                    }
                }
            }
        }

        public Building GetPipeTransmitter(IntVec3 c, Map map)
        {
            List<Thing> list = map.thingGrid.ThingsListAt(c);
            for (int i = 0; i < list.Count; i++)
            {
                if (EverTransmitsPipe(list[i].def))
                {
                    return (Building)list[i];
                }
            }
            return null;
        }

        public bool GetPipeTransmission(Building transmitter)
        {
            CompPipe pipeComp = transmitter.GetComp<CompPipe>();
            return pipeComp != null && pipeComp.Props.transmitsGas;
        }

        public bool EverTransmitsPipe(ThingDef thingdef)
        {
           
                for (int i = 0; i < thingdef.comps.Count; i++)
                {
                    CompProperties_Pipe compProperties_Pipe = thingdef.comps[i] as CompProperties_Pipe;
                    if (compProperties_Pipe != null && compProperties_Pipe.transmitsGas)
                    {
                        return true;
                    }
                }
                return false;
           
        }

        private void TryDestroyNetAt(IntVec3 cell)
        {
            if (!cell.InBounds(this.map))
            {
                return;
            }
            GasPipeNet powerNet = this.map.GetComponent<PipeNetGrid>().TransmittedPowerNetAt(cell);
            if (powerNet != null)
            {
                this.DeletePowerNet(powerNet);
            }
        }

        private void DrawDebugPowerNets()
        {
            if (Current.ProgramState != ProgramState.Playing)
            {
                return;
            }
            if (Find.CurrentMap != this.map)
            {
                return;
            }
            int num = 0;
            foreach (GasPipeNet current in this.allNets)
            {
                foreach (CompPipe current2 in current.transmitters.Concat(current.connectors))
                {
                    foreach (IntVec3 current3 in GenAdj.CellsOccupiedBy(current2.parent))
                    {
                        CellRenderer.RenderCell(current3, (float)num * 0.44f);
                    }
                }
                num++;
            }
        }
    }
}
