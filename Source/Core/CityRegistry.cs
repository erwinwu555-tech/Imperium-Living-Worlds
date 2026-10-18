namespace Imperium.Core;

/// <summary>
/// Immutable, game-independent identity and metadata for one city.
/// WorldTile is the RimWorld world-grid tile, not a map coordinate.
/// </summary>
public sealed class CityRecord
{
    public string Id { get; }
    public int WorldTile { get; }
    public string Name { get; }
    public int FoundedTick { get; }

    public CityRecord(string id, int worldTile, string name, int foundedTick)
    {
        if (!Guid.TryParseExact(id, "N", out _))
            throw new ArgumentException("City ID must be a stable 32-character GUID.", nameof(id));
        if (worldTile < 0)
            throw new ArgumentOutOfRangeException(nameof(worldTile));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("City name is required.", nameof(name));
        if (foundedTick < 0)
            throw new ArgumentOutOfRangeException(nameof(foundedTick));

        Id = id.ToLowerInvariant();
        WorldTile = worldTile;
        Name = name.Trim();
        FoundedTick = foundedTick;
    }
}

/// <summary>
/// Single owner for city identities. No RimWorld or Unity dependency.
/// Snapshot data must be converted to/from game-specific Scribe records
/// by the adapter layer.
/// </summary>
public sealed class CityRegistry
{
    public const int CurrentSchemaVersion = 1;

    private readonly Dictionary<string, CityRecord> byId = new(StringComparer.Ordinal);
    // Tile index gives O(1) lookups for map events; never scan all cities per tick.
    private readonly Dictionary<int, CityRecord> byTile = [];

    public int Count => byId.Count;

    public IReadOnlyList<CityRecord> Cities =>
        byId.Values.OrderBy(city => city.WorldTile).ToArray();

    public CityRecord? FindByTile(int worldTile) =>
        byTile.TryGetValue(worldTile, out var city) ? city : null;

    /// <summary>
    /// One stable log line per city. Players compare these lines before saving
    /// and after reloading, so the format must not include volatile data.
    /// </summary>
    public static string Describe(CityRecord city)
    {
        ArgumentNullException.ThrowIfNull(city);
        return $"id={city.Id} name=\"{city.Name}\" tile={city.WorldTile} foundedTick={city.FoundedTick}";
    }

    public CityRecord Create(string name, int worldTile, int foundedTick)
    {
        // Create is intentionally nondeterministic once; the ID is persisted
        // and never regenerated during Restore.
        var city = new CityRecord(Guid.NewGuid().ToString("N"), worldTile, name, foundedTick);
        Register(city);
        return city;
    }

    public void Register(CityRecord city)
    {
        ArgumentNullException.ThrowIfNull(city);
        if (byId.ContainsKey(city.Id))
            throw new InvalidOperationException("Duplicate city ID: " + city.Id);
        if (byTile.ContainsKey(city.WorldTile))
            throw new InvalidOperationException("There is already a city on tile " + city.WorldTile);

        byId.Add(city.Id, city);
        byTile.Add(city.WorldTile, city);
    }

    public CityRecord Get(string id) =>
        byId.TryGetValue(id, out var city) ? city : throw new KeyNotFoundException(id);

    public static CityRegistry Restore(int schemaVersion, IEnumerable<CityRecord>? cities)
    {
        // A missing version key in old saves reads as zero. This version had
        // no persisted city data, so its empty collection is a safe migration.
        if (schemaVersion == 0 && cities is not null && cities.Any())
            throw new NotSupportedException("Legacy schema has no documented city-record layout.");
        if (schemaVersion < 0 || schemaVersion > CurrentSchemaVersion)
            throw new NotSupportedException("Unsupported city save schema: " + schemaVersion);

        var registry = new CityRegistry();
        foreach (var city in cities ?? [])
            registry.Register(city);
        return registry;
    }
}
