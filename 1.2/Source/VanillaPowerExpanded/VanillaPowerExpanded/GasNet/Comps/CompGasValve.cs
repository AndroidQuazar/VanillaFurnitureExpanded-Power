// CompGasValve.cs
// Copyright Karel Kroeze, -2020

using System.Text;
using GasNetwork.Overlay;
using RimWorld;

namespace GasNetwork
{
    public class CompGasValve : CompGas
    {
        protected CompFlickable flicker;

        public override void PostSpawnSetup( bool respawningAfterLoad )
        {
            flicker = parent.GetComp<CompFlickable>();
            base.PostSpawnSetup( respawningAfterLoad );
        }

        public override bool TransmitsGasNow => flicker.SwitchIsOn;

        public override void ReceiveCompSignal( string signal )
        {
            base.ReceiveCompSignal( signal );

            // respond to switch doing switchy things
            if ( signal == CompFlickable.FlickedOffSignal )
                GasNetManager.For( parent.Map ).Notify_ConnectorRemoved( this );
            if ( signal == CompFlickable.FlickedOnSignal )
                GasNetManager.For( parent.Map ).Notify_ConnectorAdded( this );

            // notify overlay drawer
            parent.Map.mapDrawer.MapMeshDirty( parent.Position, 
                                               SectionLayer_GasNetwork.MapMeshFlag, 
                                               true, 
                                               false );
        }

        public override string CompInspectStringExtra()
        {
            if ( TransmitsGasNow )
            {
                var builder = new StringBuilder();
                builder.AppendLine( I18n.ValveOpen );
                builder.AppendLine( base.CompInspectStringExtra() );
                return builder.ToString().Trim();
            }
            else
            {
                return I18n.ValveClosed;
            }
        }
    }
}