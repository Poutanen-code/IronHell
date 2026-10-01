using IronHell.Core.Dungeon;
using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Dungeon;

public sealed class RoomGeometryBuilderTests
{
    [Fact]
    public void RoomFamilyMetadata_MatchesVerifiedDepthsAndFootprints()
    {
        AssertFamily(RoomFamily.Simple, 1, 1, 3);
        AssertFamily(RoomFamily.Overlapping, 1, 1, 3);
        AssertFamily(RoomFamily.Cross, 3, 1, 3);
        AssertFamily(RoomFamily.Large, 3, 1, 3);
        AssertFamily(RoomFamily.Nest, 5, 1, 3);
        AssertFamily(RoomFamily.Pit, 5, 1, 3);
        AssertFamily(RoomFamily.LesserVault, 5, 2, 3);
        AssertFamily(RoomFamily.GreaterVault, 10, 4, 6);
    }

    [Fact]
    public void SimpleRoom_GeometryRollsUseVerifiedRangesAndOrder()
    {
        var random = new ScriptedRandomSource(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);
        var grid = new DungeonGrid();

        var result = RoomGeometryBuilder.TryBuildSimple(grid, new RoomBlockPosition(0, 0), 1, random);

        Assert.True(result.Succeeded);
        Assert.Equal(
            [(1, 26), (1, 5), (1, 4), (1, 12), (1, 12), (0, 20), (0, 50)],
            random.Requests);
    }

    [Fact]
    public void SimpleRoom_MinimumExtents_CreateExpectedRectangle()
    {
        var grid = new DungeonGrid();
        var result = RoomGeometryBuilder.TryBuildSimple(
            grid,
            new RoomBlockPosition(0, 0),
            1,
            new ScriptedRandomSource(1, 1, 1, 1, 1, 1, 1));

        Assert.True(result.Succeeded);
        Assert.Equal(new DungeonPosition(5, 16), result.Center);
        Assert.Equal(RoomGeometryBuilder.OpenFloorFeatureId, grid.GetFeatureId(new DungeonPosition(5, 16)));
        Assert.Equal(RoomGeometryBuilder.OuterWallFeatureId, grid.GetFeatureId(new DungeonPosition(3, 16)));
        Assert.True(grid.GetCellFlags(new DungeonPosition(5, 16)).HasFlag(DungeonCellStates.Room));
        Assert.True(grid.GetCellFlags(new DungeonPosition(5, 16)).HasFlag(DungeonCellStates.Glow));
        Assert.Single(grid.RoomCenters);
    }

    [Fact]
    public void SimpleRoom_PillarSuccess_DoesNotConsumeRaggedRoll()
    {
        var grid = new DungeonGrid();
        var random = new ScriptedRandomSource(1, 1, 1, 1, 1, 0);

        var result = RoomGeometryBuilder.TryBuildSimple(grid, new RoomBlockPosition(0, 0), 1, random);

        Assert.True(result.Succeeded);
        Assert.Equal(6, random.Requests.Count);
        Assert.Equal(RoomGeometryBuilder.InnerWallFeatureId, grid.GetFeatureId(new DungeonPosition(4, 15)));
    }

    [Fact]
    public void SimpleRoom_PillarFailure_ThenTestsRagged()
    {
        var grid = new DungeonGrid();
        var random = new ScriptedRandomSource(1, 1, 1, 1, 1, 1, 0);

        var result = RoomGeometryBuilder.TryBuildSimple(grid, new RoomBlockPosition(0, 0), 1, random);

        Assert.True(result.Succeeded);
        Assert.Equal(7, random.Requests.Count);
    }

    [Fact]
    public void OverlappingRoom_UsesOneReservationAndOneCenter()
    {
        var grid = new DungeonGrid();
        var result = RoomGeometryBuilder.TryBuildOverlapping(
            grid,
            new RoomBlockPosition(0, 0),
            1,
            new ScriptedRandomSource(1, 1, 1, 1, 1, 1, 1, 1, 1));

        Assert.True(result.Succeeded);
        Assert.Equal(new DungeonPosition(5, 16), result.Center);
        Assert.Single(grid.RoomCenters);
        Assert.True(grid.IsBlockReserved(new RoomBlockPosition(0, 0)));
        Assert.True(grid.IsBlockReserved(new RoomBlockPosition(0, 2)));
    }

    [Fact]
    public void OverlappingRoom_UsesSingleSharedLightingRoll()
    {
        var grid = new DungeonGrid();
        var random = new ScriptedRandomSource(1, 1, 1, 1, 1, 1, 1, 1, 1);

        _ = RoomGeometryBuilder.TryBuildOverlapping(grid, new RoomBlockPosition(0, 0), 1, random);

        Assert.Equal((1, 26), random.Requests[0]);
        Assert.Equal(9, random.Requests.Count);
    }

    [Fact]
    public void RoomBuilder_OccupiedFootprint_FailsWithoutGeometryOrCenterMutation()
    {
        var grid = new DungeonGrid();
        Assert.True(grid.TryCommitRoom(new RoomBlockPosition(0, 0), new RoomBlockFootprint(1, 3), new DungeonPosition(5, 16)));
        var before = grid.GetFeatureId(new DungeonPosition(5, 16));

        var result = RoomGeometryBuilder.TryBuildSimple(
            grid,
            new RoomBlockPosition(0, 0),
            1,
            new ScriptedRandomSource(1, 1, 1, 1, 1, 1, 1));

        Assert.False(result.Succeeded);
        Assert.Equal(before, grid.GetFeatureId(new DungeonPosition(5, 16)));
        Assert.Single(grid.RoomCenters);
    }

    [Fact]
    public void CrossRoom_BaseGeometryMatchesVerifiedSource()
    {
        var grid = new DungeonGrid();
        var result = RoomGeometryBuilder.TryBuildCross(grid, new RoomBlockPosition(0, 0), 1, new ScriptedRandomSource(1, 3, 3, 0));

        Assert.True(result.Succeeded);
        Assert.Equal(RoomFamily.Cross, result.Family);
        Assert.Equal(0, result.Variant);
        Assert.Equal(new DungeonPosition(5, 16), result.Center);
        Assert.Equal(RoomGeometryBuilder.OpenFloorFeatureId, grid.GetFeatureId(new DungeonPosition(2, 16)));
        Assert.Equal(RoomGeometryBuilder.OpenFloorFeatureId, grid.GetFeatureId(new DungeonPosition(5, 13)));
        Assert.Equal(RoomGeometryBuilder.OuterWallFeatureId, grid.GetFeatureId(new DungeonPosition(1, 16)));
        Assert.True(grid.GetCellFlags(new DungeonPosition(5, 13)).HasFlag(DungeonCellStates.Room));
    }

    [Fact]
    public void CrossRoom_VariantSelectionUsesNext0To4()
    {
        var random = new ScriptedRandomSource(1, 3, 3, 0);
        _ = RoomGeometryBuilder.TryBuildCross(new DungeonGrid(), new RoomBlockPosition(0, 0), 1, random);

        Assert.Equal([(1, 26), (3, 5), (3, 12), (0, 4)], random.Requests);
    }

    [Fact]
    public void CrossRoom_Variant1_WritesVerifiedInnerWalls()
    {
        var grid = new DungeonGrid();
        var result = RoomGeometryBuilder.TryBuildCross(grid, new RoomBlockPosition(0, 0), 1, new ScriptedRandomSource(1, 3, 3, 1));

        Assert.Equal(RoomGeometryBuilder.InnerWallFeatureId, grid.GetFeatureId(new DungeonPosition(4, 15)));
        Assert.Equal(RoomGeometryBuilder.InnerWallFeatureId, grid.GetFeatureId(new DungeonPosition(5, 16)));
        Assert.Equal(RoomGeometryBuilder.InnerWallFeatureId, grid.GetFeatureId(new DungeonPosition(6, 17)));
        Assert.Empty(result.Attempts);
    }

    [Fact]
    public void CrossRoom_Variant2_EmitsAttemptsInSourceOrder()
    {
        var random = new ScriptedRandomSource(1, 3, 3, 2, 0, 0, 0);
        var result = RoomGeometryBuilder.TryBuildCross(new DungeonGrid(), new RoomBlockPosition(0, 0), 1, random);

        Assert.Equal(
            [
                RoomContentAttemptKind.SecretDoor,
                RoomContentAttemptKind.SpecialObject,
                RoomContentAttemptKind.Monster,
                RoomContentAttemptKind.Monster,
                RoomContentAttemptKind.Monster,
                RoomContentAttemptKind.Trap,
                RoomContentAttemptKind.Trap,
            ],
            result.Attempts.Select(attempt => attempt.Kind));
        Assert.Equal(new DungeonPosition(4, 16), result.Attempts[0].Origin);
        Assert.Equal(4, result.Attempts[5].RadiusRows);
    }

    [Fact]
    public void CrossRoom_Variant3_NestedBranchesUseSourceOrder()
    {
        var random = new ScriptedRandomSource(1, 3, 3, 3, 0, 0);
        var result = RoomGeometryBuilder.TryBuildCross(new DungeonGrid(), new RoomBlockPosition(0, 0), 1, random);

        Assert.Equal(6, random.Requests.Count);
        Assert.Equal(4, result.Attempts.Count);
        Assert.All(result.Attempts, attempt => Assert.Equal(RoomContentAttemptKind.SecretDoor, attempt.Kind));
    }

    [Fact]
    public void LargeRoom_BaseGeometryMatchesVerifiedSource()
    {
        var grid = new DungeonGrid();
        var result = RoomGeometryBuilder.TryBuildLarge(grid, new RoomBlockPosition(0, 0), 1, new ScriptedRandomSource(1, 1, 1));

        Assert.True(result.Succeeded);
        Assert.Equal(RoomFamily.Large, result.Family);
        Assert.Equal(new DungeonPosition(5, 16), result.Center);
        Assert.Equal(RoomGeometryBuilder.OuterWallFeatureId, grid.GetFeatureId(new DungeonPosition(0, 16)));
        Assert.Equal(RoomGeometryBuilder.InnerWallFeatureId, grid.GetFeatureId(new DungeonPosition(3, 7)));
        Assert.True(grid.GetCellFlags(new DungeonPosition(5, 16)).HasFlag(DungeonCellStates.Room));
    }

    [Fact]
    public void LargeRoom_Variant2_PreservesObjectStairBoundaryAndCounts()
    {
        var grid = new DungeonGrid();
        var random = new ScriptedRandomSource(1, 2, 1, 1, 1, 0, 1, 7);
        var result = RoomGeometryBuilder.TryBuildLarge(grid, new RoomBlockPosition(0, 0), 1, random);

        Assert.Equal(RoomContentAttemptKind.SpecialObject, result.Attempts[5].Kind);
        Assert.Equal(3, result.Attempts.Count(attempt => attempt.Kind == RoomContentAttemptKind.Monster));
        Assert.Equal(3, result.Attempts.Count(attempt => attempt.Kind == RoomContentAttemptKind.Trap));
        Assert.DoesNotContain(result.Attempts, attempt => attempt.Kind == RoomContentAttemptKind.RandomStair);
        var lockedDoor = Assert.Single(result.Attempts.Where(attempt => attempt.Kind == RoomContentAttemptKind.LockedDoor));
        Assert.Equal(DungeonGrid.ClosedDoorFeatureId, grid.GetFeatureId(lockedDoor.Origin));
        Assert.Equal(new DoorState(DoorCondition.Locked, 7), grid.GetDoorState(lockedDoor.Origin));
        Assert.Equal(
            [(1, 26), (1, 6), (1, 5), (1, 5), (1, 4), (0, 100), (1, 4), (1, 8)],
            random.Requests);
    }

    [Fact]
    public void LargeRoom_Variant4_WritesCheckerboardAndOrderedAttempts()
    {
        var grid = new DungeonGrid();
        var result = RoomGeometryBuilder.TryBuildLarge(grid, new RoomBlockPosition(0, 0), 1, new ScriptedRandomSource(1, 4, 1, 1, 1, 1, 1));

        Assert.Equal(RoomGeometryBuilder.InnerWallFeatureId, grid.GetFeatureId(new DungeonPosition(3, 8)));
        Assert.Equal(8, result.Attempts.Count);
        Assert.Equal(RoomContentAttemptKind.SecretDoor, result.Attempts[0].Kind);
        Assert.Equal(RoomContentAttemptKind.ObjectOrGold, result.Attempts[^1].Kind);
    }

    [Fact]
    public void LargeRoom_Variant5_SupportsBothDoorLayouts()
    {
        var topLayout = RoomGeometryBuilder.TryBuildLarge(new DungeonGrid(), new RoomBlockPosition(0, 0), 1, new ScriptedRandomSource(1, 5, 0, 1, 1, 1, 1, 1, 1, 1, 1));
        var sideLayout = RoomGeometryBuilder.TryBuildLarge(new DungeonGrid(), new RoomBlockPosition(0, 0), 1, new ScriptedRandomSource(1, 5, 99, 1, 1, 1, 1, 1, 1, 1, 1));

        Assert.Equal(4, topLayout.Attempts.Count(attempt => attempt.Kind == RoomContentAttemptKind.SecretDoor));
        Assert.Equal(4, sideLayout.Attempts.Count(attempt => attempt.Kind == RoomContentAttemptKind.SecretDoor));
        Assert.NotEqual(topLayout.Attempts.Select(attempt => attempt.Origin).ToArray(), sideLayout.Attempts.Select(attempt => attempt.Origin).ToArray());
    }

    [Fact]
    public void RoomContentAttempts_DoNotCreateRuntimeMonsters()
    {
        var result = RoomGeometryBuilder.TryBuildLarge(new DungeonGrid(), new RoomBlockPosition(0, 0), 1, new ScriptedRandomSource(1, 1, 1));

        Assert.Contains(result.Attempts, attempt => attempt.Kind == RoomContentAttemptKind.Monster);
        Assert.DoesNotContain(result.Attempts, attempt => attempt.Kind == RoomContentAttemptKind.Object);
    }

    [Fact]
    public void CrossAndLargeReservationFailure_IsAtomic()
    {
        var grid = new DungeonGrid();
        Assert.True(grid.TryCommitRoom(new RoomBlockPosition(0, 0), new RoomBlockFootprint(1, 3), new DungeonPosition(5, 16)));

        var cross = RoomGeometryBuilder.TryBuildCross(grid, new RoomBlockPosition(0, 0), 1, new ScriptedRandomSource(1, 3, 3, 2, 0, 0, 0));
        var large = RoomGeometryBuilder.TryBuildLarge(grid, new RoomBlockPosition(0, 0), 1, new ScriptedRandomSource(1, 1, 1));

        Assert.False(cross.Succeeded);
        Assert.False(large.Succeeded);
        Assert.Empty(cross.Attempts);
        Assert.Empty(large.Attempts);
        Assert.Single(grid.RoomCenters);
    }

    [Fact]
    public void IdenticalRoomInputAndRng_ReproducesGeometryAndAttempts()
    {
        var firstGrid = new DungeonGrid();
        var secondGrid = new DungeonGrid();
        var values = new[] { 1, 3, 4, 3, 0, 0 };

        var first = RoomGeometryBuilder.TryBuildCross(firstGrid, new RoomBlockPosition(0, 0), 1, new ScriptedRandomSource(values));
        var second = RoomGeometryBuilder.TryBuildCross(secondGrid, new RoomBlockPosition(0, 0), 1, new ScriptedRandomSource(values));

        Assert.Equal(first.Attempts, second.Attempts);
        Assert.Equal(first.Center, second.Center);
        Assert.Equal(firstGrid.GetFeatureId(new DungeonPosition(5, 16)), secondGrid.GetFeatureId(new DungeonPosition(5, 16)));
    }

    private static void AssertFamily(RoomFamily family, int minimumDepth, int heightBlocks, int widthBlocks)
    {
        var metadata = RoomFamilies.Get(family);
        Assert.Equal(minimumDepth, metadata.MinimumDepth);
        Assert.Equal(heightBlocks, metadata.Footprint.HeightBlocks);
        Assert.Equal(widthBlocks, metadata.Footprint.WidthBlocks);
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
