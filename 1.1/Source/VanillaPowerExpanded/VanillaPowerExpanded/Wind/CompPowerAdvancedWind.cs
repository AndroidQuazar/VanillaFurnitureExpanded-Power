﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VanillaPowerExpanded
{
    [StaticConstructorOnStartup]
    public class CompPowerAdvancedWind : CompPowerPlant
    {
        protected override float DesiredPowerOutput
        {
            get
            {
                return this.cachedPowerOutput;
            }
        }

        private float PowerPercent
        {
            get
            {
                return base.PowerOutput / (-base.Props.basePowerConsumption * 1.5f);
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            BarSize = new Vector2((float)this.parent.def.size.z - 0.95f, 0.14f);
            this.RecalculateBlockages();
            this.spinPosition = Rand.Range(0f, 15f);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.ticksSinceWeatherUpdate, "updateCounter", 0, false);
            Scribe_Values.Look<float>(ref this.cachedPowerOutput, "cachedPowerOutput", 0f, false);
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!base.PowerOn)
            {
                this.cachedPowerOutput = 0f;
                return;
            }
            this.ticksSinceWeatherUpdate++;
            if (this.ticksSinceWeatherUpdate >= this.updateWeatherEveryXTicks)
            {
                float num = Mathf.Min(this.parent.Map.windManager.WindSpeed, 1.5f);
                this.ticksSinceWeatherUpdate = 0;
                this.cachedPowerOutput = -(base.Props.basePowerConsumption * num);
                this.RecalculateBlockages();
                if (this.windPathBlockedCells.Count > 0)
                {
                    float num2 = 0f;
                    for (int i = 0; i < this.windPathBlockedCells.Count; i++)
                    {
                        num2 += this.cachedPowerOutput * 0.2f;
                    }
                    this.cachedPowerOutput -= num2;
                    if (this.cachedPowerOutput < 0f)
                    {
                        this.cachedPowerOutput = 0f;
                    }
                }
            }
            if (this.cachedPowerOutput > 0.01f)
            {
                this.spinPosition += this.PowerPercent * SpinRateFactor;
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();
            GenDraw.FillableBarRequest r = new GenDraw.FillableBarRequest
            {
                center = this.parent.DrawPos + Vector3.up * 0.1f,
                size = BarSize,
                fillPercent = this.PowerPercent,
                filledMat = WindTurbineBarFilledMat,
                unfilledMat = WindTurbineBarUnfilledMat,
                margin = 0.15f
            };
            Rot4 rotation = this.parent.Rotation;
            rotation.Rotate(RotationDirection.Clockwise);
            r.rotation = rotation;
            GenDraw.DrawFillableBar(r);
            Vector3 vector = this.parent.TrueCenter();
            vector += this.parent.Rotation.FacingCell.ToVector3() * VerticalBladeOffset;
            vector += this.parent.Rotation.RighthandCell.ToVector3() * HorizontalBladeOffset;
            vector.y += 0.0454545468f;
            float num = BladeWidth * Mathf.Sin(this.spinPosition);
            if (num < 0f)
            {
                num *= -1f;
            }
            bool flag = this.spinPosition % 3.14159274f * 2f < 3.14159274f;
            Vector2 vector2 = new Vector2(num, 1f);
            Vector3 s = new Vector3(vector2.x, 1f, vector2.y);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(vector, this.parent.Rotation.AsQuat, s);
            Graphics.DrawMesh(flag ? MeshPool.plane10 : MeshPool.plane10Flip, matrix, WindTurbineBladesMat, 0);
            vector.y -= 0.09090909f;
           
            matrix.SetTRS(vector, this.parent.Rotation.AsQuat, s);
            Graphics.DrawMesh(flag ? MeshPool.plane10Flip : MeshPool.plane10, matrix, WindTurbineBladesMat, 0);
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.CompInspectStringExtra());
            if (this.windPathBlockedCells.Count > 0)
            {
                stringBuilder.AppendLine();
                Thing thing = null;
                if (this.windPathBlockedByThings != null)
                {
                    thing = this.windPathBlockedByThings[0];
                }
                if (thing != null)
                {
                    stringBuilder.Append("WindTurbine_WindPathIsBlockedBy".Translate() + " " + thing.Label);
                }
                else
                {
                    stringBuilder.Append("WindTurbine_WindPathIsBlockedByRoof".Translate());
                }
            }
            return stringBuilder.ToString();
        }

        private void RecalculateBlockages()
        {
            if (this.windPathCells.Count == 0)
            {
                IEnumerable<IntVec3> collection = WindTurbineUtility.CalculateWindCells(this.parent.Position, this.parent.Rotation, this.parent.def.size);
                this.windPathCells.AddRange(collection);
            }
            this.windPathBlockedCells.Clear();
            this.windPathBlockedByThings.Clear();
            for (int i = 0; i < this.windPathCells.Count; i++)
            {
                IntVec3 intVec = this.windPathCells[i];
                if (this.parent.Map.roofGrid.Roofed(intVec))
                {
                    this.windPathBlockedByThings.Add(null);
                    this.windPathBlockedCells.Add(intVec);
                }
                else
                {
                    List<Thing> list = this.parent.Map.thingGrid.ThingsListAt(intVec);
                    for (int j = 0; j < list.Count; j++)
                    {
                        Thing thing = list[j];
                        if (thing.def.blockWind)
                        {
                            this.windPathBlockedByThings.Add(thing);
                            this.windPathBlockedCells.Add(intVec);
                            break;
                        }
                    }
                }
            }
        }

        public int updateWeatherEveryXTicks = 250;

        private int ticksSinceWeatherUpdate;

        private float cachedPowerOutput;

        private List<IntVec3> windPathCells = new List<IntVec3>();

        private List<Thing> windPathBlockedByThings = new List<Thing>();

        private List<IntVec3> windPathBlockedCells = new List<IntVec3>();

        private float spinPosition;

        private const float MaxUsableWindIntensity = 1.5f;

        [TweakValue("Graphics", 0f, 0.1f)]
        private static float SpinRateFactor = 0.035f;

        [TweakValue("Graphics", -1f, 1f)]
        private static float HorizontalBladeOffset = -0.02f;

        [TweakValue("Graphics", 0f, 1f)]
        private static float VerticalBladeOffset = 1.2f;

        [TweakValue("Graphics", 4f, 8f)]
        private static float BladeWidth = 6.6f;

        private const float PowerReductionPercentPerObstacle = 0.2f;

        private const string TranslateWindPathIsBlockedBy = "WindTurbine_WindPathIsBlockedBy";

        private const string TranslateWindPathIsBlockedByRoof = "WindTurbine_WindPathIsBlockedByRoof";

        private static Vector2 BarSize;

        private static readonly Material WindTurbineBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.5f, 0.475f, 0.1f), false);

        private static readonly Material WindTurbineBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f), false);

        private static readonly Material WindTurbineBladesMat = MaterialPool.MatFrom("Things/Building/Power/AdvancedWindTurbine/AdvancedWindTurbineBlades");
    }
}
