// Copyright Karel Kroeze, 2020-2020.
// VanillaPowerExpanded/VanillaPowerExpanded/Gas_Spreading.cs

using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace GasNetwork
{
    public class Gas_Spreading : Gas
    {
        private DefModExtension_SpreadingGas _gasProps;

        private Graphic _graphic;
        public float Density;

        public DefModExtension_SpreadingGas GasProps =>
            _gasProps ??= def.GetModExtension<DefModExtension_SpreadingGas>();

        public bool Flammable => GasProps?.flammable ?? false;
        public bool Toxic => GasProps.exposureHediff != null && GasProps.severityPerHourExposed > 0;

        public override Color DrawColor => def.graphicData.color * new Color(1, 1, 1, Density);

        public override string LabelMouseover => $"{LabelCap} ({Density.ToStringPercent()})";
        public override Graphic Graphic => _graphic ??= DefaultGraphic;

        private static List<Gas_Spreading> gases = new List<Gas_Spreading>();

        public static bool AnyGases => gases.Count > 0;

        public void UpdateGraphic()
        {
            if (DrawColor.IndistinguishableFrom(Graphic.Color))
            {
                return;
            }

            _graphic = DefaultGraphic.GetColoredVersion(DefaultGraphic.Shader, DrawColor, DrawColorTwo);
        }

        public void AddGas(ref float amount, bool allowOverSaturation = false)
        {
            float fillAmount;
            fillAmount = allowOverSaturation
                ? amount
                : Mathf.Clamp((1 - Density) * GasProps.maxDensity, 0, amount);

            Density += fillAmount / GasProps.maxDensity;
            amount -= fillAmount;
        }

        public override void Tick()
        {
            if (this.IsHashIntervalTick(GenTicks.TicksPerRealSecond))
            {
                DissipateAndSpread();
                if (Density <= 0)
                {
                    Destroy();
                    return;
                }

                UpdateGraphic();

                if (Flammable || Toxic)
                {
                    if (this.IsHashIntervalTick(GenTicks.TicksPerRealSecond * 4))
                    {
                        // NOTE: make sure this inner interval is a multiple of the outer interval.
                        UpdateDanger(Map);
                    }

                    var things = Position.GetThingList(Map);
                    if (Flammable
                     && (Position.GetRoom(Map).Temperature > 100
                      || things.Any(t => t.def == ThingDefOf.Fire || t.IsBurning())))
                    {
                        DoExplode();
                    }

                    if (Toxic)
                    {
                        try
                        {
                            foreach (var pawn in things.OfType<Pawn>().Where(p => p.RaceProps.IsFlesh))
                            {
                                HealthUtility.AdjustSeverity(pawn, GasProps.exposureHediff,
                                                             (float)GenTicks.TicksPerRealSecond /
                                                             GenDate.TicksPerHour *
                                                             // wind effects can cause a density > 1, we don't want pawn 
                                                             // instagibs or weird buggy healing, so clamp to 0-1.
                                                             Mathf.Clamp01(Density) * GasProps.severityPerHourExposed);
                            }
                        }
                        catch (InvalidOperationException)
                        {
                            // death causes foreach collection to be modified.
                            // catching and ignoring the error may cause gassing of very large groups to take slightly longer,
                            // as each death will stop iteration of any other victims in this cell. This seems like a minor
                            // inconvenience.
                            Log.Debug("Someone was gassed to death. Move on, nothing to see here.");
                        }
                    }
                }
            }

            graphicRotation += graphicRotationSpeed;
        }

        private void DissipateAndSpread()
        {
            var roofed = Position.Roofed(Map);

            // dissipate
            Density -= (float)GasProps.staticDissipation / GasProps.maxDensity;
            if (Density <= 0)
            {
                return;
            }

            // spread
            var room = Position.GetRoom(Map);
            if (roofed)
            {
                foreach (var neighbour in GenAdjFast.AdjacentCellsCardinal(Position))
                {
                    if (!neighbour.Impassable(Map) && neighbour.GetRoom(Map) == room)
                    {
                        var amountDissipated = GenGas.AddGas(neighbour, Map, def, GasProps.staticDissipation, false);
                        Density -= amountDissipated / GasProps.maxDensity;
                    }
                }
            }
            else
            {
                // not roofed, so lets assume wind influence.
                var wind = Map.windVector() * Density;

                // TODO: wind dissipation can probably use some balancing passes
                // have a look at fluid dynamics for a simplified but proper algorithm?
                var windNorth = Mathf.Clamp((Vector2.up * wind).y, .1f, float.MaxValue) * GasProps.windDissipation;
                var windEast = Mathf.Clamp((Vector2.right * wind).x, .1f, float.MaxValue) * GasProps.windDissipation;
                var windSouth = Mathf.Clamp((Vector2.down * wind).y, .1f, float.MaxValue) * GasProps.windDissipation;
                var windWest = Mathf.Clamp((Vector2.left * wind).x, .1f, float.MaxValue) * GasProps.windDissipation;

                // normalize dissipation so that no gas is created out of nothing
                var sum = windNorth + windEast + windSouth + windWest;
                if (sum > Density * GasProps.maxDensity)
                {
                    var factor = Density * GasProps.maxDensity / sum;
                    windNorth *= factor;
                    windEast *= factor;
                    windSouth *= factor;
                    windWest *= factor;
                }

                // north
                var neighbour = Position + IntVec3.North;
                if (!neighbour.Impassable(Map) && neighbour.GetRoom(Map) == room)
                {
                    var amountDissipated = GenGas.AddGas(
                        neighbour, Map, def, windNorth, false, true);
                    Density -= amountDissipated / GasProps.maxDensity;
                }

                // east
                neighbour = Position + IntVec3.East;
                if (!neighbour.Impassable(Map) && neighbour.GetRoom(Map) == room)
                {
                    var amountDissipated = GenGas.AddGas(
                        neighbour, Map, def, windEast, false, true);
                    Density -= amountDissipated / GasProps.maxDensity;
                }

                // south
                neighbour = Position + IntVec3.South;
                if (!neighbour.Impassable(Map) && neighbour.GetRoom(Map) == room)
                {
                    var amountDissipated = GenGas.AddGas(
                        neighbour, Map, def, windSouth, false, true);
                    Density -= amountDissipated / GasProps.maxDensity;
                }

                // west
                neighbour = Position + IntVec3.West;
                if (!neighbour.Impassable(Map) && neighbour.GetRoom(Map) == room)
                {
                    var amountDissipated = GenGas.AddGas(
                        neighbour, Map, def, windWest, false, true);
                    Density -= amountDissipated / GasProps.maxDensity;
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Density, "density");
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            gases.Add(this);
            base.SpawnSetup(map, respawningAfterLoad);
            MapComponent_GasDanger.GetCachedComp(map).RegisterAt(this, Position);
            UpdateDanger(map);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            gases.Remove(this);
            var map = Map;
            MapComponent_GasDanger.GetCachedComp(map).Deregister(this, Position);
            base.DeSpawn(mode);
            UpdateDanger(map);
        }

        public virtual void DoExplode()
        {
            var range = Mathf.Clamp01(Density) * 3;
            var damage = (int)(Mathf.Clamp01(Density) * 20);
            if (Position.Roofed(Map))
            {
                // a contained explosion does MUCH more damage, discourage indoor use as a gas chamber
                damage *= 3;
            }

            GenExplosion.DoExplosion(Position, Map, range, DamageDefOf.Flame, this,
                                     damage, damageFalloff: true, chanceToStartFire: .3f);
            Destroy();
        }

        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostApplyDamage(dinfo, totalDamageDealt);

            if (Flammable && dinfo.Def == DamageDefOf.Flame)
            {
                DoExplode();
            }
        }

        public virtual void UpdateDanger(Map map)
        {
            if (!Flammable && !Toxic)
            {
                return;
            }

            foreach (var offset in GenAdj.AdjacentCellsAndInside)
            {
                var cell = Position + offset;
                if (cell.InBounds(map))
                {
                    map.pathGrid.RecalculatePerceivedPathCostAt(cell);
#if DEBUG_DANGER
                    map.debugDrawer.FlashCell(
                        cell,
                        SolidColorMaterials.SimpleSolidColorMaterial(Color.Lerp(Color.green, Color.red,
                                                                         Mathf.Clamp01(
                                                                             (float) map.pathGrid.pathGrid[
                                                                                 map.cellIndices.CellToIndex(cell)] /
                                                                             1000))));
#endif
                }
            }
        }
    }
}