using UnityEngine;
using Verse;

namespace VanillaPowerExpanded
{
    [StaticConstructorOnStartup]
    public static class GraphicsCache
    {
        public static readonly Texture2D Paste = ContentFinder<Texture2D>.Get("UI/Buttons/Paste");
    }
}