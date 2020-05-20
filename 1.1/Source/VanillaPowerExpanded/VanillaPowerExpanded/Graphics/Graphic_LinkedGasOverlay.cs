// Graphic_LinkedGasOverlay.cs
// Copyright Karel Kroeze, 2020-2020

using RimWorld;
using Verse;

namespace GasNetwork.Overlay
{
    public class Graphic_LinkedGasOverlay : Graphic_LinkedTransmitterOverlay
    {
        public Graphic_LinkedGasOverlay( Graphic innerGraphic ) : base( innerGraphic )
        {
        }

        public override bool ShouldLinkWith( IntVec3 c, Thing parent )
        {
            return c.InBounds( parent.Map ) && GasNetManager.For( parent.Map ).GasNetAt( c ) != null;
        }
    }
}