using Imperium.Core;
using LudeonTK;
using RimWorld;
using Verse;

namespace Imperium.RimWorld;

/// <summary>
/// Development-mode only commands for validating M1 save/reload in a real game.
/// They appear in the Dev Mode debug actions menu under "Imperium" and are not
/// a player feature: no buildings are placed and no map is modified.
/// </summary>
public static class ImperiumDebugActions
{
    private const string Category = "Imperium";

    [DebugAction(Category, "Register city on current map", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void RegisterCityOnCurrentMap()
    {
        var registry = Find.World?.GetComponent<ImperiumWorldComponent>();
        var map = Find.CurrentMap;
        if (registry is null || map is null)
        {
            Report("No world or current map; nothing registered.", MessageTypeDefOf.RejectInput);
            return;
        }
        if (registry.IsLocked)
        {
            Report("City registry is locked because its save data failed to load. See the log; exit without saving.", MessageTypeDefOf.RejectInput);
            return;
        }

        int tile = map.Tile;
        var existing = registry.FindCityOnTile(tile);
        if (existing is not null)
        {
            Report("This map's tile already has a city: " + CityRegistry.Describe(existing), MessageTypeDefOf.RejectInput);
            return;
        }

        var name = map.Parent?.Label;
        if (string.IsNullOrWhiteSpace(name))
            name = "City at tile " + tile;

        var city = registry.RegisterCity(name, tile, Find.TickManager.TicksGame);
        Report("Registered city " + CityRegistry.Describe(city), MessageTypeDefOf.PositiveEvent);
        registry.LogSummary("after registration");
    }

    [DebugAction(Category, "Log city registry", allowedGameStates = AllowedGameStates.Playing)]
    private static void LogCityRegistry()
    {
        var registry = Find.World?.GetComponent<ImperiumWorldComponent>();
        if (registry is null)
        {
            Log.Warning("[Imperium: Living Worlds] No world loaded; city registry unavailable.");
            return;
        }
        registry.LogSummary("manual check");
        Messages.Message("[Imperium] City registry written to the log (" + registry.Cities.Count + " cities).", MessageTypeDefOf.NeutralEvent, false);
    }

    private static void Report(string text, MessageTypeDef type)
    {
        Log.Message("[Imperium: Living Worlds] " + text);
        Messages.Message("[Imperium] " + text, type, false);
    }
}
