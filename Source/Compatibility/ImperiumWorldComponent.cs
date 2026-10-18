using Imperium.Core;
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
    private bool preserveUnrecognizedSaveData;

    public ImperiumWorldComponent(World world) : base(world) { }

    public IReadOnlyList<CityRecord> Cities => registry.Cities;

    public CityRecord RegisterCity(string name, int worldTile, int foundedTick)
    {
        if (preserveUnrecognizedSaveData)
            throw new InvalidOperationException("City data cannot be edited while its save schema is unrecognized.");
        return registry.Create(name, worldTile, foundedTick);
    }

    public override void ExposeData()
    {
        base.ExposeData();

        if (Scribe.mode == LoadSaveMode.Saving && !preserveUnrecognizedSaveData)
        {
            schemaVersion = CityRegistry.CurrentSchemaVersion;
            serializedCities = registry.Cities.Select(CitySaveRecord.FromCore).ToList();
        }

        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            // LoadingVars must never keep entries from a previous save.
            registry = new CityRegistry();
            preserveUnrecognizedSaveData = false;
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
                Log.Message("[Imperium: Living Worlds] Restored " + registry.Cities.Count + " city records.");
            }
            catch (Exception error)
            {
                // Avoid overwriting data from an unknown or malformed schema
                // on the next save. Keep raw data and original schema version.
                preserveUnrecognizedSaveData = true;
                Log.Error("[Imperium: Living Worlds] City data not loaded; raw save records preserved: " + error);
            }

            if (!preserveUnrecognizedSaveData)
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
