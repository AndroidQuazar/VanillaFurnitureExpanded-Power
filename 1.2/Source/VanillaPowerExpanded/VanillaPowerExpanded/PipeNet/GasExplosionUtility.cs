using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VanillaPowerExpanded
{
    public static class GasExplosionUtility
    {
        public static IEnumerable<Building> GetExplodableGasConduits(Map map)
        {
            GasExplosionUtility.tmpPowerNetHasActivePowerSource.Clear();
            try
            {
                List<Thing> conduits = map.listerThings.ThingsOfDef(ThingDef.Named("VPE_GasPipe"));
                int num;
                for (int i = 0; i < conduits.Count; i = num + 1)
                {
                    Building building = (Building)conduits[i];
                    CompPipe pipeComp = building.GetComp<CompPipe>();
                    if (pipeComp != null)
                    {
                        bool hasActivePowerSource;
                        if (!GasExplosionUtility.tmpPowerNetHasActivePowerSource.TryGetValue(pipeComp.GasPipeNet, out hasActivePowerSource))
                        {
                            hasActivePowerSource = pipeComp.GasPipeNet.HasActivePowerSource;
                            GasExplosionUtility.tmpPowerNetHasActivePowerSource.Add(pipeComp.GasPipeNet, hasActivePowerSource);
                        }
                        if (hasActivePowerSource)
                        {
                            yield return building;
                        }
                    }
                    num = i;
                }
                conduits = null;
            }
            finally
            {
                GasExplosionUtility.tmpPowerNetHasActivePowerSource.Clear();
            }
            yield break;
            yield break;
        }

        public static void DoExplosion(Building culprit)
        {
            GasPipeNet pipeNet = culprit.GetComp<CompPipe>().GasPipeNet;
            Map map = culprit.Map;
            float num = 0f;
            float num2 = 0f;
            bool flag = false;
            if (pipeNet.batteryComps.Any((CompPipeTank x) => x.StoredEnergy > 20f))
            {
                GasExplosionUtility.DrainBatteriesAndCauseExplosion(pipeNet, culprit, out num, out num2);
            }
            else
            {
                flag = GasExplosionUtility.TryStartFireNear(culprit);
            }
            string value;
            if (culprit.def == ThingDef.Named("VPE_GasPipe"))
            {
                value = "VPE_AGasPipe".Translate();
            }
            else
            {
                value = Find.ActiveLanguageWorker.WithIndefiniteArticlePostProcessed(culprit.Label, false, false);
            }
            StringBuilder stringBuilder = new StringBuilder();
            if (flag)
            {
                stringBuilder.Append("VPE_GasExplosionStartedFire".Translate(value));
            }
            else
            {
                stringBuilder.Append("VPE_GasExplosion".Translate(value));
            }
            if (num > 0f)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine();
                stringBuilder.Append("VPE_GasExplosionLostGas".Translate(num.ToString("F0")));
            }
            if (num2 > 5f)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine();
                stringBuilder.Append("VPE_GasExplosionLWasLarge".Translate());
            }
            if (num2 > 8f)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine();
                stringBuilder.Append("VPE_GasExplosionLWasHuge".Translate());
            }
            Find.LetterStack.ReceiveLetter("VPE_LetterLabelGasExplosion".Translate(), stringBuilder.ToString(), LetterDefOf.NegativeEvent, new TargetInfo(culprit.Position, map, false), null, null, null, null);
        }

       

        private static void DrainBatteriesAndCauseExplosion(GasPipeNet net, Building culprit, out float totalEnergy, out float explosionRadius)
        {
            totalEnergy = 0f;
            for (int i = 0; i < net.batteryComps.Count; i++)
            {
                CompPipeTank compTank = net.batteryComps[i];
                totalEnergy += compTank.StoredEnergy;
                compTank.DrawPower(compTank.StoredEnergy);
            }
            explosionRadius = Mathf.Sqrt(totalEnergy) * 0.05f;
            explosionRadius = Mathf.Clamp(explosionRadius, 1.5f, 14.9f);
            GenExplosion.DoExplosion(culprit.Position, net.Map, explosionRadius, DamageDefOf.Flame, null, -1, -1f, null, null, null, null, null, 0f, 1, false, null, 0f, 1, 0f, false, null, null);
            if (explosionRadius > 3.5f)
            {
                GenExplosion.DoExplosion(culprit.Position, net.Map, explosionRadius * 0.3f, DamageDefOf.Bomb, null, -1, -1f, null, null, null, null, null, 0f, 1, false, null, 0f, 1, 0f, false, null, null);
            }
        }

        private static bool TryStartFireNear(Building b)
        {
            GasExplosionUtility.tmpCells.Clear();
            int num = GenRadial.NumCellsInRadius(3f);
            CellRect startRect = b.OccupiedRect();
            for (int i = 0; i < num; i++)
            {
                IntVec3 intVec = b.Position + GenRadial.RadialPattern[i];
                if (GenSight.LineOfSight(b.Position, intVec, b.Map, startRect, CellRect.SingleCell(intVec), null) && FireUtility.ChanceToStartFireIn(intVec, b.Map) > 0f)
                {
                    GasExplosionUtility.tmpCells.Add(intVec);
                }
            }
            return GasExplosionUtility.tmpCells.Any<IntVec3>() && FireUtility.TryStartFireIn(GasExplosionUtility.tmpCells.RandomElement<IntVec3>(), b.Map, Rand.Range(0.1f, 1.75f));
        }

        private static Dictionary<GasPipeNet, bool> tmpPowerNetHasActivePowerSource = new Dictionary<GasPipeNet, bool>();

        private static List<IntVec3> tmpCells = new List<IntVec3>();
    }
}
