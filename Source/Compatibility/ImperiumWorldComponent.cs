using Imperium.Core;
using RimWorld.Planet;
using Verse;

namespace Imperium.RimWorld;

/// <summary>
/// World-scoped authoritative registry. RimWorld discovers WorldComponent
/// subclasses and calls ExposeData on normal saves/loads.
/// </summary>
public sealed class ImperiumWorldComponent : WorldComponent
{
    private int schemaVersion = CityRegistry.CurrentSchemaVersion;
    private List<CitySaveRecord>? serializedCities;
    private CityRegistry registry = new();
    private bool readOnlyAfterLoadError;

    public ImperiumWorldComponent(World world) : base(world) { }

    public IReadOnlyList<CityRecord> Cities => registry.Cities;

    public bool IsLocked => readOnlyAfterLoadError;

    public CityRecord? FindCityOnTile(int worldTile) => registry.FindByTile(worldTile);

    /// <summary>
    /// Writes the registry to the log in the stable format players compare
    /// before saving and after reloading. Only called on load or on request.
    /// </summary>
    public void LogSummary(string reason)
    {
        var cities = registry.Cities;
        Log.Message($"[Imperium: Living Worlds] City registry ({reason}): schema={CityRegistry.CurrentSchemaVersion} cities={cities.Count} locked={readOnlyAfterLoadError}");
        foreach (var city in cities)
            Log.Message("[Imperium: Living Worlds]   " + CityRegistry.Describe(city));
    }

    public CityRecord RegisterCity(string name, int worldTile, int foundedTick)
    {
        if (readOnlyAfterLoadError)
            throw new InvalidOperationException("City data cannot be edited while its save schema is unrecognized.");
        return registry.Create(name, worldTile, foundedTick);
    }

    public override void ExposeData()
    {
        base.ExposeData();

        if (Scribe.mode == LoadSaveMode.Saving && readOnlyAfterLoadError)
            Log.Error("[Imperium: Living Worlds] UNSAFE SAVE: city schema failed to load; unknown fields may be lost. Exit without overwriting the original save.");

        if (Scribe.mode == LoadSaveMode.Saving && !readOnlyAfterLoadError)
        {
            schemaVersion = CityRegistry.CurrentSchemaVersion;
            serializedCities = registry.Cities.Select(CitySaveRecord.FromCore).ToList();
        }

        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            // LoadingVars must never keep entries from a previous save.
            registry = new CityRegistry();
            readOnlyAfterLoadError = false;
        }

        Scribe_Values.Look(ref schemaVersion, "imperiumSchemaVersion", 0);
        Scribe_Collections.Look(ref serializedCities, "imperiumCities", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            try
            {
                var records = (serializedCities ?? [])
                    .Select(saved => saved.ToCore());
                registry = CityRegistry.Restore(schemaVersion, records);
                LogSummary("loaded from save");
            }
            catch (Exception error)
            {
                // Keep the registry read-only and alert the player. Scribe cannot
                // guarantee an unknown schema's fields survive re-saving.
                // The player MUST exit without saving and retain a backup.
                readOnlyAfterLoadError = true;
                Log.Error("[Imperium: Living Worlds] City data could not be loaded. EXIT WITHOUT SAVING and keep a backup of this save. City registry locked: " + error);
            }

            if (!readOnlyAfterLoadError)
                serializedCities = null;
        }
    }
}

/// <summary>
/// Verse/Scribe wire format is deliberately separate from immutable Core models.
/// Public parameterless constructor required by LookMode.Deep.
/// </summary>
public sealed class CitySaveRecord : IExposable
{
    public string id = "";
    public int worldTile = -1;
    public string name = "";
    public int foundedTick;

    public CitySaveRecord() { }

    public static CitySaveRecord FromCore(CityRecord city) => new()
    {
        id = city.Id,
        worldTile = city.WorldTile,
        name = city.Name,
        foundedTick = city.FoundedTick,
    };

    public CityRecord ToCore() => new(id, worldTile, name, foundedTick);

    public void ExposeData()
    {
        Scribe_Values.Look(ref id, "id", "");
        Scribe_Values.Look(ref worldTile, "worldTile", -1);
        Scribe_Values.Look(ref name, "name", "");
        Scribe_Values.Look(ref foundedTick, "foundedTick", 0);
    }
}
