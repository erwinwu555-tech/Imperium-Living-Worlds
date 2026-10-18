using Imperium.Core;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}
static void Throws<T>(Action action) where T : Exception
{
    try { action(); } catch (T) { return; }
    throw new Exception($"Expected {typeof(T).Name}");
}
var queue = new ConstructionQueue();
queue.Enqueue(new("expansion", ConstructionKind.DistrictExpansion));
queue.Enqueue(new("house", ConstructionKind.OrdinaryBuilding));
Check(queue.StartNext()?.Id == "house", "Pending expansion must not block an ordinary building.");
Check(queue.StartNext() == null, "Only one active order is allowed.");
Throws<InvalidOperationException>(() => queue.Complete("expansion"));
queue.Complete("house");
Check(queue.StartNext() == null, "Expansion must wait for approval.");
queue.Decide("expansion", true);
Check(queue.StartNext()?.Id == "expansion", "Approved expansion must start.");
queue.Complete("expansion");
queue.Enqueue(new("demolition", ConstructionKind.MajorDemolition));
queue.Decide("demolition", false);
Check(queue.StartNext() == null, "Rejected demolition must never start.");
Throws<InvalidOperationException>(() => queue.Decide("demolition", true));
Throws<InvalidOperationException>(() => queue.Enqueue(new("house", ConstructionKind.OrdinaryBuilding)));
Throws<ArgumentException>(() => new ConstructionOrder("", ConstructionKind.OrdinaryBuilding));
Throws<ArgumentOutOfRangeException>(() => new ConstructionOrder("bad", (ConstructionKind)99));
Throws<KeyNotFoundException>(() => queue.Decide("missing", true));
Console.WriteLine("PASS: construction permissions, queue ordering, rejection and invalid transitions.");


var registry = new CityRegistry();
var first = registry.Create("Northgate", 42, 1200);
var second = registry.Create("Southwatch", 97, 2400);
Check(first.Id.Length == 32, "A city must receive a stable 32-character GUID.");
Check(registry.Get(first.Id).Name == "Northgate", "Registered city must be addressable by ID.");
Check(registry.Cities.Count == 2, "City count must match registered records.");
Check(registry.Cities[0].WorldTile == 42, "City list should have stable tile ordering.");
Throws<InvalidOperationException>(() => registry.Register(first));
Throws<InvalidOperationException>(() => registry.Create("Duplicate tile", 42, 1));
Throws<ArgumentException>(() => new CityRecord("invalid", 1, "Test", 0));
Throws<ArgumentException>(() => new CityRecord(Guid.NewGuid().ToString("N"), 2, " ", 0));
Throws<ArgumentOutOfRangeException>(() => new CityRecord(Guid.NewGuid().ToString("N"), -1, "Test", 0));
Throws<ArgumentOutOfRangeException>(() => new CityRecord(Guid.NewGuid().ToString("N"), 1, "Test", -1));
Throws<KeyNotFoundException>(() => registry.Get(Guid.NewGuid().ToString("N")));
var restored = CityRegistry.Restore(CityRegistry.CurrentSchemaVersion, registry.Cities);
Check(restored.Get(first.Id).FoundedTick == 1200, "Restored city must retain founded tick.");
Check(restored.Get(second.Id).WorldTile == 97, "Restored city must retain world tile.");
Check(CityRegistry.Restore(0, null).Cities.Count == 0, "Empty legacy saves must migrate.");
Throws<NotSupportedException>(() => CityRegistry.Restore(0, registry.Cities));
Throws<NotSupportedException>(() => CityRegistry.Restore(99, registry.Cities));
Throws<InvalidOperationException>(() => CityRegistry.Restore(1, new[] { first, first }));
Console.WriteLine("PASS: city registration, stable identity, restore, legacy migration, invalid saves.");

// Tile lookup and the save/reload comparison line used for in-game validation.
Check(registry.FindByTile(42)?.Id == first.Id, "Tile lookup must return the city on that tile.");
Check(registry.FindByTile(7) is null, "Tile lookup must return null for an empty tile.");
Check(registry.Count == 2, "Count must match registered records.");
var failedRegistry = new CityRegistry();
failedRegistry.Create("Kept", 5, 0);
Throws<InvalidOperationException>(() => failedRegistry.Create("Rejected", 5, 0));
Check(failedRegistry.Count == 1 && failedRegistry.FindByTile(5)?.Name == "Kept",
    "A rejected registration must leave the registry and tile index unchanged.");
Check(restored.FindByTile(97)?.Id == second.Id, "Restore must rebuild the tile index.");
var mixedCase = new CityRecord(first.Id.ToUpperInvariant(), 300, "  Padded  ", 5);
Check(mixedCase.Id == first.Id && mixedCase.Name == "Padded", "IDs normalise to lower case and names are trimmed.");
foreach (var original in registry.Cities)
{
    // Same data must always render the same log line before save and after reload.
    var reloaded = restored.Get(original.Id);
    Check(CityRegistry.Describe(original) == CityRegistry.Describe(reloaded),
        "Describe must be identical for a city before save and after restore.");
}
Check(CityRegistry.Describe(first) == $"id={first.Id} name=\"Northgate\" tile=42 foundedTick=1200",
    "Describe format is part of the in-game test procedure and must stay stable.");
Throws<ArgumentNullException>(() => CityRegistry.Describe(null!));
Console.WriteLine("PASS: tile index, failed registration rollback, ID normalisation, stable describe line.");
