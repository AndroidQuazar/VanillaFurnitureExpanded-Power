// GasNetManager.cs
// Copyright Karel Kroeze, -2020

using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace GasNetwork
{
    public class GasNetManager : MapComponent
    {

        public bool sentUpdateLetter = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.sentUpdateLetter, "sentUpdateLetter", false, false);
        }

        public GasNetManager(Map map) : base(map)
        {
        }

        public static GasNetManager For(Map map)
        {
            return map.GetComponent<GasNetManager>();
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            Initialize();
        }

        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();

#if DEBUG
            foreach (var net in GasNetworks)
                net.DebugOnGUI();
#endif
        }

        public List<GasNet> GasNetworks = new List<GasNet>();

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            /* if (!sentUpdateLetter)
             {
                 Find.LetterStack.ReceiveLetter("VPE_GasUpdateNoticeLetterLabel".Translate(), "VPE_GasUpdateNoticeLetterText".Translate(), DefDatabase<LetterDef>.GetNamed("VPE_GasUpdateNoticeLetter"));
                 sentUpdateLetter = true;
             }*/

            foreach (var net in GasNetworks)
                if (net.IsHashIntervalTick(30))
                    net.GasNetTick(30);
        }

        public void Initialize()
        {
            // get a list of all GasComps
            var all = map.listerBuildings
                         .AllBuildingsColonistWithComp<CompGas>()
                         .GetComps<CompGas>()
                         .ToList();

            CreateGasNetworks(all);
        }

        public void Notify_ConnectorAdded(CompGas comp)
        {
            Log.Debug($"adding connector for '{comp.parent.Label}'");

            // sanity check; does this comp already exist?
            if (GasNetworks.Any(n => n.connectors.Contains(comp)) || comp.Network != null)
                Log.Error($"adding gas comp that is already in a network: {comp.parent.Label}");

            // check if it has neighbours that are in a Network.
            var neighbours = comp.GetAdjacentGasComps(true);
            var neighbourNets = neighbours.Select(g => g.Network)
                                          .Distinct()
                                          .Where(n => n != null);

            // if 1, add
            if (neighbourNets.Count() == 1)
                neighbourNets.First().Register(comp);
            else
            {
                // if multiple, destroy
                foreach (var neighbourNet in neighbourNets)
                    DestroyGasNet(neighbourNet);

                // (re)create network(s)
                CreateGasNetworks(neighbours);
            }
        }

        public void Notify_ConnectorRemoved(CompGas comp)
        {
            DestroyGasNet(comp.Network);
            CreateGasNetworks(comp.GetAdjacentGasComps(map));
        }

        public void CreateGasNetworks(List<CompGas> connectors)
        {
            while (connectors.Any())
            {
                GasNetworks.Add(CreateGasNetFrom(connectors.First(), out var seen));

                // remove things we've seen from the list
                foreach (var connector in seen)
                    connectors.Remove(connector);
            }
        }

        public void DestroyGasNet(GasNet net)
        {
            GasNetworks.Remove(net);
            net.Destroy();
        }

        public GasNet CreateGasNetFrom(CompGas root, out HashSet<CompGas> seen)
        {
            // set up vars
            var queue = new Queue<CompGas>();
            var network = new HashSet<CompGas>();
            seen = new HashSet<CompGas>();

            // sanity checks
            if (root == null)
                throw new ArgumentNullException(nameof(root), $"tried to create gas net from null root");

            // if this itself does not allow flow, create network from single comp.
            if (!root.TransmitsGasNow)
            {
                seen.Add(root);
                return new GasNet(new[] { root }, map);
            }

            // basic flood fill. Start somewhere, put all neighbours on a queue,
            // continue until there are no more neighbours.
            queue.Enqueue(root);
            seen.Add(root);
            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                // sanity check; is this already in a network?
                if (cur.Network != null)
                    Log.Error($"trying to add {cur.parent.Label} to a new network while it still has a network");

                network.Add(cur);

                // check neighbours, add to queue if eligible
                foreach (var neighbour in cur.GetAdjacentGasComps())
                    if (!seen.Contains(neighbour))
                    {
                        seen.Add(neighbour);
                        if (!neighbour.TransmitsGasNow) continue;
                        queue.Enqueue(neighbour);
                    }
            }

            return new GasNet(network, map);
        }

        public GasNet GasNetAt(IntVec3 cell)
        {
            foreach (var net in GasNetworks)
                if (net.netGrid[cell])
                    return net;
            return null;
        }
    }
}