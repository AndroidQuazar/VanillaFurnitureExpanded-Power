// Copyright Karel Kroeze, 2020-2020.
// VanillaPowerExpanded/VanillaPowerExpanded/Mod.cs

using System;
using System.Linq;
using GasNetwork.Overlay;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace GasNetwork
{
    public class Mod : Verse.Mod
    {
        public static GasNetSettings Settings;

        public Mod(ModContentPack content) : base(content)
        {
#if DEBUG
            Harmony.DEBUG = true;
#endif
            var harmony = new Harmony("VFE.Power");
            harmony.PatchAll();

            Settings = GetSettings<GasNetSettings>();

            // queue up to be handled after loading defs is finished.
            LongEventHandler.ExecuteWhenFinished(ReplaceLinkedGraphics);
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return I18n.Settings_Category;
        }

        public static void ReplaceLinkedGraphics()
        {
            Log.Debug("assigning custom graphics");
            // replace linked graphic type on any linked gas users. 
            foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def.EverTransmitsGas() && (def.graphicData?.Linked ?? false))
                {
                    Log.Debug($"assigning Graphic_LinkedGas to {def.defName}");
                    try
                    {
                        // get innerGraphic (we don't want the basic linked wrapper).
                        // note that calling Graphic makes sure it is properly initialized.
                        var innerGraphic = Traverse.Create(def.graphicData.Graphic as Graphic_Linked)
                                                   .Field("subGraphic")
                                                   .GetValue<Graphic>();

                        // assign our linked version back to the cached graphic slot.
                        Traverse.Create(def.graphicData)
                                .Field("cachedGraphic")
                                .SetValue(new Graphic_LinkedGas(innerGraphic));

                        // assign it to the def as well
                        def.graphic = def.graphicData.Graphic;
                    }
                    catch (Exception e)
                    {
                        Log.Error($"assigning Graphic_LinkedGas to {def.defName} failed:\n{e}");
                    }
                }
            }
        }
    }

    public class GasNetSettings : ModSettings
    {
        public WindSpeedUnit unit = WindSpeedUnit.KilometersPerHour;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref unit, "windSpeedUnit", WindSpeedUnit.KilometersPerHour);
        }

        public void DoWindowContents(Rect canvas)
        {
            var options = new Listing_Standard();
            options.Begin(canvas);
            if (options.ButtonTextLabeled(I18n.Settings_WindSpeedUnit, unit.ToString()))
            {
                var unitOptions = Enum.GetValues(typeof(WindSpeedUnit)) as WindSpeedUnit[];
                Find.WindowStack.Add(new FloatMenu(unitOptions
                                                  .Select(option =>
                                                              new FloatMenuOption(
                                                                  option.ToString(), () => unit = option))
                                                  .ToList()));
            }

            options.End();
        }
    }
}