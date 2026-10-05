using IronHell.Core.Dungeon;
using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Dungeon;

public sealed class DungeonStairTests
{
    [Theory]
    [InlineData(49, DungeonGrid.DownStairFeatureId)]
    [InlineData(50, DungeonGrid.UpStairFeatureId)]
    public void PrepareRandomStairFeature_UsesSourceThreshold(int roll, string expectedFeatureId)
    {
        var random = new RecordingRandomSource(roll);

        var featureId = DungeonStairGenerator.PrepareRandomStairFeature(1, false, random);

        Assert.Equal(expectedFeatureId, featureId);
        Assert.Equal([(0, 100)], random.Requests);
    }

    [Theory]
    [InlineData(0, false, DungeonGrid.DownStairFeatureId)]
    [InlineData(1, true, DungeonGrid.UpStairFeatureId)]
    [InlineData(127, false, DungeonGrid.UpStairFeatureId)]
    public void PrepareRandomStairFeature_DepthRestrictionsDoNotRoll(int depth, bool isQuestLevel, string expectedFeatureId)
    {
        var random = new RecordingRandomSource();

        var featureId = DungeonStairGenerator.PrepareRandomStairFeature(depth, isQuestLevel, random);

        Assert.Equal(expectedFeatureId, featureId);
        Assert.Empty(random.Requests);
    }

    [Fact]
    public void Execute_RealLargeRoomRandomStairUsesPreparedDirectionBeforeTrapCount()
    {
        var grid = new DungeonGrid();
        var builderRandom = new RecordingRandomSource(1, 2, 1, 1, 7, 1, 99, 49, 1);
        var room = RoomGeometryBuilder.TryBuildLarge(grid, new RoomBlockPosition(0, 0), 1, builderRandom);
        var stair = Assert.Single(room.Attempts.Where(attempt => attempt.Kind == RoomContentAttemptKind.RandomStair));

        Assert.Equal(DungeonGrid.DownStairFeatureId, stair.PreparedStairFeatureId);
        Assert.Equal(
            [(1, 26), (1, 6), (1, 5), (1, 5), (1, 8), (1, 4), (0, 100), (0, 100), (1, 4)],
            builderRandom.Requests);

        var executorRandom = new RecordingRandomSource();
        var result = RoomStairAttemptExecutor.Execute(grid, room.Attempts, 1, false, executorRandom);

        Assert.Equal(DungeonGrid.DownStairFeatureId, grid.GetFeatureId(stair.Origin));
        Assert.True(grid.GetCellFlags(stair.Origin).HasFlag(DungeonCellStates.Room));
        Assert.Contains(stair, result.ExecutedStairAttempts);
        Assert.DoesNotContain(result.RemainingAttempts, attempt => attempt.Kind == RoomContentAttemptKind.RandomStair);
        Assert.Empty(executorRandom.Requests);
    }

    [Fact]
    public void Execute_MixedAttemptsExecutesStairsOnlyAndPreservesOrderAndCellFlags()
    {
        var grid = RockGrid();
        var firstPosition = new DungeonPosition(10, 10);
        var secondPosition = new DungeonPosition(10, 11);
        var flags = DungeonCellStates.Room | DungeonCellStates.Icky | DungeonCellStates.Glow | DungeonCellStates.TunnelSolid;
        foreach (var position in new[] { firstPosition, secondPosition })
        {
            grid.SetFeatureId(position, RoomGeometryBuilder.OpenFloorFeatureId);
            grid.AddCellFlags(position, flags);
        }

        var monster = new RoomContentAttempt(RoomContentAttemptKind.Monster, firstPosition);
        var firstStair = new RoomContentAttempt(RoomContentAttemptKind.RandomStair, firstPosition,
            PreparedStairFeatureId: DungeonGrid.UpStairFeatureId);
        var trap = new RoomContentAttempt(RoomContentAttemptKind.Trap, secondPosition);
        var secondStair = new RoomContentAttempt(RoomContentAttemptKind.RandomStair, secondPosition,
            PreparedStairFeatureId: DungeonGrid.DownStairFeatureId);
        var item = new RoomContentAttempt(RoomContentAttemptKind.Object, firstPosition);
        var random = new RecordingRandomSource();

        var result = RoomStairAttemptExecutor.Execute(
            grid,
            [monster, firstStair, trap, secondStair, item],
            1,
            false,
            random);

        Assert.Equal([firstStair, secondStair], result.ExecutedStairAttempts);
        Assert.Equal([monster, trap, item], result.RemainingAttempts);
        Assert.Equal(DungeonGrid.UpStairFeatureId, grid.GetFeatureId(firstPosition));
        Assert.Equal(DungeonGrid.DownStairFeatureId, grid.GetFeatureId(secondPosition));
        Assert.Equal(flags, grid.GetCellFlags(firstPosition));
        Assert.Equal(flags, grid.GetCellFlags(secondPosition));
        Assert.Empty(random.Requests);
    }

    [Fact]
    public void Execute_SameCellStairsUseSequentialCleanFloorSemantics()
    {
        var grid = RockGrid();
        var position = new DungeonPosition(10, 10);
        grid.SetFeatureId(position, RoomGeometryBuilder.OpenFloorFeatureId);
        var first = new RoomContentAttempt(RoomContentAttemptKind.RandomStair, position);
        var second = new RoomContentAttempt(RoomContentAttemptKind.RandomStair, position);
        var random = new RecordingRandomSource(49);

        var result = RoomStairAttemptExecutor.Execute(grid, [first, second], 1, false, random);

        Assert.Equal([first, second], result.ExecutedStairAttempts);
        Assert.Equal(DungeonGrid.DownStairFeatureId, grid.GetFeatureId(position));
        Assert.Equal([(0, 100)], random.Requests);
    }

    [Fact]
    public void Execute_NoStairAttemptsLeavesGridAndRandomStreamUnchanged()
    {
        var grid = RockGrid();
        var position = new DungeonPosition(10, 10);
        var attempts = new[]
        {
            new RoomContentAttempt(RoomContentAttemptKind.Monster, position),
            new RoomContentAttempt(RoomContentAttemptKind.Trap, position),
            new RoomContentAttempt(RoomContentAttemptKind.Object, position),
        };
        var random = new RecordingRandomSource();

        var result = RoomStairAttemptExecutor.Execute(grid, attempts, 1, false, random);

        Assert.Empty(result.ExecutedStairAttempts);
        Assert.Equal(attempts, result.RemainingAttempts);
        Assert.Equal(DungeonGrid.GraniteWallBasicFeatureId, grid.GetFeatureId(position));
        Assert.Empty(random.Requests);
    }

    [Fact]
    public void TryPlaceStair_DoesNotOverwritePermanentTerrainOrCellFlags()
    {
        var grid = RockGrid();
        var position = new DungeonPosition(10, 10);
        var flags = DungeonCellStates.Room | DungeonCellStates.Icky | DungeonCellStates.TunnelSolid;
        grid.SetFeatureId(position, VaultRoomBuilder.PermanentInnerWallFeatureId);
        grid.AddCellFlags(position, flags);

        var placed = DungeonStairGenerator.TryPlaceStair(grid, position, DungeonGrid.UpStairFeatureId);

        Assert.False(placed);
        Assert.Equal(VaultRoomBuilder.PermanentInnerWallFeatureId, grid.GetFeatureId(position));
        Assert.Equal(flags, grid.GetCellFlags(position));
    }

    [Fact]
    public void Allocate_UsesExactCountsOrderAndCandidateCoordinates()
    {
        var grid = RockGrid();
        var expectedPositions = new[]
        {
            new DungeonPosition(10, 10),
            new DungeonPosition(20, 20),
            new DungeonPosition(30, 30),
            new DungeonPosition(40, 40),
        };
        foreach (var position in expectedPositions)
        {
            grid.SetFeatureId(position, RoomGeometryBuilder.OpenFloorFeatureId);
        }
        var random = new RecordingRandomSource(3, 10, 10, 20, 20, 30, 30, 1, 40, 40);

        var result = DungeonStairAllocator.Allocate(grid, 1, false, random);

        Assert.Equal(expectedPositions[..3], result.DownStairPositions);
        Assert.Equal([expectedPositions[3]], result.UpStairPositions);
        Assert.Equal(
            [(3, 5), (0, 66), (0, 198), (0, 66), (0, 198), (0, 66), (0, 198), (1, 3), (0, 66), (0, 198)],
            random.Requests);
        Assert.Equal(DungeonGrid.DownStairFeatureId, grid.GetFeatureId(expectedPositions[0]));
        Assert.Equal(DungeonGrid.UpStairFeatureId, grid.GetFeatureId(expectedPositions[3]));
    }

    [Theory]
    [InlineData(12, true)]
    [InlineData(127, false)]
    public void Allocate_QuestAndBottomDepthsForceAllRequestedStairsUp(int depth, bool isQuestLevel)
    {
        var grid = RockGrid();
        var positions = new[]
        {
            new DungeonPosition(10, 10),
            new DungeonPosition(20, 20),
            new DungeonPosition(30, 30),
            new DungeonPosition(40, 40),
        };
        foreach (var position in positions)
        {
            grid.SetFeatureId(position, RoomGeometryBuilder.OpenFloorFeatureId);
        }
        var random = new RecordingRandomSource(3, 10, 10, 20, 20, 30, 30, 1, 40, 40);

        var result = DungeonStairAllocator.Allocate(grid, depth, isQuestLevel, random);

        Assert.Empty(result.DownStairPositions);
        Assert.Equal(positions, result.UpStairPositions);
        Assert.All(positions, position => Assert.Equal(DungeonGrid.UpStairFeatureId, grid.GetFeatureId(position)));
    }

    [Fact]
    public void Allocate_RelaxesWallRequirementAfterExactly3001RejectedCandidates()
    {
        var grid = RockGrid();
        var relaxedPosition = new DungeonPosition(10, 10);
        grid.SetFeatureId(relaxedPosition, RoomGeometryBuilder.OpenFloorFeatureId);
        grid.SetFeatureId(new DungeonPosition(9, 10), RoomGeometryBuilder.OpenFloorFeatureId);
        grid.SetFeatureId(new DungeonPosition(11, 10), RoomGeometryBuilder.OpenFloorFeatureId);
        var otherPositions = new[]
        {
            new DungeonPosition(20, 20),
            new DungeonPosition(30, 30),
            new DungeonPosition(40, 40),
        };
        foreach (var position in otherPositions)
        {
            grid.SetFeatureId(position, RoomGeometryBuilder.OpenFloorFeatureId);
        }
        var random = new RelaxationBoundaryRandomSource();

        var result = DungeonStairAllocator.Allocate(grid, 1, false, random);

        Assert.Equal(relaxedPosition, result.DownStairPositions[0]);
        Assert.Equal(otherPositions[..2], result.DownStairPositions.Skip(1));
        Assert.Equal([otherPositions[2]], result.UpStairPositions);
        Assert.Equal(6012, random.Requests.Count);
        Assert.Equal(3001 * 2 + 2, random.Requests.Skip(1).Take(6004)
            .Count(request => request == (0, 66) || request == (0, 198)));
    }

    [Fact]
    public void StairGeneration_ReplaysExactly()
    {
        var firstGrid = RockGrid();
        var secondGrid = RockGrid();
        var candidates = new[]
        {
            new DungeonPosition(10, 10),
            new DungeonPosition(20, 20),
            new DungeonPosition(30, 30),
            new DungeonPosition(40, 40),
        };
        foreach (var grid in new[] { firstGrid, secondGrid })
        {
            foreach (var position in candidates)
            {
                grid.SetFeatureId(position, RoomGeometryBuilder.OpenFloorFeatureId);
            }
        }
        var values = new[] { 3, 10, 10, 20, 20, 30, 30, 1, 40, 40 };

        var first = DungeonStairAllocator.Allocate(firstGrid, 1, false, new RecordingRandomSource(values));
        var second = DungeonStairAllocator.Allocate(secondGrid, 1, false, new RecordingRandomSource(values));

        Assert.Equal(first.DownStairPositions, second.DownStairPositions);
        Assert.Equal(first.UpStairPositions, second.UpStairPositions);
        Assert.Equal(candidates.Select(firstGrid.GetFeatureId), candidates.Select(secondGrid.GetFeatureId));
        Assert.Equal(candidates.Select(firstGrid.GetCellFlags), candidates.Select(secondGrid.GetCellFlags));
    }

    private static DungeonGrid RockGrid()
    {
        var grid = new DungeonGrid();
        grid.InitializeRock();
        return grid;
    }

    private sealed class RecordingRandomSource : IRandomSource
    {
        private readonly Queue<int> _values;

        public RecordingRandomSource(params int[] values)
        {
            _values = new Queue<int>(values);
        }

        public List<(int MinInclusive, int MaxExclusive)> Requests { get; } = [];

        public int Next(int minInclusive, int maxExclusive)
        {
            Requests.Add((minInclusive, maxExclusive));
            if (!_values.TryDequeue(out var value) || value < minInclusive || value >= maxExclusive)
            {
                throw new InvalidOperationException("The scripted random value is unavailable or outside the requested range.");
            }

            return value;
        }

        public int RollDice(int count, int sides) => throw new NotSupportedException();
    }

    private sealed class RelaxationBoundaryRandomSource : IRandomSource
    {
        public List<(int MinInclusive, int MaxExclusive)> Requests { get; } = [];

        public int Next(int minInclusive, int maxExclusive)
        {
            var call = Requests.Count;
            Requests.Add((minInclusive, maxExclusive));
            return call switch
            {
                0 => 3,
                <= 6004 => 10,
                6005 => 20,
                6006 => 20,
                6007 => 30,
                6008 => 30,
                6009 => 1,
                6010 => 40,
                6011 => 40,
                _ => throw new InvalidOperationException("Unexpected stair RNG request."),
            };
        }

        public int RollDice(int count, int sides) => throw new NotSupportedException();
    }
}