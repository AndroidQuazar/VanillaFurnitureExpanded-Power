// SectionLayer_GasNetwork.cs
// Copyright Karel Kroeze, 2020-2020

using UnityEngine;
using Verse;

namespace GasNetwork.Overlay
{
    public class SectionLayer_GasNetwork: SectionLayer_Things
    {
        public const MapMeshFlag MapMeshFlag = (MapMeshFlag)1024;

        public SectionLayer_GasNetwork( Section section ) : base( section )
        {
            requireAddToMapMesh = false;

            // we're updating on buildings, and our custom non-existent flag.
            // is RimWorld going to deal with this? Let's find out!
            // (turns out it does, yay)
            relevantChangeTypes = MapMeshFlag.Buildings | MapMeshFlag;
        }

        public override void DrawLayer()
        {
            if (ShouldDrawLayer)
                base.DrawLayer();
        }

        // this seems like a retarded way to check if we should draw the layer, 
        // but it's what vanilla uses, so what the fuck do I know.
        private static int lastGasGridDrawFrame;
        public static void DrawGasGridOverlayThisFrame() => lastGasGridDrawFrame = Time.frameCount;
        public virtual bool ShouldDrawLayer => lastGasGridDrawFrame + 1 >= Time.frameCount;

        protected override void TakePrintFrom( Thing t )
        {
            if ( !t.Faction?.IsPlayer ?? false )
                return;
            
            // vanilla outsources print logic for power to comps, I really don't
            // see the point in that. Just check the comp exists, and handle it 
            // here.
            if ( t.TryGetComp<CompGas>( out var gasComp ) && gasComp.TransmitsGasNow )
                Resources.GasOverlay.Print( this, t );
        }
    }
}