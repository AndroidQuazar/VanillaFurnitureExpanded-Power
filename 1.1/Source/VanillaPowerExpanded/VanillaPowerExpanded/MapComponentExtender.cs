using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using RimWorld.Planet;

namespace VanillaPowerExpanded
{
    public class MapComponentExtender : MapComponent
    {

        public bool verifyFirstTime = true;
        public int spawnCounter = 0;

        public MapComponentExtender(Map map) : base(map)
        {

        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<bool>(ref this.verifyFirstTime, "verifyFirstTime", true, true);


        }

        public override void FinalizeInit()
        {

            base.FinalizeInit();

            if (verifyFirstTime)
            {
                this.doMapSpawns();
                

            }

        }

        public void doMapSpawns()
        {

            IEnumerable<IntVec3> tmpTerrain = map.AllCells.InRandomOrder();

            
                foreach (SpecialPowerSpawnsDef element in DefDatabase<SpecialPowerSpawnsDef>.AllDefs.Where(element => element.disallowedBiome != map.Biome.defName))
                {
                    int extraGeneration = 0;
                    foreach (string biome in element.biomesWithExtraGeneration)
                    {
                        if (map.Biome.defName == biome)
                        {
                            extraGeneration = element.extraGeneration;
                        }
                       
                    }
                    bool canSpawn = true;
                    if (spawnCounter == 0)
                    {
                        spawnCounter = Rand.RangeInclusive(element.numberToSpawn.min, element.numberToSpawn.max) + extraGeneration;
                    //Log.Message(spawnCounter.ToString());
                   
                    }
                    foreach (IntVec3 c in tmpTerrain)
                    {
                        if (!c.CloseToEdge(map, 15))
                        {
                        TerrainDef terrain = c.GetTerrain(map);

                        foreach (string allowed in element.terrainValidationAllowed)
                        {
                            if (terrain.defName == allowed)
                            {
                                canSpawn = true;
                                break;
                            }
                            canSpawn = false;
                        }
                        foreach (string notAllowed in element.terrainValidationDisallowed)
                        {
                            if (terrain.defName == notAllowed)
                            {
                                canSpawn = false;
                                break;
                            }
                        }

                        if (canSpawn)
                        {

                            Thing thing = (Thing)ThingMaker.MakeThing(element.thingDef, null);
                            CellRect occupiedRect = GenAdj.OccupiedRect(c, thing.Rotation, thing.def.Size);

                            if (occupiedRect.InBounds(map))
                            {
                                GenSpawn.Spawn(thing, c, map);
                                spawnCounter--;
                            }

                        }
                        if (canSpawn && spawnCounter <= 0)
                        {
                            spawnCounter = 0;
                            break;
                        }
                    }
                        
                    }


                }



            



            this.verifyFirstTime = false;

        }

    }
}
