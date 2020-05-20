using RimWorld;
using System;
using Verse;
using System.Collections.Generic;


namespace VanillaPowerExpanded
{
    public class SpecialPowerSpawnsDef : Def
    {
        public ThingDef thingDef;
        public bool allowOnWater;
        public IntRange numberToSpawn;
        public List<string> terrainValidationAllowed;
        public List<string> terrainValidationDisallowed;
        public List<string> biomesWithExtraGeneration;
        public int extraGeneration = 0;
        public string disallowedBiome;

    }
}