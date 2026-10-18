namespace Imperium.Core;

public enum ConstructionKind { OrdinaryBuilding, DistrictExpansion, MajorDemolition }
public enum ConstructionState { AwaitingApproval, Ready, InProgress, Completed, Rejected }

public sealed class ConstructionOrder
{
    public string Id { get; }
    public ConstructionKind Kind { get; }
    public ConstructionState State { get; internal set; }
    public ConstructionOrder(string id, ConstructionKind kind)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Order ID is required.", nameof(id));
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        Id = id;
        Kind = kind;
        State = kind == ConstructionKind.OrdinaryBuilding
            ? ConstructionState.Ready : ConstructionState.AwaitingApproval;
    }
}

// Pure domain prototype. The game adapter must validate ownership, terrain,
// resources and player approval before issuing real construction commands.
public sealed class ConstructionQueue
{
    private readonly List<ConstructionOrder> orders = [];
    public IReadOnlyList<ConstructionOrder> Orders => orders.AsReadOnly();

    public void Enqueue(ConstructionOrder order)
    {
        ArgumentNullException.ThrowIfNull(order);
        if (orders.Any(x => x.Id == order.Id)) throw new InvalidOperationException("Duplicate order ID.");
        orders.Add(order);
    }

    public void Decide(string id, bool approved)
    {
        var order = Find(id);
        if (order.State != ConstructionState.AwaitingApproval)
            throw new InvalidOperationException("Only pending orders can be approved or rejected.");
        order.State = approved ? ConstructionState.Ready : ConstructionState.Rejected;
    }

    public ConstructionOrder? StartNext()
    {
        if (orders.Any(x => x.State == ConstructionState.InProgress)) return null;
        var next = orders.FirstOrDefault(x => x.State == ConstructionState.Ready);
        if (next != null) next.State = ConstructionState.InProgress;
        return next;
    }

    public void Complete(string id)
    {
        var order = Find(id);
        if (order.State != ConstructionState.InProgress)
            throw new InvalidOperationException("Only an active order can be completed.");
        order.State = ConstructionState.Completed;
    }

    private ConstructionOrder Find(string id) => orders.SingleOrDefault(x => x.Id == id)
        ?? throw new KeyNotFoundException(id);
}
