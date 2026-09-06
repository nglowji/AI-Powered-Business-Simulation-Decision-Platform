namespace BusinessTwin.Server.Modules.DigitalTwin;

public sealed class DigitalTwinService
{
    private readonly List<DigitalTwinSnapshot> snapshots = [];

    public DigitalTwinSnapshot CreateSnapshot(
        CreateSnapshotRequest request,
        DateTimeOffset capturedAt)
    {
        if (string.IsNullOrWhiteSpace(request.Source))
        {
            throw new ArgumentException("Snapshot source is required.", nameof(request.Source));
        }

        var snapshot = new DigitalTwinSnapshot(
            Guid.NewGuid(),
            snapshots.Count + 1,
            capturedAt,
            request.Source,
            request.State);
        snapshots.Add(snapshot);
        return snapshot;
    }

    public DigitalTwinSnapshot GetCurrent()
    {
        return snapshots.LastOrDefault()
            ?? throw new InvalidOperationException("No Digital Twin snapshot exists.");
    }

    public IReadOnlyList<DigitalTwinSnapshot> GetTimeline() => snapshots.AsReadOnly();

    public SnapshotComparison Compare(int fromVersion, int toVersion)
    {
        var from = FindVersion(fromVersion);
        var to = FindVersion(toVersion);

        return new SnapshotComparison(
            from.Version,
            to.Version,
            to.State.Revenue - from.State.Revenue,
            to.State.Profit - from.State.Profit,
            to.State.InventoryValue - from.State.InventoryValue,
            to.State.CashFlow - from.State.CashFlow,
            to.State.Orders - from.State.Orders);
    }

    private DigitalTwinSnapshot FindVersion(int version)
    {
        return snapshots.FirstOrDefault(snapshot => snapshot.Version == version)
            ?? throw new KeyNotFoundException($"Digital Twin snapshot version {version} was not found.");
    }
}