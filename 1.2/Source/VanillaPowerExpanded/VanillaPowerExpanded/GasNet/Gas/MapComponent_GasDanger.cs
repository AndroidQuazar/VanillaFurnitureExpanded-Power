// Copyright Karel Kroeze, 2020-2020.
// VanillaPowerExpanded/VanillaPowerExpanded/MapComponent_GasDangerGrid.cs

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace GasNetwork
{
    public class MapComponent_GasDanger : MapComponent
    {
        public const int                        CHECK_INTERVAL = 300;
        protected    Dictionary<Region, Danger> _danger        = new Dictionary<Region, Danger>();
        protected    Dictionary<Region, int>    _lastChecked   = new Dictionary<Region, int>();

        public MapComponent_GasDanger(Map map) : base(map)
        {
        }

        public Danger DangerIn(Region region)
        {
            if (_lastChecked.TryGetValue(region, out var lastChecked) &&
                _danger.TryGetValue(region, out var danger)           &&
                lastChecked > GenTicks.TicksGame + CHECK_INTERVAL)
            {
                return danger;
            }

            // calculate danger, some if any toxic/flammable gas in region,
            // deadly if region has > 25% density of dangerous gasses.
            var gasses = region.ListerThings.AllThings.OfType<Gas_Spreading>().Where(g => g.Flammable || g.Toxic);
            danger = Danger.None;
            if (gasses.Any())
            {
                var totalDensity = gasses.Sum(g => g.Density) / region.CellCount;
                danger = totalDensity > .2 ? Danger.Deadly : totalDensity > .05 ? Danger.Some : Danger.None;
            }

            _lastChecked[region] = GenTicks.TicksGame;
            _danger[region]      = danger;
            return danger;
        }

#if DEBUG_DANGER
        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();
            foreach (var danger in _danger)
            {
                if (danger.Value > Danger.None)
                {
                    GenDraw.DrawFieldEdges(danger.Key.Cells.ToList(),
                                           danger.Value == Danger.Deadly ? Color.red : Color.yellow);
                }
            }
        }
#endif
    }
}