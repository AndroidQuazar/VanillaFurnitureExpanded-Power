﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace VanillaPowerExpanded
{
    [StaticConstructorOnStartup]
    public class CompPowerAdvancedWater : CompPowerPlant
    {
        protected override float DesiredPowerOutput
        {
            get
            {
                if (this.cacheDirty)
                {
                    this.RebuildCache();
                }
                if (!this.waterUsable)
                {
                    return 0f;
                }
                if (this.waterDoubleUsed)
                {
                    return base.DesiredPowerOutput * 0.3f;
                }
                return base.DesiredPowerOutput;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.spinPosition = Rand.Range(0f, 15f);
            this.RebuildCache();
            this.ForceOthersToRebuildCache(this.parent.Map);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            this.ForceOthersToRebuildCache(map);
        }

        private void ClearCache()
        {
            this.cacheDirty = true;
        }

        private void RebuildCache()
        {
            this.waterUsable = true;
            foreach (IntVec3 c in this.WaterCells())
            {
                if (c.InBounds(this.parent.Map) && !this.parent.Map.terrainGrid.TerrainAt(c).affordances.Contains(TerrainAffordanceDefOf.MovingFluid))
                {
                    this.waterUsable = false;
                    break;
                }
            }
            this.waterDoubleUsed = false;
            IEnumerable<Building> enumerable = this.parent.Map.listerBuildings.AllBuildingsColonistOfDef(ThingDef.Named("VFE_AdvancedWatermillGenerator"));
            foreach (IntVec3 c2 in this.WaterUseCells())
            {
                if (c2.InBounds(this.parent.Map))
                {
                    foreach (Building building in enumerable)
                    {
                        if (building != this.parent && building.GetComp<CompPowerAdvancedWater>().WaterUseRect().Contains(c2))
                        {
                            this.waterDoubleUsed = true;
                            break;
                        }
                    }
                }
            }
            if (!this.waterUsable)
            {
                this.spinRate = 0f;
                return;
            }
            Vector3 vector = Vector3.zero;
            foreach (IntVec3 intVec in this.WaterCells())
            {
                vector += this.parent.Map.waterInfo.GetWaterMovement(intVec.ToVector3Shifted());
            }
            this.spinRate = Mathf.Sign(Vector3.Dot(vector, this.parent.Rotation.Rotated(RotationDirection.Clockwise).FacingCell.ToVector3()));
            this.spinRate *= Rand.RangeSeeded(0.9f, 1.1f, this.parent.thingIDNumber * 60509 + 33151);
            if (this.waterDoubleUsed)
            {
                this.spinRate *= 0.5f;
            }
            this.cacheDirty = false;
        }

        private void ForceOthersToRebuildCache(Map map)
        {
            foreach (Building building in map.listerBuildings.AllBuildingsColonistOfDef(ThingDef.Named("VFE_AdvancedWatermillGenerator")))
            {
                building.GetComp<CompPowerAdvancedWater>().ClearCache();
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (base.PowerOutput > 0.01f)
            {
                this.spinPosition = (this.spinPosition + 0.006666667f * this.spinRate + 6.28318548f) % 6.28318548f;
            }
        }

        public IEnumerable<IntVec3> WaterCells()
        {
            return WaterCells(this.parent.Position, this.parent.Rotation);
        }

        public static IEnumerable<IntVec3> WaterCells(IntVec3 loc, Rot4 rot)
        {
            IntVec3 perpOffset = rot.Rotated(RotationDirection.Counterclockwise).FacingCell;
            yield return loc + rot.FacingCell * 3;
            yield return loc + rot.FacingCell * 3 - perpOffset;
            yield return loc + rot.FacingCell * 3 - perpOffset * 2;
            yield return loc + rot.FacingCell * 3 + perpOffset;
            yield return loc + rot.FacingCell * 3 + perpOffset * 2;
            yield break;
        }

        public CellRect WaterUseRect()
        {
            return WaterUseRect(this.parent.Position, this.parent.Rotation);
        }

        public static CellRect WaterUseRect(IntVec3 loc, Rot4 rot)
        {
            int width = rot.IsHorizontal ? 7 : 13;
            int height = rot.IsHorizontal ? 13 : 7;
            return CellRect.CenteredOn(loc + rot.FacingCell * 4, width, height);
        }

        public IEnumerable<IntVec3> WaterUseCells()
        {
            return WaterUseCells(this.parent.Position, this.parent.Rotation);
        }

        public static IEnumerable<IntVec3> WaterUseCells(IntVec3 loc, Rot4 rot)
        {
            foreach (IntVec3 intVec in WaterUseRect(loc, rot))
            {
                yield return intVec;
            }
            yield break;
            
        }

        public IEnumerable<IntVec3> GroundCells()
        {
            return GroundCells(this.parent.Position, this.parent.Rotation);
        }

        public static IEnumerable<IntVec3> GroundCells(IntVec3 loc, Rot4 rot)
        {
            IntVec3 perpOffset = rot.Rotated(RotationDirection.Counterclockwise).FacingCell;
            yield return loc - rot.FacingCell;
            yield return loc - rot.FacingCell - perpOffset;
            yield return loc - rot.FacingCell + perpOffset;
            yield return loc;
            yield return loc - perpOffset;
            yield return loc + perpOffset;
            yield return loc + rot.FacingCell;
            yield return loc + rot.FacingCell - perpOffset;
            yield return loc + rot.FacingCell + perpOffset;
            yield break;
        }

        public override void PostDraw()
        {
            base.PostDraw();
            Vector3 a = this.parent.TrueCenter();
            a += this.parent.Rotation.FacingCell.ToVector3() * 2.36f;
            for (int i = 0; i < 9; i++)
            {
                float num = this.spinPosition + 6.28318548f * (float)i / 9f;
                float x = Mathf.Abs(4f * Mathf.Sin(num));
                bool flag = num % 6.28318548f < 3.14159274f;
                Vector2 vector = new Vector2(x, 1f);
                Vector3 s = new Vector3(vector.x, 1f, vector.y);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(a + Vector3.up * 0.0454545468f * Mathf.Cos(num), this.parent.Rotation.AsQuat, s);
                Graphics.DrawMesh(flag ? MeshPool.plane10 : MeshPool.plane10Flip, matrix, BladesMat, 0);
            }
        }

        public override string CompInspectStringExtra()
        {
            string text = base.CompInspectStringExtra();
            if (this.waterUsable && this.waterDoubleUsed)
            {
                text += "\n" + "Watermill_WaterUsedTwice".Translate();
            }
            return text;
        }

        private float spinPosition;

        private bool cacheDirty = true;

        private bool waterUsable;

        private bool waterDoubleUsed;

        private float spinRate = 1f;

        private const float PowerFactorIfWaterDoubleUsed = 0.3f;

        private const float SpinRateFactor = 0.006666667f;

        private const float BladeOffset = 2.36f;

        private const int BladeCount = 9;

        public static readonly Material BladesMat = MaterialPool.MatFrom("Things/Building/Power/AdvancedWatermillGenerator/AdvancedWatermillGeneratorBlades");
    }
}
