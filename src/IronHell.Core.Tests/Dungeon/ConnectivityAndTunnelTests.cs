using IronHell.Core.Randomness;
using IronHell.Core.Dungeon;
using Xunit;

namespace IronHell.Core.Tests.Dungeon;

public sealed class ConnectivityAndTunnelTests
{
    [Fact]
    public void TunnelFoundation_UntouchedCellRepresentation_IsExplicit()
    {
        var grid = new DungeonGrid();

        Assert.False(grid.IsRockInitialized);
        Assert.Null(grid.GetFeatureId(new DungeonPosition(10, 10)));
        Assert.False(DungeonTunnelBuilder.Build(grid, new DungeonPosition(10, 10), new DungeonPosition(10, 11), new RepeatingRandomSource(99)).Completed);
        Assert.Null(grid.GetFeatureId(new DungeonPosition(10, 11)));

        grid.InitializeRock();

        Assert.True(grid.IsRockInitialized);
        Assert.Equal(DungeonGrid.GraniteWallBasicFeatureId, grid.GetFeatureId(new DungeonPosition(10, 10)));
    }

    [Fact]
    public void Connectivity_ZeroRoomCenters_DoesNothing()
    {
        var grid = new DungeonGrid();
        grid.InitializeRock();
        var random = new RepeatingRandomSource(0);

        var result = RoomConnectivityBuilder.Build(grid, random);

        Assert.Empty(result.Connections);
        Assert.Empty(result.Tunnels);
        Assert.Empty(result.WorkingCenters);
        Assert.Empty(random.Requests);
    }

    [Fact]
    public void Connectivity_OneRoomCenter_UsesOneSelfConnectionAfterShuffle()
    {
        var grid = CreateGridWithCenters(new DungeonPosition(10, 10));
        var random = new RepeatingRandomSource(0);

        var result = RoomConnectivityBuilder.Build(grid, random);

        Assert.Equal([(new DungeonPosition(10, 10), new DungeonPosition(10, 10))], result.Connections.Select(connection => (connection.From, connection.To)));
        Assert.Single(result.Tunnels);
        Assert.True(result.Tunnels[0].Completed);
        Assert.Equal(2, random.Requests.Count);
        Assert.Equal((0, 1), random.Requests[0]);
        Assert.Equal((0, 1), random.Requests[1]);
    }

    [Fact]
    public void Connectivity_MultipleCenters_UsesShuffledCyclicPairOrderingWithoutMutatingRoomCenters()
    {
        var centers = new[]
        {
            new DungeonPosition(10, 10),
            new DungeonPosition(10, 20),
            new DungeonPosition(10, 30),
        };
        var grid = CreateGridWithCenters(centers);
        var random = new RepeatingRandomSource(99);

        var result = RoomConnectivityBuilder.Build(grid, random);

        Assert.Equal(centers, grid.RoomCenters);
        Assert.Equal(6, random.Requests.Take(6).Count());
        Assert.All(random.Requests.Take(6), request => Assert.Equal((0, 3), request));
        Assert.Equal(result.WorkingCenters[^1], result.Connections[0].To);
        Assert.Equal(result.Connections.Count, centers.Length);
        Assert.Equal(result.Connections[^1].From, result.Connections[0].To);
    }

    [Fact]
    public void TunnelBuilder_StraightConnection_ProducesExactCarvedPath()
    {
        var grid = new DungeonGrid();
        grid.InitializeRock();

        var result = DungeonTunnelBuilder.Build(
            grid,
            new DungeonPosition(10, 10),
            new DungeonPosition(10, 15),
            new RepeatingRandomSource(99));

        Assert.True(result.Completed);
        Assert.Equal(
            [
                new DungeonPosition(10, 11),
                new DungeonPosition(10, 12),
                new DungeonPosition(10, 13),
                new DungeonPosition(10, 14),
                new DungeonPosition(10, 15),
            ],
            result.TunnelPositions);
        Assert.All(result.TunnelPositions, position => Assert.Equal(RoomGeometryBuilder.OpenFloorFeatureId, grid.GetFeatureId(position)));
    }

    [Fact]
    public void TunnelBuilder_OuterWall_RecordsPiercingAndProtectsAdjacentWalls()
    {
        var grid = new DungeonGrid();
        grid.InitializeRock();
        grid.SetFeatureId(new DungeonPosition(10, 11), RoomGeometryBuilder.OuterWallFeatureId);
        grid.SetFeatureId(new DungeonPosition(9, 11), RoomGeometryBuilder.OuterWallFeatureId);

        var result = DungeonTunnelBuilder.Build(
            grid,
            new DungeonPosition(10, 10),
            new DungeonPosition(10, 14),
            new RepeatingRandomSource(99));

        Assert.Equal([new DungeonPosition(10, 11)], result.PiercedWallPositions);
        Assert.Equal(RoomGeometryBuilder.OpenFloorFeatureId, grid.GetFeatureId(new DungeonPosition(10, 11)));
        Assert.True(grid.GetCellFlags(new DungeonPosition(9, 11)).HasFlag(DungeonCellStates.TunnelSolid));
    }

    [Fact]
    public void TunnelBuilder_ExistingFloor_RecordsDeferredDoorCandidate()
    {
        var grid = new DungeonGrid();
        grid.InitializeRock();
        grid.SetFeatureId(new DungeonPosition(10, 11), RoomGeometryBuilder.OpenFloorFeatureId);

        var result = DungeonTunnelBuilder.Build(
            grid,
            new DungeonPosition(10, 10),
            new DungeonPosition(10, 13),
            new RepeatingRandomSource(99));

        Assert.Equal([new DungeonPosition(10, 11)], result.DoorCandidatePositions);
        Assert.Equal(RoomGeometryBuilder.OpenFloorFeatureId, grid.GetFeatureId(new DungeonPosition(10, 11)));
    }

    [Fact]
    public void TunnelBuilder_PermanentWall_IsNotOverwritten()
    {
        var grid = new DungeonGrid();
        grid.InitializeRock();
        grid.SetFeatureId(new DungeonPosition(10, 11), VaultRoomBuilder.PermanentInnerWallFeatureId);

        var result = DungeonTunnelBuilder.Build(
            grid,
            new DungeonPosition(10, 10),
            new DungeonPosition(10, 10),
            new RepeatingRandomSource(99));

        Assert.True(result.Completed);
        Assert.Equal(VaultRoomBuilder.PermanentInnerWallFeatureId, grid.GetFeatureId(new DungeonPosition(10, 11)));
    }

    [Fact]
    public void Connectivity_IdenticalGridCentersAndRng_ReplaysExactly()
    {
        var firstGrid = CreateGridWithCenters(new DungeonPosition(10, 10), new DungeonPosition(10, 20));
        var secondGrid = CreateGridWithCenters(new DungeonPosition(10, 10), new DungeonPosition(10, 20));

        var first = RoomConnectivityBuilder.Build(firstGrid, new RepeatingRandomSource(99));
        var second = RoomConnectivityBuilder.Build(secondGrid, new RepeatingRandomSource(99));

        Assert.Equal(first.Connections, second.Connections);
        Assert.Equal(
            first.Tunnels.Select(tunnel => (tunnel.Completed, tunnel.TunnelPositions.ToArray(), tunnel.PiercedWallPositions.ToArray(), tunnel.DoorCandidatePositions.ToArray())),
            second.Tunnels.Select(tunnel => (tunnel.Completed, tunnel.TunnelPositions.ToArray(), tunnel.PiercedWallPositions.ToArray(), tunnel.DoorCandidatePositions.ToArray())));
        Assert.Equal(firstGrid.GetFeatureId(new DungeonPosition(10, 15)), secondGrid.GetFeatureId(new DungeonPosition(10, 15)));
    }

    private static DungeonGrid CreateGridWithCenters(params DungeonPosition[] centers)
    {
        var grid = new DungeonGrid();
        grid.InitializeRock();
        foreach (var center in centers)
        {
            grid.SetFeatureId(center, RoomGeometryBuilder.OpenFloorFeatureId);
            grid.AddCellFlags(center, DungeonCellStates.Room);
            Assert.True(grid.TryCommitRoom(new RoomBlockPosition(center.Row / 11, center.Column / 11), new RoomBlockFootprint(1, 1), center));
        }

        return grid;
    }

    private sealed class RepeatingRandomSource(params int[] values) : IRandomSource
    {
        private readonly int[] _values = values;
        private int _index;

        public List<(int MinInclusive, int MaxExclusive)> Requests { get; } = [];

        public int Next(int minInclusive, int maxExclusive)
        {
            Requests.Add((minInclusive, maxExclusive));
            var value = _values.Length == 0 ? minInclusive : _values[Math.Min(_index++, _values.Length - 1)];
            return Math.Clamp(value, minInclusive, maxExclusive - 1);
        }

        public int RollDice(int count, int sides) => throw new NotSupportedException();
    }
}