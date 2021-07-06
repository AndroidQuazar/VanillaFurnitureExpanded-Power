// Copyright Karel Kroeze, 2020-2020.
// VanillaPowerExpanded/VanillaPowerExpanded/MapComponent_WindDirection.cs

using System;
using UnityEngine;
using Verse;

namespace GasNetwork
{
    public class MapComponent_WindDirection : MapComponent
    {
        public const float TwoPI = Mathf.PI * 2;

        System.Random rand = new System.Random();

        // wind speed is normally maxed at 2, translating to a max beaufort of 12 gives us a factor of 6.
        public const float WindSpeedFactor = 6f;

        // wind direction in radians.
        public float windDirection;

        private static Map[] maps = new Map[20];
        private static MapComponent_WindDirection[] comps = new MapComponent_WindDirection[20];

        public MapComponent_WindDirection(Map map) : base(map)
        {
            if (map.Index >= 0 && map.Index < 20)
            {
                maps[map.Index] = map;
                comps[map.Index] = this;
            }
        }

        public static MapComponent_WindDirection GetCachedComp(Map map)
        {
            var index = map.Index;
            if (index >= 0 && index < 20)
            {
                if (maps[index] == map) return comps[index];

                maps[index] = map;
                return comps[index] = map.GetComponent<MapComponent_WindDirection>();
            }
            Log.Warning($"MapComponent_GasDanger not found for map with strange index of {index}");
            return map.GetComponent<MapComponent_WindDirection>();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref windDirection, "windDirection");
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if ((GenTicks.TicksAbs + GetHashCode()) % GenTicks.TickRareInterval == 0)
            {
                windDirection = (windDirection + (float)(rand.NextDouble()*0.6f-0.3f) ) % TwoPI;
            }
        }

        public override void MapGenerated()
        {
            windDirection = (float)rand.NextDouble() * TwoPI;

            base.MapGenerated();

            
            
        }

       
    }

    public static class WindExtensions
    {
        public const float RAD_2_DEGREE = 180 / Mathf.PI;

        public static SimpleCurve WindSpeedCurve = new SimpleCurve
        {
            // beaufort - kph conversion.
            new CurvePoint(0, 0),
            new CurvePoint(1, 2),
            new CurvePoint(2, 6),
            new CurvePoint(3, 12),
            new CurvePoint(4, 20),
            new CurvePoint(5, 29),
            new CurvePoint(6, 39),
            new CurvePoint(7, 50),
            new CurvePoint(8, 62),
            new CurvePoint(9, 75),
            new CurvePoint(10, 89),
            new CurvePoint(11, 103),
            new CurvePoint(12, 118),
            new CurvePoint(13, 150) // imaginary.
        };

        public static Vector2 windVector(this Map map)
        {
            return map.windDirection().toUnitVector() * map.windSpeed(WindSpeedUnit.Beaufort);
        }

        public static float windDirection(this Map map)
        {
            return map.GetComponent<MapComponent_WindDirection>().windDirection;
        }

        public static float windSpeed(this Map map, WindSpeedUnit unit = WindSpeedUnit.KilometersPerHour)
        {
            // on the basis that map.windManager returns a value that is normally somewhere between 0-2,
            // and can go over 2 in extreme cases. Translated to a 1-12 scale, that's a factor of 6ish.
            var beaufort = map.windManager.WindSpeed * 5.6f;
            if (unit == WindSpeedUnit.Beaufort)
            {
                return beaufort;
            }

            var kph = WindSpeedCurve.Evaluate(Mathf.Clamp(beaufort, 0, 13));
            if (unit == WindSpeedUnit.KilometersPerHour)
            {
                return kph;
            }

            return unit switch
            {
                WindSpeedUnit.MetersPerSecond => kph / 3.6f,
                WindSpeedUnit.MilesPerHour => kph / 1.609f,
                WindSpeedUnit.Knots => kph / 1.852f,
                WindSpeedUnit.FeetPerSecond => kph / 1.097f,
                _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
            };
        }

        public static Vector2 toUnitVector(this float radians)
        {
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        public static string windSpeedString(this Map map, WindSpeedUnit unit = WindSpeedUnit.KilometersPerHour)
        {
            var speed = windSpeed(map, unit);
            return unit switch
            {
                WindSpeedUnit.Beaufort => $"{speed:F0} bft",
                WindSpeedUnit.MetersPerSecond => $"{speed:F1} m/s",
                WindSpeedUnit.KilometersPerHour => $"{speed:F0} km/h",
                WindSpeedUnit.MilesPerHour => $"{speed:F0} mph",
                WindSpeedUnit.Knots => $"{speed:F0} kts",
                WindSpeedUnit.FeetPerSecond => $"{speed:F0} f/s",
                _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
            };
        }

        public static void DrawWindVector(Map map, Rect rect)
        {
            var rectSize = Mathf.Min(rect.width, rect.height);
            var iconSize = Mathf.Min(Resources.WindVector.width, Resources.WindVector.height);
            var scale = Mathf.Clamp(map.windSpeed(WindSpeedUnit.Beaufort) / 8f, .3f, 1.1f) * (rectSize / iconSize);
            Widgets.DrawTextureRotated(rect.center, Resources.WindVector, map.windDirection() * RAD_2_DEGREE + 90,
                                       scale);
        }
    }

    public enum WindSpeedUnit
    {
        Beaufort,
        MetersPerSecond,
        KilometersPerHour,
        MilesPerHour,
        Knots,
        FeetPerSecond
    }
}