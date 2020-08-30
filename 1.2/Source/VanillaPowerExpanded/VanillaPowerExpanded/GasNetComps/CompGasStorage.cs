// CompGasStorage.cs
// Copyright Karel Kroeze, -2020

using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace GasNetwork
{
    public class CompGasStorage : CompGas
    {
        // todo: interact with CompBreakdownable
        private float _stored;

        public virtual float Stored
        {
            get => _stored;
            protected set => _stored = value;
        }

        public virtual float Capacity => Props.capacity - Stored;

        public virtual Vector2 BarSize => new Vector2( 1f, .16f );
        public new CompProperties_GasStorage Props => props as CompProperties_GasStorage;

        public virtual void Store( float amount )
        {
            Stored += amount;
            if ( Stored > Props.capacity )
                Log.Error( $"storage greater than capacity: {parent.Label} ({parent.Position})" );
            if ( Stored < 0 )
                Log.Error( $"storage lower than 0: {parent.Label} ({parent.Position})" );
        }

        public virtual void Draw( float amount ) => Store( -amount );

        public override void PostDraw()
        {
            base.PostDraw();

            var barRequest = new GenDraw.FillableBarRequest
            {
                center = parent.DrawPos + Vector3.up * .2f + Vector3.forward * .25f,
                size = BarSize,
                fillPercent = Stored / Props.capacity,
                filledMat = Resources.GasBarFilledMaterial,
                unfilledMat = Resources.GasBarUnfilledMaterial,
                margin = .1f,
                rotation = parent.Rotation.Rotated( RotationDirection.Clockwise )
            };
            GenDraw.DrawFillableBar( barRequest );
        }

        public override void ReceiveCompSignal( string signal )
        {
            base.ReceiveCompSignal( signal );

            // remove stored gas on breakdown.
            // todo; this is a perfect place for a gas leak/explosion mechanic!
            if ( signal == CompBreakdownable.BreakdownSignal )
                Stored = 0;
        }

        public override string CompInspectStringExtra()
        {
            var builder = new StringBuilder();
            builder.AppendLine( I18n.Stored( Stored, Props.capacity ) );
            builder.AppendLine( base.CompInspectStringExtra() );
            return builder.ToString().Trim();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach ( Gizmo c in base.CompGetGizmosExtra() )
            {
                yield return c;
            }

            if ( Prefs.DevMode )
            {
                if ( Capacity > 0 )
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEBUG: Fill",
                        action       = delegate { Store( Capacity ); }
                    };
                }

                if ( Stored > 0 )
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEBUG: Empty",
                        action       = delegate { Draw( Stored ); }
                    };
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look( ref _stored, "stored" );
        }
    }
    public class CompProperties_GasStorage : CompProperties_Gas
    {
        public int capacity = 100;
    }
}