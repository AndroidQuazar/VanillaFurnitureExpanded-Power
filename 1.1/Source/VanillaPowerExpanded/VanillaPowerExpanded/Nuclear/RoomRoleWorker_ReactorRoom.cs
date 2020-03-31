﻿using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace VanillaPowerExpanded
{
    class RoomRoleWorker_ReactorRoom : RoomRoleWorker
    {
        public override float GetScore(Room room)
        {
            int num = 0;
            List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
            for (int i = 0; i < containedAndAdjacentThings.Count; i++)
            {
                Thing thing = containedAndAdjacentThings[i];
                if (thing is Building && thing.def.defName == "VPE_NuclearGenerator")
                {
                    num++;
                }
            }
            return 30f * (float)num;
        }
    }
}
