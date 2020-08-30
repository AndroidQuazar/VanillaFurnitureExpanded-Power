using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VanillaPowerExpanded
{
    public static class PipeNetMaker
    {
        private static IEnumerable<CompPipe> ContiguousPowerBuildings(Building root)
        {
            closedSet.Clear();
            openSet.Clear();
            currentSet.Clear();
            openSet.Add(root);
            do
            {
                foreach (Building item in openSet)
                {
                    closedSet.Add(item);
                }
                HashSet<Building> hashSet = currentSet;
                currentSet = openSet;
                openSet = hashSet;
                openSet.Clear();
                foreach (Building building in currentSet)
                {
                    foreach (IntVec3 c in GenAdj.CellsAdjacentCardinal(building))
                    {
                        if (c.InBounds(building.Map))
                        {
                            List<Thing> thingList = c.GetThingList(building.Map);
                            for (int i = 0; i < thingList.Count; i++)
                            {
                                Building building2 = thingList[i] as Building;
                                if (building2 != null && building2.def.defName!="PowerConduit"&&!PipeNetMaker.openSet.Contains(building2) && !PipeNetMaker.currentSet.Contains(building2) && !PipeNetMaker.closedSet.Contains(building2))
                                {
                                    PipeNetMaker.openSet.Add(building2);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            while (openSet.Count > 0);
            CompPipe[] result = (from b in closedSet
                                  select b.GetComp<CompPipe>()).ToArray<CompPipe>();
            closedSet.Clear();
            openSet.Clear();
            currentSet.Clear();
            return result;
        }

        public static GasPipeNet NewPowerNetStartingFrom(Building root)
        {
            return new GasPipeNet(ContiguousPowerBuildings(root));
        }

        public static void UpdateVisualLinkagesFor(GasPipeNet net)
        {
        }

        private static HashSet<Building> closedSet = new HashSet<Building>();

        private static HashSet<Building> openSet = new HashSet<Building>();

        private static HashSet<Building> currentSet = new HashSet<Building>();
    }
}
