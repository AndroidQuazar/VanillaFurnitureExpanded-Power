// CompGas.cs
// Copyright Karel Kroeze, -2020

using System.Text;
using Verse;

namespace GasNetwork
{
    public class CompGas : ThingComp
    {
        public virtual GasNet Network         { get; set; }
        public virtual bool   TransmitsGasNow => true;

        public CompProperties_Gas Props => props as CompProperties_Gas;

        public override void PostSpawnSetup( bool respawningAfterLoad )
        {
            base.PostSpawnSetup( respawningAfterLoad );

            if ( !respawningAfterLoad)
                GasNetManager.For( parent.Map ).Notify_ConnectorAdded( this );
        }

        public override void PostDeSpawn( Map map )
        {
            base.PostDeSpawn( map );
            GasNetManager.For( map ).Notify_ConnectorRemoved( this );
        }

        public override string CompInspectStringExtra()
        {
            var builder = new StringBuilder( base.CompInspectStringExtra() + "\n" );
            builder.AppendLine( Network?.InspectString ?? I18n.NoNetwork );
            return builder.ToString().Trim();
        }
    }

    public class CompProperties_Gas : CompProperties
    {

    }
}