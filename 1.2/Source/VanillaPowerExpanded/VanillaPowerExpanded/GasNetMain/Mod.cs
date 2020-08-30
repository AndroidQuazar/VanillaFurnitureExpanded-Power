using System;
using GasNetwork.Overlay;
using Verse;
using HarmonyLib;

namespace GasNetwork
{
	public class Mod : Verse.Mod
	{
		public Mod(ModContentPack content) : base(content)
		{

#if DEBUG
			Harmony.DEBUG = true;
#endif
			Harmony harmony = new Harmony("Fluffy.GasNetwork");
			harmony.PatchAll();

            // queue up to be handled after loading defs is finished.
            LongEventHandler.ExecuteWhenFinished( ReplaceLinkedGraphics );
        }

        public static void ReplaceLinkedGraphics()
        {
            Log.Debug( "assigning custom graphics" );
            // replace linked graphic type on any linked gas users. 
            foreach ( var def in DefDatabase<ThingDef>.AllDefsListForReading )
            {
                if ( def.EverTransmitsGas() && ( def.graphicData?.Linked ?? false ) )
                {
                    Log.Debug( $"assigning Graphic_LinkedGas to {def.defName}" );
                    try
                    {
                        // get innerGraphic (we don't want the basic linked wrapper).
                        // note that calling Graphic makes sure it is properly initialized.
                        var innerGraphic = Traverse.Create( def.graphicData.Graphic as Graphic_Linked )
                                                   .Field( "subGraphic" )
                                                   .GetValue<Graphic>();

                        // assign our linked version back to the cached graphic slot.
                        Traverse.Create( def.graphicData )
                                .Field( "cachedGraphic" )
                                .SetValue( new Graphic_LinkedGas( innerGraphic ) );

                        // assign it to the def as well
                        def.graphic = def.graphicData.Graphic;
                    }
                    catch ( Exception e )
                    {
                        Log.Error( $"assigning Graphic_LinkedGas to {def.defName} failed:\n{e}" );
                    }
                }
            }
        }
        
	}
}