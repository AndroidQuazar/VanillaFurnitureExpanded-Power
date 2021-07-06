// Graphic_LinkedGas.cs
// Copyright Karel Kroeze, -2020

using System.Linq;
using UnityEngine;
using Verse;

namespace GasNetwork.Overlay
{
    public class Graphic_LinkedGas : Graphic_Linked
    {
        public Graphic_LinkedGas( Graphic innerGraphic ) : base( innerGraphic )
        {
        }

        public override bool ShouldLinkWith( IntVec3 c, Thing parent )
        {
            return c.InBounds( parent.Map ) && GasNetManager.For( parent.Map ).GasNetAt( c ) != null;
        }

        public override void Print( SectionLayer layer, Thing thing, float extraRotation)
        {
            base.Print( layer, thing , extraRotation);

            // also print on cardinal neighbours if that neighbour transmits gas but is not linked itself
            // this draws 'connections' underneath gas users.
            foreach ( var cell in GenAdj.CellsAdjacentCardinal( thing )
                                        .Where( c => GenGrid.InBounds( (IntVec3) c, thing.Map ) ) )
            {
                var neighbour = cell.GetThingList( thing.Map )
                                    .FirstOrDefault( t => t.TryGetComp<CompGas>( out _ )
                                                       && !t.def.graphicData.Linked );
                if ( neighbour != null )
                {
                    var mat = LinkedDrawMatFrom( thing, cell );
                    Printer_Plane.PrintPlane( layer, cell.ToVector3ShiftedWithAltitude( thing.def.Altitude ),
                                              Vector2.one, mat );
                }
            }
        }
    }
}