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
