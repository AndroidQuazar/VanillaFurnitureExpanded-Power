using System;
using System.Reflection;
using Verse;

namespace VanillaPowerExpanded
{

    [StaticConstructorOnStartup]
    public static class NonPublicFields
    {

        public static FieldInfo ThingDef_allRecipesCached = typeof(ThingDef).GetField("allRecipesCached", BindingFlags.Instance | BindingFlags.NonPublic);

    }

}
