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
        public int numberToSpawn;
        public List<string> terrainValidationAllowed;
        public List<string> terrainValidationDisallowed;
        public string disallowedBiome;

    }
}