using IronHell.Core.Definitions;
using IronHell.Core.Dungeon;
using IronHell.Core.Monsters;
using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Dungeon;

public sealed class RoomMonsterAttemptExecutorTests
{
    [Fact]
    public void Execute_PreparedNestAttemptPlacesExactDefinitionAndPosition()
    {
        var grid = RoomGrid();
        var attempt = PreparedMonster(new DungeonPosition(10, 20), "nest_monster");
        var definitions = Definitions(CreateMonster("nest_monster"));
        var runtime = new MonsterRuntimeState();

        var result = Execute(grid, [attempt], definitions, runtime);

        var placement = Assert.Single(result.ProcessedAttempts).Placement;
        Assert.True(placement.Success);
        Assert.Equal("nest_monster", placement.Monster?.DefinitionId);
        Assert.Equal(new MonsterPosition(20, 10), placement.Monster?.Position);
        Assert.True(runtime.IsOccupied(new MonsterPosition(20, 10)));
        Assert.Empty(result.RemainingAttempts);
    }

    [Fact]
    public void Execute_PreparedPitAttemptPlacesExactDefinitionAndPosition()
    {
        var grid = RoomGrid();
        var attempt = PreparedMonster(new DungeonPosition(11, 21), "pit_monster");
        var definitions = Definitions(CreateMonster("pit_monster"));
        var runtime = new MonsterRuntimeState();

        var result = Execute(grid, [attempt], definitions, runtime);

        var placement = Assert.Single(result.ProcessedAttempts).Placement;
        Assert.True(placement.Success);
        Assert.Equal("pit_monster", placement.Monster?.DefinitionId);
        Assert.Equal(new MonsterPosition(21, 11), placement.Monster?.Position);
    }

    [Fact]
    public void ToMonsterPosition_MapsDungeonRowToYAndColumnToX()
    {
        Assert.Equal(new MonsterPosition(37, 12), RoomMonsterAttemptExecutor.ToMonsterPosition(new DungeonPosition(12, 37)));
    }

    [Fact]
    public void Execute_PreservesAttemptOrderAndRepeatedDefinitionPlacements()
    {
        var grid = RoomGrid();
        var attempts = new[]
        {
            PreparedMonster(new DungeonPosition(10, 10), "orc"),
            PreparedMonster(new DungeonPosition(10, 11), "orc"),
            PreparedMonster(new DungeonPosition(11, 10), "orc"),
        };
        var runtime = new MonsterRuntimeState();

        var result = Execute(grid, attempts, Definitions(CreateMonster("orc")), runtime);

        Assert.Equal(attempts, result.ProcessedAttempts.Select(item => item.Attempt));
        Assert.Equal(3, result.RequestedCount);
        Assert.Equal(3, result.SuccessfulPlacementCount);
        Assert.Equal(["monster-1", "monster-2", "monster-3"], runtime.Monsters.Select(monster => monster.InstanceId));
        Assert.Equal(attempts.Select(attempt => RoomMonsterAttemptExecutor.ToMonsterPosition(attempt.Origin)),
            runtime.Monsters.Select(monster => monster.Position));
    }

    [Fact]
    public void Execute_OccupiedPositionFailsWithoutRelocationOrRetry()
    {
        var grid = RoomGrid();
        var attempt = PreparedMonster(new DungeonPosition(10, 10), "orc");
        var definition = CreateMonster("orc");
        var runtime = new MonsterRuntimeState();
        var position = RoomMonsterAttemptExecutor.ToMonsterPosition(attempt.Origin);
        Assert.True(MonsterPlacementService.Place(
            runtime,
            CreateMonster("occupant"),
            position,
            new SeededRandomSource(1)).Success);
        var random = new RecordingRandomSource();

        var result = Execute(grid, [attempt], Definitions(definition), runtime, randomSource: random);

        var placement = Assert.Single(result.ProcessedAttempts).Placement;
        Assert.False(placement.Success);
        Assert.Equal(MonsterPlacementFailureReason.Occupied, placement.FailureReason);
        Assert.Equal(1, result.RequestedCount);
        Assert.Equal(0, result.SuccessfulPlacementCount);
        Assert.Single(runtime.Monsters);
        Assert.True(runtime.IsOccupied(position));
        Assert.False(runtime.IsOccupied(new MonsterPosition(position.X + 1, position.Y)));
        Assert.Empty(random.Requests);
    }

    [Fact]
    public void Execute_RejectsWallDoorAndStairTerrainButAllowsIckyFlagAtDungeonDepth()
    {
        var grid = RoomGrid();
        var wall = new DungeonPosition(10, 10);
        var door = new DungeonPosition(10, 11);
        var stair = new DungeonPosition(10, 12);
        var ickyFloor = new DungeonPosition(10, 13);
        grid.SetFeatureId(wall, RoomGeometryBuilder.InnerWallFeatureId);
        grid.SetFeatureId(door, DungeonGrid.ClosedDoorFeatureId);
        grid.SetFeatureId(stair, DungeonGrid.DownStairFeatureId);
        grid.AddCellFlags(ickyFloor, DungeonCellStates.Icky);
        var attempts = new[]
        {
            PreparedMonster(wall, "orc"),
            PreparedMonster(door, "orc"),
            PreparedMonster(stair, "orc"),
            PreparedMonster(ickyFloor, "orc"),
        };

        var result = Execute(grid, attempts, Definitions(CreateMonster("orc")), new MonsterRuntimeState());

        Assert.Equal(4, result.RequestedCount);
        Assert.Equal(3, result.ProcessedAttempts.Count(item => item.Placement.FailureReason == MonsterPlacementFailureReason.IllegalCell));
        Assert.True(result.ProcessedAttempts[^1].Placement.Success);
        Assert.True(grid.GetCellFlags(ickyFloor).HasFlag(DungeonCellStates.Icky));
    }

    [Fact]
    public void Execute_CombinesGridBoundsPlacementSpaceAndUniqueLegality()
    {
        var grid = RoomGrid();
        var outOfPlacementSpace = PreparedMonster(new DungeonPosition(10, 15), "orc");
        var illegal = PreparedMonster(new DungeonPosition(10, 11), "orc");
        var uniqueA = PreparedMonster(new DungeonPosition(10, 12), "unique");
        var uniqueB = PreparedMonster(new DungeonPosition(10, 13), "unique");
        var definitions = Definitions(CreateMonster("orc"), CreateMonster("unique", unique: true));
        var space = new MonsterPlacementSpace(15, DungeonGrid.Height, [new MonsterPosition(11, 10)]);

        var result = RoomMonsterAttemptExecutor.Execute(
            grid,
            [outOfPlacementSpace, illegal, uniqueA, uniqueB],
            5,
            definitions,
            new MonsterRuntimeState(),
            new RecordingRandomSource(),
            placementSpace: space);

        Assert.Equal(MonsterPlacementFailureReason.OutOfBounds, result.ProcessedAttempts[0].Placement.FailureReason);
        Assert.Equal(MonsterPlacementFailureReason.IllegalCell, result.ProcessedAttempts[1].Placement.FailureReason);
        Assert.True(result.ProcessedAttempts[2].Placement.Success);
        Assert.Equal(MonsterPlacementFailureReason.UniqueUnavailable, result.ProcessedAttempts[3].Placement.FailureReason);
    }

    [Fact]
    public void Execute_NeverExpandsFriendsOrEscortsForSuppressedRequest()
    {
        var grid = RoomGrid();
        var attempt = PreparedMonster(new DungeonPosition(10, 10), "friend_escort");
        var definition = CreateMonster("friend_escort", friends: true, escort: true, escorts: true);
        var runtime = new MonsterRuntimeState();

        var result = Execute(grid, [attempt], Definitions(definition), runtime);

        Assert.True(Assert.Single(result.ProcessedAttempts).Placement.Success);
        Assert.Single(runtime.Monsters);
    }

    [Fact]
    public void Execute_RejectsOutOfDepthForceDepthMonsterWithoutRetry()
    {
        var attempt = PreparedMonster(new DungeonPosition(10, 10), "force_depth");
        var definition = CreateMonster("force_depth", nativeLevel: 6, forceDepth: true);
        var runtime = new MonsterRuntimeState();

        var result = Execute(RoomGrid(), [attempt], Definitions(definition), runtime, depth: 5);

        Assert.Equal(MonsterPlacementFailureReason.ForceDepth, Assert.Single(result.ProcessedAttempts).Placement.FailureReason);
        Assert.Equal(1, result.RequestedCount);
        Assert.Empty(runtime.Monsters);
    }

    [Fact]
    public void Execute_LeavesOrdinaryVaultAndNonMonsterAttemptsDeferredInOrder()
    {
        var grid = RoomGrid();
        var ordinary = new RoomContentAttempt(RoomContentAttemptKind.Monster, new DungeonPosition(10, 10));
        var secretDoor = new RoomContentAttempt(RoomContentAttemptKind.SecretDoor, new DungeonPosition(10, 11));
        var vaultMonster = new RoomContentAttempt(RoomContentAttemptKind.Monster, new DungeonPosition(10, 12), GenerationDepthOffset: 5, Special: true);
        var trap = new RoomContentAttempt(RoomContentAttemptKind.Trap, new DungeonPosition(10, 13));
        var prepared = PreparedMonster(new DungeonPosition(10, 14), "orc");

        var result = Execute(grid, [ordinary, secretDoor, vaultMonster, trap, prepared], Definitions(CreateMonster("orc")), new MonsterRuntimeState());

        Assert.Single(result.ProcessedAttempts);
        Assert.Equal([ordinary, secretDoor, vaultMonster, trap], result.RemainingAttempts);
    }

    [Fact]
    public void Execute_UnknownPreparedDefinitionFailsClearly()
    {
        var attempt = PreparedMonster(new DungeonPosition(10, 10), "missing");

        var error = Assert.Throws<InvalidOperationException>(() => Execute(
            RoomGrid(),
            [attempt],
            Definitions(),
            new MonsterRuntimeState()));

        Assert.Contains("Prepared monster definition 'missing' was not found", error.Message);
    }

    [Fact]
    public void Execute_RealNestBuilderRequestsPlacePreparedMonstersWithoutAdditionalRng()
    {
        var grid = RockGrid();
        var preparation = new MonsterNestPreparationResult(
            true,
            MonsterNestFamily.Jelly,
            Enumerable.Repeat("nest_monster", MonsterNestPreparer.SampleCount).ToArray());
        var random = new RecordingRandomSource();
        var room = RoomGeometryBuilder.TryBuildNest(grid, new RoomBlockPosition(0, 0), preparation, random);
        var attempts = room.Attempts.Where(attempt => attempt.Kind == RoomContentAttemptKind.Monster).ToArray();
        var drawsBeforeExecution = random.DrawCount;
        var runtime = new MonsterRuntimeState();

        var result = Execute(
            grid,
            attempts,
            Definitions(CreateMonster("nest_monster")),
            runtime,
            randomSource: random,
            placementSpace: PlacementSpace());

        Assert.Equal(95, result.RequestedCount);
        Assert.Equal(95, result.SuccessfulPlacementCount);
        Assert.All(result.ProcessedAttempts, placement =>
        {
            Assert.Equal(placement.Attempt.DefinitionId, placement.Placement.Monster?.DefinitionId);
            Assert.Equal(RoomMonsterAttemptExecutor.ToMonsterPosition(placement.Attempt.Origin), placement.Placement.Monster?.Position);
            Assert.True(grid.GetCellFlags(placement.Attempt.Origin).HasFlag(DungeonCellStates.Room));
        });
        Assert.Equal(drawsBeforeExecution + 95, random.DrawCount);
        Assert.Equal(95, runtime.Monsters.Count);
    }

    [Fact]
    public void Execute_RealPitBuilderRequestsPlacePreparedMonstersWithoutAdditionalRng()
    {
        var grid = RockGrid();
        var preparation = new MonsterPitPreparationResult(
            true,
            new MonsterPitSelection(MonsterPitFamily.Orc, null),
            Enumerable.Repeat("pit_monster", MonsterPitPreparer.SampleCount).ToArray(),
            Enumerable.Repeat("pit_monster", MonsterPitPreparer.SampleCount).ToArray(),
            Enumerable.Repeat("pit_monster", 8).ToArray());
        var random = new RecordingRandomSource();
        var room = RoomGeometryBuilder.TryBuildPit(grid, new RoomBlockPosition(0, 0), preparation, random);
        var attempts = room.Attempts.Where(attempt => attempt.Kind == RoomContentAttemptKind.Monster).ToArray();
        var drawsBeforeExecution = random.DrawCount;
        var runtime = new MonsterRuntimeState();

        var result = Execute(
            grid,
            attempts,
            Definitions(CreateMonster("pit_monster")),
            runtime,
            randomSource: random,
            placementSpace: PlacementSpace());

        Assert.Equal(95, result.RequestedCount);
        Assert.Equal(95, result.SuccessfulPlacementCount);
        Assert.Equal(drawsBeforeExecution + 95, random.DrawCount);
        Assert.Equal(95, runtime.Monsters.Count);
        Assert.Equal(attempts.Select(attempt => RoomMonsterAttemptExecutor.ToMonsterPosition(attempt.Origin)),
            runtime.Monsters.Select(monster => monster.Position));
    }

    [Fact]
    public void Execute_IdenticalPreparedRequestsReplayExactly()
    {
        var attempts = new[]
        {
            PreparedMonster(new DungeonPosition(10, 10), "orc"),
            PreparedMonster(new DungeonPosition(10, 11), "orc"),
        };
        var definitions = Definitions(CreateMonster("orc"));
        var firstRuntime = new MonsterRuntimeState();
        var secondRuntime = new MonsterRuntimeState();

        var first = Execute(RoomGrid(), attempts, definitions, firstRuntime);
        var second = Execute(RoomGrid(), attempts, definitions, secondRuntime);

        Assert.Equal(first.ProcessedAttempts, second.ProcessedAttempts);
        Assert.Equal(first.RemainingAttempts, second.RemainingAttempts);
        Assert.Equal(firstRuntime.Monsters, secondRuntime.Monsters);
    }

    private static RoomMonsterAttemptExecutionResult Execute(
        DungeonGrid grid,
        IReadOnlyList<RoomContentAttempt> attempts,
        IReadOnlyDictionary<string, MonsterDefinition> definitions,
        MonsterRuntimeState runtime,
        MonsterPlacementSpace? placementSpace = null,
        IRandomSource? randomSource = null,
        int depth = 5) =>
        RoomMonsterAttemptExecutor.Execute(
            grid,
            attempts,
            depth,
            definitions,
            runtime,
            randomSource ?? new SeededRandomSource(1),
            placementSpace ?? PlacementSpace());

    private static MonsterPlacementSpace PlacementSpace() =>
        new(DungeonGrid.Width, DungeonGrid.Height);

    private static DungeonGrid RoomGrid()
    {
        var grid = RockGrid();
        for (var row = 5; row < 25; row++)
        {
            for (var column = 5; column < 25; column++)
            {
                var position = new DungeonPosition(row, column);
                grid.SetFeatureId(position, RoomGeometryBuilder.OpenFloorFeatureId);
                grid.AddCellFlags(position, DungeonCellStates.Room);
            }
        }

        return grid;
    }

    private static DungeonGrid RockGrid()
    {
        var grid = new DungeonGrid();
        grid.InitializeRock();
        return grid;
    }

    private static RoomContentAttempt PreparedMonster(DungeonPosition position, string definitionId) =>
        new(
            RoomContentAttemptKind.Monster,
            position,
            DefinitionId: definitionId,
            AllowGroupExpansion: false,
            MonsterSource: RoomMonsterAttemptSource.PreparedNestPit);

    private static IReadOnlyDictionary<string, MonsterDefinition> Definitions(params MonsterDefinition[] definitions) =>
        definitions.ToDictionary(definition => definition.Id, StringComparer.Ordinal);

    private static MonsterDefinition CreateMonster(
        string id,
        bool unique = false,
        bool friends = false,
        bool escort = false,
        bool escorts = false,
        int nativeLevel = 1,
        bool forceDepth = false) =>
        new(
            id,
            new DiceRollDefinition("dice", 1, 1),
            new MonsterAiDefinition("wanderer", 0, false, false),
            [],
            [],
            new MonsterSensesDefinition(0, MonsterTelepathyProfile.Normal),
            new SpawnPolicy(unique, false, forceDepth, false, false, escort, escorts, friends, false),
            NativeLevel: nativeLevel,
            Rarity: 1,
            MovementSpeed: 100);

    private sealed class RecordingRandomSource : IRandomSource
    {
        public int DrawCount { get; private set; }

        public List<(int MinInclusive, int MaxExclusive)> Requests { get; } = [];

        public int Next(int minInclusive, int maxExclusive)
        {
            DrawCount++;
            Requests.Add((minInclusive, maxExclusive));
            return minInclusive;
        }

        public int RollDice(int count, int sides)
        {
            var total = 0;
            for (var index = 0; index < count; index++)
            {
                total += Next(1, sides + 1);
            }

            return total;
        }
    }
}