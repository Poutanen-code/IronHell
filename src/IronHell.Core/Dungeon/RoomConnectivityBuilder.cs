using IronHell.Core.Randomness;

namespace IronHell.Core.Dungeon;

public readonly record struct RoomConnection(DungeonPosition From, DungeonPosition To);

public sealed record RoomConnectivityResult(
    IReadOnlyList<DungeonPosition> WorkingCenters,
    IReadOnlyList<RoomConnection> Connections,
    IReadOnlyList<TunnelBuildResult> Tunnels);

public static class RoomConnectivityBuilder
{
    public static RoomConnectivityResult Build(DungeonGrid grid, IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(randomSource);

        var centers = grid.RoomCenters.ToList();
        for (var index = 0; index < centers.Count; index++)
        {
            var first = randomSource.Next(0, centers.Count);
            var second = randomSource.Next(0, centers.Count);
            (centers[first], centers[second]) = (centers[second], centers[first]);
        }

        if (centers.Count == 0)
        {
            return new RoomConnectivityResult([], [], []);
        }

        var connections = new List<RoomConnection>(centers.Count);
        var tunnels = new List<TunnelBuildResult>(centers.Count);
        var previous = centers[^1];
        foreach (var center in centers)
        {
            connections.Add(new RoomConnection(center, previous));
            tunnels.Add(DungeonTunnelBuilder.Build(grid, center, previous, randomSource));
            previous = center;
        }

        return new RoomConnectivityResult(centers.AsReadOnly(), connections.AsReadOnly(), tunnels.AsReadOnly());
    }
}