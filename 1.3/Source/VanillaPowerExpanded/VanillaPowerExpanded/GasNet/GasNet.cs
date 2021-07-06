// GasNetworkManager.cs
// Copyright Karel Kroeze, 2020-2020

using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Random = UnityEngine.Random;

namespace GasNetwork
{
    public class GasNet
    {
        public List<CompGas> connectors = new List<CompGas>();
        public List<CompGasTrader> traders = new List<CompGasTrader>();
        public List<CompGasStorage> storages = new List<CompGasStorage>();

        public BoolGrid netGrid;
        public Map map;

#if DEBUG
        public Color color = Random.ColorHSV( 0, 1, 1, 1, 1, 1, 1, 1 );
        public CellBoolDrawer drawer;
#endif

        public virtual float Consumption { get; private set; }
        public virtual float Production { get; private set; }
        public virtual float Stored { get; private set; }
        public string InspectString => I18n.GasNetworkInspectString( Production - Consumption, Stored );

        public GasNet( IEnumerable<CompGas> connectors, Map map )
        {
            this.map = map;
            netGrid = new BoolGrid( map );

#if DEBUG
            drawer = new CellBoolDrawer( ( index ) => netGrid[index], () => color, ( index ) => color, map.Size.x,
                                         map.Size.z );
#endif

            foreach ( var connector in connectors )
                Register( connector );

        }

#if DEBUG
        public void DebugOnGUI()
        {
            drawer.CellBoolDrawerUpdate();
            drawer.MarkForDraw();
        }
#endif

        private List<CompGasTrader> consumers = new List<CompGasTrader>();
        private List<CompGasTrader> consumersWantingToBeOn = new List<CompGasTrader>();
        public virtual void GasNetTick( int ticks )
        {
            // update working lists.
            consumers.Clear();
            foreach ( var trader in traders.Where( t => t.GasOn && t.GasConsumption > 0 ) )
                consumers.Add( trader );

            // update production stats
            Consumption = consumers.Select( c => c.GasConsumption ).Sum();
            Production = -traders.Where( t => t.GasOn && t.GasConsumption < 0 )
                                 .Select( t => t.GasConsumption ).Sum();
            Stored = storages.Sum( s => s.Stored );

            // get current available
            float multiplier = (float)GenDate.TicksPerDay / ticks;
            float production = Production / multiplier;
            float consumption = Consumption / multiplier;
            // start turning devices off before we run out of gas completely. 
            // note that this may lead to storages not being completely empty, 
            // but devices being turned off - and probably bug reports. Tough.
            float available = production + Mathf.Max( Stored - 5, 0 ); 

//            Log.Debug( $"ticking network {GetHashCode()} x{ticks} :: p; {production} :: c; {consumption} :: s; {Stored} (-5 buffer)"  );
            
            // if not enough, start randomly shutting shit down
            if ( available < consumption )
            {
                if ( consumers.Any() )
                {
                    var consumer = consumers.RandomElement();
                    consumption -= consumer.GasConsumptionPerTick * ticks;
                    consumption = Mathf.Max( 0f, consumption );
                    consumer.GasOn = false;
//                    Log.Debug( $"turning off {consumer.parent.Label}" );
                }
            }

            // update working list for parts wanting to be on
            // note that this includes gas _producers_ that were for some reason switched off.
            consumersWantingToBeOn.Clear();
            foreach ( var trader in traders.Where( t => !t.GasOn
                                                     && ( t.GasConsumption                < 0 // producer
                                                       || t.GasConsumptionPerTick * ticks < available - consumption ) // consumer that consumes less than excess
                                                     && t.WantsToBeOn ) )
                consumersWantingToBeOn.Add( trader );

            // more than needed, see about turning shit back on
            if ( consumersWantingToBeOn.Any() )
            {
                var consumer = consumersWantingToBeOn.RandomElement();
                consumption += consumer.GasConsumptionPerTick * ticks;
                consumer.GasOn = true;
//                Log.Debug( $"turning on {consumer.parent.Label}");
            }

            // update storage
            float storage = production - consumption;
//            Log.Debug( $"dealing with excess/shortfall :: p; {production} :: c; {consumption} :: s; {(storage > 0 ? "+" : "" ) + storage}" );

            if ( storage > 0 )
                Store( storage );
            else
                Draw( -storage );
        }

        public void Store( float amount )
        {
            // try to evenly spread out amongst storage units - emphasizing those with more capacity.
            var capacity = storages.Sum( s => s.Capacity );
            amount = Math.Min( capacity, amount );
            if ( amount < Mathf.Epsilon )
                return;
            foreach ( var storage in storages )
            {
//                Log.Debug( $"storing {storage.Capacity}/{capacity} * {amount} ({( amount * storage.Capacity ) / capacity}) in {storage.parent.Label} (cur: {storage.Stored})"  );
                storage.Store( ( amount * storage.Capacity ) / capacity ); // store amount proportional to capacity
//                Log.Debug( $"{storage.parent.Label} (cur: {storage.Stored})"  );
            }
        }

        public void Draw( float amount )
        {
            // draw chunks of gas until amount fulfilled, 
            // chunksize starts at amount / numStorages,
            // but is decreased when necessary.
            var numChunks = 2      * storages.Count;
            var chunkSize = amount / numChunks;
            var iteration = 0;

            while ( amount    > 0         &&
                    iteration < numChunks && // worst case; all chunks come from same storage
                    storages.Any( s => s.Stored > 0 ) )
            {
                foreach ( var storage in storages.Where( s => s.Stored > 0 ) )
                {
                    var chunk = Mathf.Min( chunkSize, storage.Stored, amount );
//                    Log.Debug( $"drawing {chunk} from {storage.parent.Label} ({storage.Stored}), {amount} left to draw" );
                    
                    if ( !( chunk >= Mathf.Epsilon ) ) continue;
                    amount -= chunk;
                    storage.Draw( chunk );
                }

                iteration++;
            }

            if ( amount > Mathf.Epsilon )
                Log.Warning( "Tried to draw more gas than is available" );
        }

        public virtual void Register( CompGas comp )
        {
            connectors.Add( comp );
            if ( comp is CompGasTrader trader ) traders.Add( trader );
            if ( comp is CompGasStorage storage ) storages.Add( storage );
            comp.Network = this;

            foreach ( var cell in comp.parent.OccupiedRect().Cells )
                netGrid.Set( cell, true );

#if DEBUG
            drawer.SetDirty();
#endif
        }

        public virtual void Destroy()
        {
            foreach ( var connector in connectors )
                connector.Network = null;

#if DEBUG
            drawer.SetDirty();
#endif
        }
    }
}