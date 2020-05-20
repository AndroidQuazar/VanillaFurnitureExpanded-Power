// Resources.cs
// Copyright Karel Kroeze, 2020-2020

using GasNetwork.Overlay;
using RimWorld;
using UnityEngine;
using Verse;

namespace GasNetwork
{
    [StaticConstructorOnStartup]
    public static class Resources
    {
        public static readonly Material GasOff =
            MaterialPool.MatFrom( "UI/Overlays/GasOff", ShaderDatabase.MetaOverlay );

        public static readonly Material NeedsGas =
            MaterialPool.MatFrom( "UI/Overlays/NeedsGas", ShaderDatabase.MetaOverlay );
        
        public static readonly Graphic_LinkedGasOverlay GasOverlay = new Graphic_LinkedGasOverlay(
            GraphicDatabase.Get<Graphic_Single>("Things/Special/Fluid/GasTransmitterAtlas", ShaderDatabase.MetaOverlay ) );

        public static readonly Material GasBarFilledMaterial =
            SolidColorMaterials.SimpleSolidColorMaterial( new Color( .25f, .69f, .22f, 1f ) );

        public static readonly Material GasBarUnfilledMaterial =
            SolidColorMaterials.SimpleSolidColorMaterial( new Color( .15f, .15f, .15f, 1f ) );
    }
}