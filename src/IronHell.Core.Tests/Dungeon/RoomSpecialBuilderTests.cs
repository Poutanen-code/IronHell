using IronHell.Core.Definitions;
using IronHell.Core.Dungeon;
using IronHell.Core.Monsters;
using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Dungeon;

public sealed class RoomSpecialBuilderTests
{
    [Fact]
    public void RoomDispatcher_NestBranch_UsesPreparationAndSpatialBuilder()
    {
        var grid = new DungeonGrid();
        var inputs = Inputs(CreateMonster("jelly", "i"));
        var result = RoomDispatcher.Generate(grid, 5, inputs, new RepeatingRandomSource(0, 0, 0, 45));

        var room = Assert.Single(result.SuccessfulRooms);
        Assert.Equal(RoomFamily.Nest, room.Family);
        Assert.Equal(96, room.Attempts.Count);
        Assert.Equal(95, room.Attempts.Count(attempt => attempt.Kind == RoomContentAttemptKind.Monster));
        Assert.All(room.Attempts.Where(attempt => attempt.Kind == RoomContentAttemptKind.Monster), attempt =>
        {
            Assert.Equal("jelly", attempt.DefinitionId);
            Assert.False(attempt.AllowGroupExpansion);
        });
        Assert.Single(grid.RoomCenters);
        Assert.True(grid.IsBlockReserved(new RoomBlockPosition(0, 2)));
    }

    [Fact]
    public void RoomDispatcher_PitBranch_UsesPreparationAndExactTierPattern()
    {
        var grid = new DungeonGrid();
        var inputs = Inputs(CreateMonster("orc", "o"));
        var result = RoomDispatcher.Generate(grid, 5, inputs, new RepeatingRandomSource(0, 0, 0, 30));

        var room = Assert.Single(result.SuccessfulRooms);
        Assert.Equal(RoomFamily.Pit, room.Family);
        var monsters = room.Attempts.Where(attempt => attempt.Kind == RoomContentAttemptKind.Monster).ToArray();
        Assert.Equal(95, monsters.Length);
        Assert.All(monsters, attempt =>
        {
            Assert.Equal("orc", attempt.DefinitionId);
            Assert.False(attempt.AllowGroupExpansion);
        });
        Assert.Equal(new DungeonPosition(3, 7), monsters[0].Origin);
        Assert.Equal(new DungeonPosition(3, 25), monsters[18].Origin);
        Assert.Equal(new DungeonPosition(5, 16), monsters[^1].Origin);
        Assert.Single(grid.RoomCenters);
    }

    [Fact]
    public void NestAndPitMonsterRequests_TargetRoomFloor()
    {
        var nestGrid = new DungeonGrid();
        var nest = RoomGeometryBuilder.TryBuildNest(
            nestGrid,
            new RoomBlockPosition(0, 0),
            new MonsterNestPreparationResult(true, MonsterNestFamily.Jelly, Enumerable.Repeat("orc", 64).ToArray()),
            new RepeatingRandomSource());
        var pitGrid = new DungeonGrid();
        var pit = RoomGeometryBuilder.TryBuildPit(
            pitGrid,
            new RoomBlockPosition(0, 0),
            new MonsterPitPreparationResult(
                true,
                new MonsterPitSelection(MonsterPitFamily.Orc, null),
                Enumerable.Repeat("orc", 16).ToArray(),
                Enumerable.Repeat("orc", 16).ToArray(),
                Enumerable.Repeat("orc", 8).ToArray()),
            new RepeatingRandomSource());

        Assert.All(nest.Attempts.Where(attempt => attempt.Kind == RoomContentAttemptKind.Monster), attempt =>
        {
            Assert.Equal(RoomGeometryBuilder.OpenFloorFeatureId, nestGrid.GetFeatureId(attempt.Origin));
            Assert.True(nestGrid.GetCellFlags(attempt.Origin).HasFlag(DungeonCellStates.Room));
        });
        Assert.All(pit.Attempts.Where(attempt => attempt.Kind == RoomContentAttemptKind.Monster), attempt =>
        {
            Assert.Equal(RoomGeometryBuilder.OpenFloorFeatureId, pitGrid.GetFeatureId(attempt.Origin));
            Assert.True(pitGrid.GetCellFlags(attempt.Origin).HasFlag(DungeonCellStates.Room));
        });
    }

    [Fact]
    public void RoomDispatcher_NestPreparationFailure_CountsOneFailedAttempt()
    {
        var grid = new DungeonGrid();
        var inputs = Inputs(CreateMonster("orc", "o"));
        var result = RoomDispatcher.Generate(grid, 5, inputs, new RepeatingRandomSource(0, 0, 0, 45));

        Assert.Equal(50, result.AttemptCount);
        Assert.DoesNotContain(result.SuccessfulRooms, room => room.Family == RoomFamily.Nest);
        Assert.Equal(result.SuccessfulRoomCount, grid.RoomCenters.Count);
    }

    [Fact]
    public void RoomDispatcher_PitPreparationFailure_CountsOneFailedAttempt()
    {
        var grid = new DungeonGrid();
        var inputs = Inputs(CreateMonster("jelly", "i"));
        var result = RoomDispatcher.Generate(grid, 5, inputs, new RepeatingRandomSource(0, 0, 0, 30));

        Assert.Equal(50, result.AttemptCount);
        Assert.DoesNotContain(result.SuccessfulRooms, room => room.Family == RoomFamily.Pit);
        Assert.Equal(result.SuccessfulRoomCount, grid.RoomCenters.Count);
    }

    [Theory]
    [InlineData(0, 8, RoomFamily.GreaterVault)]
    [InlineData(10, 7, RoomFamily.LesserVault)]
    public void RoomDispatcher_VaultBranchesRemainReachable(int familyRoll, int type, RoomFamily expectedFamily)
    {
        var definition = new VaultDefinition("vault", type, 1, ["."]);
        var result = RoomDispatcher.Generate(
            new DungeonGrid(),
            10,
            new RoomDispatchInputs([definition], [], []),
            new RepeatingRandomSource(0, 0, 0, familyRoll));

        Assert.Contains(result.SuccessfulRooms, room => room.Family == expectedFamily);
    }

    private static RoomDispatchInputs Inputs(params MonsterDefinition[] definitions) =>
        new([], definitions, MonsterAllocationTableBuilder.Build(definitions));

    private static MonsterDefinition CreateMonster(string id, string symbol) =>
        new(
            id,
            new DiceRollDefinition("dice", 1, 1),
            new MonsterAiDefinition("wanderer", 0, false, false),
            [],
            [],
            new MonsterSensesDefinition(0, MonsterTelepathyProfile.Normal),
            new SpawnPolicy(false, false, false, false, false, false, false, false, false),
            null,
            NativeLevel: 1,
            Rarity: 1,
            Symbol: symbol,
            Categories: []);

    private sealed class RepeatingRandomSource(params int[] prefix) : IRandomSource
    {
        private readonly Queue<int> _values = new(prefix);

        public int Next(int minInclusive, int maxExclusive)
        {
            var value = _values.Count == 0 ? minInclusive : _values.Dequeue();
            return Math.Clamp(value, minInclusive, maxExclusive - 1);
        }

        public int RollDice(int count, int sides) => throw new NotSupportedException();
    }
}