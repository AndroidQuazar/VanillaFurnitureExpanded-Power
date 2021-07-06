// Copyright Karel Kroeze, 2020-2020.
// VanillaPowerExpanded/VanillaPowerExpanded/DefModExtension_SpreadingGas.cs

using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace GasNetwork
{
    public class DefModExtension_SpreadingGas : DefModExtension
    {
        public const int       defaultMaxDensity = 1000;
        public       HediffDef exposureHediff;
        public       bool      flammable  = false;
        public       int       maxDensity = defaultMaxDensity;
        public       float     severityPerHourExposed;
        public       int       staticDissipation = defaultMaxDensity / 20;
        public       int       windDissipation   = defaultMaxDensity / 10;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var configError in base.ConfigErrors())
            {
                yield return configError;
            }

            if (staticDissipation < Mathf.Epsilon)
            {
                yield return "static dissipation should be > 0";
            }

            if (windDissipation < 0)
            {
                yield return "wind dissipation can not be lower than zero.";
            }

            if (exposureHediff != null && Mathf.Abs(severityPerHourExposed) < Mathf.Epsilon)
            {
                yield return "if exposure hediff is set, severity should be non-zero.";
            }

            if (Mathf.Abs(severityPerHourExposed) > Mathf.Epsilon && exposureHediff == null)
            {
                yield return "non-zero exposure severity should have an exposure hediff set.";
            }
        }
    }
}