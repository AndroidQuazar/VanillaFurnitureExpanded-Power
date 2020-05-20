using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VanillaPowerExpanded
{
    public abstract class CompPipe : ThingComp
    {

        public GasPipeNet transNet;

        public CompPipe connectParent;

        public List<CompPipe> connectChildren;

        private static List<GasPipeNet> recentlyConnectedNets = new List<GasPipeNet>();

        private static CompPipe lastManualReconnector = null;

        public static readonly float WattsToWattDaysPerTick = 1.66666669E-05f;

        public bool TransmitsGasNow
        {
            get
            {
                return this.Props.transmitsGas;


            }
        }

        public GasPipeNet GasPipeNet
        {
            get
            {
                if (this.transNet != null)
                {
                    return this.transNet;
                }
                if (this.connectParent != null)
                {
                    return this.connectParent.transNet;
                }
                return null;
            }
        }

        public CompProperties_Pipe Props
        {
            get
            {
                return (CompProperties_Pipe)this.props;
            }
        }

        public virtual void ResetPowerVars()
        {
            this.transNet = null;
            this.connectParent = null;
            this.connectChildren = null;
            CompPipe.recentlyConnectedNets.Clear();
            CompPipe.lastManualReconnector = null;
        }

        public virtual void SetUpPowerVars()
        {
        }

        public override void PostExposeData()
        {
            Thing thing = null;
            if ((this.parent.def.defName != "VPE_HelixienGenerator")&& (this.parent.def.defName != "VPE_IndustrialHelixienGenerator"))
            {
                if (Scribe.mode == LoadSaveMode.Saving && this.connectParent != null)
                {
                    thing = this.connectParent.parent;
                }
                Scribe_References.Look<Thing>(ref thing, "parentThing", false);
                if (thing != null)
                {
                    this.connectParent = ((ThingWithComps)thing).GetComp<CompPipe>();
                }
                if (Scribe.mode == LoadSaveMode.PostLoadInit && this.connectParent != null)
                {
                    this.ConnectToTransmitter(this.connectParent, true);
                }
            }
               
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            
            base.PostSpawnSetup(respawningAfterLoad);
          /*  if (this.Props.transmitsGas)
            {
                this.parent.Map.mapDrawer.MapMeshDirty(this.parent.Position, MapMeshFlag.PowerGrid, true, false);
                if (this.Props.transmitsGas)
                {
                   
                    this.parent.Map.GetComponent<PipeMapComponent>().Notify_TransmitterSpawned(this);
                }
               
                this.SetUpPowerVars();
            }*/
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            
                if (this.Props.transmitsGas)
                {
                    if (this.connectChildren != null)
                    {
                        for (int i = 0; i < this.connectChildren.Count; i++)
                        {
                            this.connectChildren[i].LostConnectParent();
                        }
                    }
                    map.GetComponent<PipeMapComponent>().Notify_TransmitterDespawned(this);
                }
                
                map.mapDrawer.MapMeshDirty(this.parent.Position, MapMeshFlag.PowerGrid, true, false);
            
        }

        public virtual void LostConnectParent()
        {
            this.connectParent = null;
            if (this.parent.Spawned)
            {
                this.parent.Map.GetComponent<PipeMapComponent>().Notify_ConnectorWantsConnect(this);
            }
        }

        public override void PostPrintOnto(SectionLayer layer)
        {
            base.PostPrintOnto(layer);
            if (this.connectParent != null)
            {
                PipeNetGraphics.PrintWirePieceConnecting(layer, this.parent, this.connectParent.parent, false);
            }
        }

        public override void CompPrintForPowerGrid(SectionLayer layer)
        {
            if (this.TransmitsGasNow)
            {
               
                PipeOverlayMats.LinkedOverlayGraphic.Print(layer, this.parent);
            }
            if (this.parent.GetComp<CompPipe>().Props.transmitsGas)
            {
                
                PipeNetGraphics.PrintOverlayConnectorBaseFor(layer, this.parent);
            }
            /*if (this.connectParent != null)
            {
                PipeNetGraphics.PrintWirePieceConnecting(layer, this.parent, this.connectParent.parent, true);
            }*/
        }



      

        public void ConnectToTransmitter(CompPipe transmitter, bool reconnectingAfterLoading = false)
        {
            if (this.connectParent != null && (!reconnectingAfterLoading || this.connectParent != transmitter))
            {
                Log.Error(string.Concat(new object[]
                {
                    "Tried to connect ",
                    this,
                    " to transmitter ",
                    transmitter,
                    " but it's already connected to ",
                    this.connectParent,
                    "."
                }), false);
                return;
            }
            this.connectParent = transmitter;
            if (this.connectParent.connectChildren == null)
            {
                this.connectParent.connectChildren = new List<CompPipe>();
            }
            transmitter.connectChildren.Add(this);
            GasPipeNet pipeNet = this.GasPipeNet;
            if (pipeNet != null)
            {
                pipeNet.RegisterConnector(this);
            }
        }

       /* public override string CompInspectStringExtra()
        {
            if (this.GasPipeNet == null)
            {
                return "VPE_PipesNotConnected".Translate();
            }
            string value = (this.GasPipeNet.CurrentEnergyGainRate() / CompPipe.WattsToWattDaysPerTick).ToString("F0");
            string value2 = this.GasPipeNet.CurrentStoredEnergy().ToString("F0");
            return "VPE_PipeConnectedRateStored".Translate(value, value2);
        }*/

       
    }
}
