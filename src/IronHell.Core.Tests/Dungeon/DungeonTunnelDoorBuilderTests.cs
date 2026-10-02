using IronHell.Core.Dungeon;
using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Dungeon;

public sealed class DungeonTunnelDoorBuilderTests
{
    [Fact]
    public void DoorPlacement_NoCandidates_ConsumesNoRng()
    {
        var grid = RockGrid();
        var random = new ScriptedRandomSource();

        var result = DungeonTunnelDoorBuilder.PlaceTunnelDoors(grid, [], [], random);

        Assert.Empty(result.EntranceDoorPositions);
        Assert.Empty(result.JunctionDoorPositions);
        Assert.Empty(random.Requests);
    }

    [Theory]
    [InlineData(24, true)]
    [InlineData(25, false)]
    public void EntranceDoor_UsesStrict25PercentBoundary(int roll, bool expectDoor)
    {
        var grid = RockGrid();
        var position = new DungeonPosition(10, 10);
        var flags = DungeonCellStates.Room | DungeonCellStates.Icky | DungeonCellStates.Glow | DungeonCellStates.TunnelSolid;
        grid.AddCellFlags(position, flags);
        var random = new ScriptedRandomSource(roll, 0);

        var result = DungeonTunnelDoorBuilder.PlaceTunnelDoors(grid, [position], [], random);

        Assert.Equal(expectDoor, result.EntranceDoorPositions.Contains(position));
        Assert.Equal(expectDoor ? DungeonGrid.OpenDoorFeatureId : RoomGeometryBuilder.OpenFloorFeatureId, grid.GetFeatureId(position));
        Assert.Equal(flags, grid.GetCellFlags(position));
        Assert.Equal(expectDoor ? [(0, 100), (0, 1000)] : [(0, 100)], random.Requests);
    }

    [Fact]
    public void EntranceDoors_AreProcessedInPiercingOrder()
    {
        var grid = RockGrid();
        var first = new DungeonPosition(10, 10);
        var second = new DungeonPosition(10, 11);
        var random = new ScriptedRandomSource(0, 0, 24, 400);

        var result = DungeonTunnelDoorBuilder.PlaceEntranceDoors(grid, [first, second], random);

        Assert.Equal([first, second], result);
        Assert.Equal(DungeonGrid.OpenDoorFeatureId, grid.GetFeatureId(first));
        Assert.Equal(DungeonGrid.SecretDoorFeatureId, grid.GetFeatureId(second));
        Assert.Equal([(0, 100), (0, 1000), (0, 100), (0, 1000)], random.Requests);
    }

    [Theory]
    [InlineData(89, true)]
    [InlineData(90, false)]
    public void JunctionDoor_UsesStrict90PercentBoundaryAfterEligibility(int roll, bool expectDoor)
    {
        var grid = EligibleJunctionGrid(new DungeonPosition(10, 10));
        var random = new ScriptedRandomSource(roll, 0);
        var expectedDoor = new DungeonPosition(9, 10);

        var result = DungeonTunnelDoorBuilder.PlaceTunnelDoors(grid, [], [new DungeonPosition(10, 10)], random);

        Assert.Equal(expectDoor, result.JunctionDoorPositions.Contains(expectedDoor));
        Assert.Equal(expectDoor ? DungeonGrid.OpenDoorFeatureId : RoomGeometryBuilder.OpenFloorFeatureId, grid.GetFeatureId(expectedDoor));
        Assert.Equal(expectDoor ? [(0, 100), (0, 1000)] : [(0, 100)], random.Requests);
    }

    [Fact]
    public void JunctionDoor_IneligibleCandidateDoesNotConsumeChanceRoll()
    {
        var grid = RockGrid();
        var roomCell = new DungeonPosition(10, 9);
        grid.SetFeatureId(roomCell, RoomGeometryBuilder.OpenFloorFeatureId);
        grid.AddCellFlags(roomCell, DungeonCellStates.Room);
        var random = new ScriptedRandomSource();

        _ = DungeonTunnelDoorBuilder.PlaceTunnelDoors(grid, [], [new DungeonPosition(10, 10)], random);

        Assert.Empty(random.Requests);
    }

    [Fact]
    public void JunctionDoor_BorderNeighborIsOutsideSourceInBounds()
    {
        var grid = RockGrid();
        var random = new ScriptedRandomSource();

        _ = DungeonTunnelDoorBuilder.PlaceJunctionDoors(grid, [new DungeonPosition(1, 10)], random);

        Assert.Empty(random.Requests);
        Assert.Equal(DungeonGrid.GraniteWallBasicFeatureId, grid.GetFeatureId(new DungeonPosition(0, 10)));
    }

    [Fact]
    public void JunctionDoor_UsesCandidateAndNeighborSourceOrderWithoutDeduplicating()
    {
        var first = new DungeonPosition(10, 10);
        var grid = EligibleJunctionGrid(first);
        var random = new ScriptedRandomSource(90, 90);

        _ = DungeonTunnelDoorBuilder.PlaceTunnelDoors(grid, [], [first, first], random);

        Assert.Equal(2, random.Requests.Count);
        Assert.All(random.Requests, request => Assert.Equal((0, 100), request));
    }

    [Theory]
    [InlineData(0, DungeonGrid.OpenDoorFeatureId, null, null, 1)]
    [InlineData(299, DungeonGrid.OpenDoorFeatureId, null, null, 1)]
    [InlineData(300, DungeonGrid.BrokenDoorFeatureId, null, null, 1)]
    [InlineData(399, DungeonGrid.BrokenDoorFeatureId, null, null, 1)]
    [InlineData(400, DungeonGrid.SecretDoorFeatureId, null, null, 1)]
    [InlineData(599, DungeonGrid.SecretDoorFeatureId, null, null, 1)]
    [InlineData(600, DungeonGrid.ClosedDoorFeatureId, DoorCondition.Closed, 0, 1)]
    [InlineData(899, DungeonGrid.ClosedDoorFeatureId, DoorCondition.Closed, 0, 1)]
    [InlineData(900, DungeonGrid.ClosedDoorFeatureId, DoorCondition.Locked, 1, 2)]
    [InlineData(998, DungeonGrid.ClosedDoorFeatureId, DoorCondition.Locked, 1, 2)]
    [InlineData(999, DungeonGrid.ClosedDoorFeatureId, DoorCondition.Stuck, 1, 2)]
    public void PlaceRandomDoor_UsesVerifiedDistribution(int roll, string featureId, DoorCondition? condition, int? power, int requestCount)
    {
        var grid = RockGrid();
        var position = new DungeonPosition(10, 10);
        var random = new ScriptedRandomSource(roll, 1);

        DungeonDoorGenerator.PlaceRandomDoor(grid, position, random);

        Assert.Equal(featureId, grid.GetFeatureId(position));
        Assert.Equal(condition, grid.GetDoorState(position)?.Condition);
        Assert.Equal(power, grid.GetDoorState(position)?.Power);
        Assert.Equal(requestCount, random.Requests.Count);
        Assert.Equal((0, 1000), random.Requests[0]);
    }

    [Fact]
    public void JunctionDoor_PreservesCellFlagsAndRejectsPermanentTerrain()
    {
        var grid = RockGrid();
        var candidate = new DungeonPosition(10, 10);
        grid.SetFeatureId(candidate, VaultRoomBuilder.PermanentInnerWallFeatureId);
        grid.AddCellFlags(candidate, DungeonCellStates.Room | DungeonCellStates.Icky | DungeonCellStates.Glow | DungeonCellStates.TunnelSolid);
        var random = new ScriptedRandomSource();

        _ = DungeonTunnelDoorBuilder.PlaceTunnelDoors(grid, [], [candidate], random);

        Assert.Equal(VaultRoomBuilder.PermanentInnerWallFeatureId, grid.GetFeatureId(candidate));
        Assert.Equal(DungeonCellStates.Room | DungeonCellStates.Icky | DungeonCellStates.Glow | DungeonCellStates.TunnelSolid, grid.GetCellFlags(candidate));
        Assert.Empty(random.Requests);
    }

    [Fact]
    public void TunnelDoorPlacement_DoesNotExecuteRoomContentAttempts()
    {
        var grid = EligibleJunctionGrid(new DungeonPosition(10, 10));
        var deferred = new RoomContentAttempt(RoomContentAttemptKind.SecretDoor, new DungeonPosition(30, 30));
        var before = grid.GetFeatureId(deferred.Origin);

        _ = DungeonTunnelDoorBuilder.PlaceTunnelDoors(grid, [], [new DungeonPosition(10, 10)], new ScriptedRandomSource(90));

        Assert.Equal(before, grid.GetFeatureId(deferred.Origin));
        Assert.Equal(RoomContentAttemptKind.SecretDoor, deferred.Kind);
    }

    [Fact]
    public void TunnelDoorPlacement_IdenticalCandidatesAndRng_ReplaysExactly()
    {
        var firstGrid = EligibleJunctionGrid(new DungeonPosition(10, 10));
        var secondGrid = EligibleJunctionGrid(new DungeonPosition(10, 10));
        var first = DungeonTunnelDoorBuilder.PlaceTunnelDoors(firstGrid, [new DungeonPosition(30, 30)], [new DungeonPosition(10, 10)], new ScriptedRandomSource(24, 450, 0, 500));
        var second = DungeonTunnelDoorBuilder.PlaceTunnelDoors(secondGrid, [new DungeonPosition(30, 30)], [new DungeonPosition(10, 10)], new ScriptedRandomSource(24, 450, 0, 500));

        Assert.Equal(first.EntranceDoorPositions, second.EntranceDoorPositions);
        Assert.Equal(first.JunctionDoorPositions, second.JunctionDoorPositions);
        Assert.Equal(firstGrid.GetFeatureId(new DungeonPosition(30, 30)), secondGrid.GetFeatureId(new DungeonPosition(30, 30)));
        Assert.Equal(firstGrid.GetFeatureId(new DungeonPosition(9, 10)), secondGrid.GetFeatureId(new DungeonPosition(9, 10)));
    }

    [Fact]
    public void ConnectivityAndTunnelDoorPhase_ComposeInSourceOrder()
    {
        var grid = RockGrid();
        grid.SetFeatureId(new DungeonPosition(10, 10), RoomGeometryBuilder.OpenFloorFeatureId);
        grid.SetFeatureId(new DungeonPosition(10, 11), RoomGeometryBuilder.OuterWallFeatureId);
        grid.SetFeatureId(new DungeonPosition(10, 12), RoomGeometryBuilder.OpenFloorFeatureId);
        var tunnel = new TunnelBuildResult(true, [], [new DungeonPosition(10, 11)], [new DungeonPosition(10, 12)]);
        var random = new ScriptedRandomSource(0, 500, 99);

        var doors = DungeonTunnelDoorBuilder.PlaceTunnelDoors(grid, tunnel.PiercedWallPositions, tunnel.DoorCandidatePositions, random);

        Assert.Equal([new DungeonPosition(10, 11)], doors.EntranceDoorPositions);
        Assert.Equal(DungeonGrid.SecretDoorFeatureId, grid.GetFeatureId(new DungeonPosition(10, 11)));
        Assert.Equal([(0, 100), (0, 1000), (0, 100)], random.Requests);
    }

    [Fact]
    public void ConnectivityTunnelAndDoors_ComposeInVerifiedOrder()
    {
        var grid = RockGrid();
        var firstCenter = new DungeonPosition(10, 10);
        var secondCenter = new DungeonPosition(10, 43);
        grid.SetFeatureId(firstCenter, RoomGeometryBuilder.OpenFloorFeatureId);
        grid.AddCellFlags(firstCenter, DungeonCellStates.Room);
        grid.SetFeatureId(secondCenter, RoomGeometryBuilder.OpenFloorFeatureId);
        grid.AddCellFlags(secondCenter, DungeonCellStates.Room);
        grid.SetFeatureId(new DungeonPosition(10, 11), RoomGeometryBuilder.OuterWallFeatureId);
        Assert.True(grid.TryCommitRoom(new RoomBlockPosition(0, 0), new RoomBlockFootprint(1, 1), firstCenter));
        Assert.True(grid.TryCommitRoom(new RoomBlockPosition(0, 3), new RoomBlockFootprint(1, 1), secondCenter));
        var rolls = Enumerable.Repeat(0, 4)
            .Concat(Enumerable.Repeat(99, 33))
            .Concat([0, 500])
            .Concat(Enumerable.Repeat(99, 200))
            .ToArray();

        var result = RoomConnectivityBuilder.Build(grid, new ScriptedRandomSource(rolls));

        Assert.Equal([new DungeonPosition(10, 11)], result.EntranceDoorPositions);
        Assert.Equal(DungeonGrid.SecretDoorFeatureId, grid.GetFeatureId(new DungeonPosition(10, 11)));
        Assert.Equal([firstCenter, secondCenter], grid.RoomCenters);
    }

    private static DungeonGrid EligibleJunctionGrid(DungeonPosition junction)
    {
        var grid = RockGrid();
        var doorPosition = new DungeonPosition(junction.Row - 1, junction.Column);
        grid.SetFeatureId(doorPosition, RoomGeometryBuilder.OpenFloorFeatureId);
        grid.SetFeatureId(new DungeonPosition(doorPosition.Row - 1, doorPosition.Column), RoomGeometryBuilder.OpenFloorFeatureId);
        grid.SetFeatureId(new DungeonPosition(doorPosition.Row + 1, doorPosition.Column), RoomGeometryBuilder.OpenFloorFeatureId);
        grid.SetFeatureId(new DungeonPosition(doorPosition.Row, doorPosition.Column - 1), DungeonGrid.GraniteWallBasicFeatureId);
        grid.SetFeatureId(new DungeonPosition(doorPosition.Row, doorPosition.Column + 1), DungeonGrid.GraniteWallBasicFeatureId);
        return grid;
    }

    private static DungeonGrid RockGrid()
    {
        var grid = new DungeonGrid();
        grid.InitializeRock();
        return grid;
    }

    private sealed class ScriptedRandomSource(params int[] values) : IRandomSource
    {
        private readonly Queue<int> _values = new(values);

        public List<(int MinInclusive, int MaxExclusive)> Requests { get; } = [];

        public int Next(int minInclusive, int maxExclusive)
        {
            Requests.Add((minInclusive, maxExclusive));
            if (!_values.TryDequeue(out var value))
            {
                throw new InvalidOperationException("The scripted random source ran out of values.");
            }

            if (value < minInclusive || value >= maxExclusive)
            {
                throw new InvalidOperationException($"Scripted value {value} is outside [{minInclusive}, {maxExclusive}).");
            }

            return value;
        }

        public int RollDice(int count, int sides) => throw new NotSupportedException();
    }
}
