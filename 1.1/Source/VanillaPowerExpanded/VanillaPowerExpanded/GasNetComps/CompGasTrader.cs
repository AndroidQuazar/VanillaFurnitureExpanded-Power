// CompGasTrader.cs
// Copyright Karel Kroeze, -2020

using System;
using System.Text;
using GasNetwork.Patches;
using RimWorld;
using UnityEngine;
using Verse;

namespace GasNetwork
{
    public class CompGasTrader : CompGas
    {
        // todo; react to comp on signals properly.
        public const string Signal_GasOn = "GasOn";
        public const string Signal_GasOff = "GasOff";

        public new     CompProperties_GasTrader Props                 => props as CompProperties_GasTrader;
        private float? _gasConsumption;

        public virtual float GasConsumption
        {
            get => _gasConsumption ?? Props.gasConsumption;
            set => _gasConsumption = value;
        }

        public virtual float GasConsumptionPerTick => GasConsumption / GenDate.TicksPerDay;
        private bool _gasOn = true;

        public virtual bool GasOn
        {
            get => _gasOn;
            set
            {
                if ( value == _gasOn )
                    return;
                _gasOn = value;
                parent.BroadcastCompSignal( _gasOn ? Signal_GasOn : Signal_GasOff );
            }
        }
        
        public override void ReceiveCompSignal( string signal )
        {
            base.ReceiveCompSignal( signal );

            if ( signal == CompFlickable.FlickedOffSignal
              || signal == CompSchedule.ScheduledOffSignal
              || signal == CompBreakdownable.BreakdownSignal
              || signal == CompPowerTrader.PowerTurnedOffSignal )
                GasOn = false;
        }

        public virtual bool WantsToBeOn => parent.WantsAndCanBeOn() && parent.HasPower();

        public override string CompInspectStringExtra()
        {
            var builder = new StringBuilder( base.CompInspectStringExtra() + "\n" );
            if ( GasOn )
            {
                if ( GasConsumption > 0 )
                    builder.AppendLine( I18n.Consumption( GasConsumption ) );
                else
                    builder.AppendLine( I18n.Production( -GasConsumption ) );
            }
            else
                builder.AppendLine( I18n.GasOff );

            return builder.ToString().Trim();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look( ref _gasOn, "on", true );
            Scribe_Values.Look( ref lastUsed, "lastUsed" );
        }

        public override void PostSpawnSetup( bool respawningAfterLoad )
        {
            base.PostSpawnSetup( respawningAfterLoad );
            usable = Props.gasConsumptionWhenUsed > 0 && 
                     Mathf.Abs( Props.gasConsumption - Props.gasConsumptionWhenUsed ) > Mathf.Epsilon;
        }

        public override void PostDraw()
        {
            if ( parent.TryGetComp<CompFlickable>( out var flickable ) && !flickable.SwitchIsOn )
                parent.Map.overlayDrawer.DrawOverlay( parent, GasOverlays.GasOff );

            if ( !GasOn && WantsToBeOn )
                parent.Map.overlayDrawer.DrawOverlay( parent, GasOverlays.NeedsGas );
        }

        private int lastUsed;
        private bool usable;
        public override void CompTick()
        {
            if ( !usable ) return;

            GasConsumption = lastUsed + 1 >= Find.TickManager.TicksGame
                ? Props.gasConsumptionWhenUsed
                : Props.gasConsumption;
        }
        public void Notify_UsedThisTick()
        {
            lastUsed = Find.TickManager.TicksGame;
        }
    }

    public class CompProperties_GasTrader : CompProperties_Gas
    {
        public float gasConsumption = 0;
        public float gasConsumptionWhenUsed = 0;
    }
}