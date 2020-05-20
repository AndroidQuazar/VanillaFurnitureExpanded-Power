using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GasNetwork
{
	static class Log
	{
		[System.Diagnostics.Conditional("DEBUG")]
		public static void Debug( string msg ){
			Message( msg );
		}

		public static void Message(string msg )
		{
			Verse.Log.Message( $"GasNetwork :: {msg}");
		}

        public static void Error( string msg )
        {
            Verse.Log.Error($"GasNetwork :: {msg}" );
		}

        public static void Warning( string msg )
        {
			Verse.Log.Warning( $"GasNetwork :: {msg}" );
        }
    }
}
