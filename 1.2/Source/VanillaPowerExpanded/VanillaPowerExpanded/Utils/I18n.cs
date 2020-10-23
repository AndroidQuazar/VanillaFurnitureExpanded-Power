using Verse;

namespace GasNetwork
{
    public static class I18n
    {
        public const string Unit = " m³";

        public static readonly string GasOff                 = "GasOff".Translate();
        public static readonly string NoNetwork              = "NoNetwork".Translate();
        public static readonly string ValveOpen              = "ValveOpen".Translate();
        public static readonly string ValveClosed            = "ValveClosed".Translate();
        public static readonly string Settings_WindSpeedUnit = "Settings.Wind.WindSpeedUnit".Translate();
        public static readonly string Settings_Category      = "Settings.Wind.Category".Translate();
        public static readonly string BufferOff              = "BufferOff".Translate();

        private static string Translate(this string key, params NamedArgument[] args)
        {
            return TranslatorFormattedStringExtensions.Translate(Key(key), args).Resolve();
        }

        private static string Key(string key)
        {
            return $"Fluffy.GasNetwork.{key}";
        }

        public static string GasNetworkInspectString(float consumption, float stored)
        {
            return "GasNetworkInspectString".Translate(consumption.ToString("#0.#") + Unit + "/d",
                                                       stored.ToString("#0.#")      + Unit);
        }

        public static string Stored(float stored, int capacity)
        {
            return "Stored".Translate(stored.ToString("#0.#"), capacity.ToString("#0.#") + Unit);
        }

        public static string Consumption(float consumption)
        {
            return "Consumption".Translate(consumption.ToString("#0.#") + Unit + "/d");
        }

        public static string Production(float production)
        {
            return "Production".Translate(production.ToString("#0.#") + Unit + "/d");
        }

        public static string DesiredBuffer(float desired, float propsMaxBuffer)
        {
            return "DesiredBuffer".Translate(desired.ToString("F0"), propsMaxBuffer.ToString("F0"));
        }
    }
}