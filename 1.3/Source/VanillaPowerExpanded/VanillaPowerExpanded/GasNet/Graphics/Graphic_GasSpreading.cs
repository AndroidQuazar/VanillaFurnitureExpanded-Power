// Copyright Karel Kroeze, 2020-2020.
// VanillaPowerExpanded/VanillaPowerExpanded/Graphic_GasSpreading.cs

using UnityEngine;
using Verse;

namespace GasNetwork
{
    public class Graphic_GasSpreading : Graphic_Gas
    {
        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            return GraphicDatabase.Get<Graphic_GasSpreading>(path, newShader, drawSize, newColor);
        }
    }
}