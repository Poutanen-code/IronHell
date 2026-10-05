using IronHell.Core.Definitions;
using IronHell.Core.Monsters;
using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Monsters;

public sealed class MonsterPlacementTests
{
    [Fact]
    public void Place_EmptyTarget_PlacesOneOrdinaryMonster()
    {
        var state = new MonsterRuntimeState();
        var definition = CreateMonster("orc");
        var position = new MonsterPosition(2, 3);

        var result = MonsterPlacementService.Place(state, definition, position, new SeededRandomSource(1));

        Assert.True(result.Success);
        Assert.Equal("monster-1", result.Monster?.InstanceId);
        Assert.Equal("orc", result.Monster?.DefinitionId);
        Assert.Equal(position, result.Monster?.Position);
        Assert.True(state.IsOccupied(position));
        Assert.Single(state.Monsters);
    }

    [Fact]
    public void Place_OccupiedTarget_ReturnsFailureWithoutChangingState()
    {
        var state = new MonsterRuntimeState();
        var first = CreateMonster("first");
        var second = CreateMonster("second");
        var position = new MonsterPosition(2, 3);
        var firstResult = MonsterPlacementService.Place(state, first, position, new SeededRandomSource(1));
        var before = state.Monsters.ToArray();

        var result = MonsterPlacementService.Place(state, second, position, new ScriptedRandomSource());

        Assert.True(firstResult.Success);
        Assert.False(result.Success);
        Assert.Equal(MonsterPlacementFailureReason.Occupied, result.FailureReason);
        Assert.Equal(before, state.Monsters);
        Assert.Equal("first", Assert.Single(state.Monsters).DefinitionId);
        Assert.Null(result.Monster);
    }

    [Fact]
    public void Place_FirstUnique_Succeeds()
    {
        var state = new MonsterRuntimeState();
        var definition = CreateMonster("unique_orc", options: new SpawnFixtureOptions(Unique: true));

        var result = MonsterPlacementService.Place(state, definition, new MonsterPosition(1, 1), new SeededRandomSource(1));

        Assert.True(result.Success);
        Assert.False(state.HasUniqueCapacity(definition));
    }

    [Fact]
    public void Place_SecondInstanceOfSameUnique_IsRejected()
    {
        var state = new MonsterRuntimeState();
        var definition = CreateMonster("unique_orc", options: new SpawnFixtureOptions(Unique: true));
        var firstPosition = new MonsterPosition(1, 1);
        var secondPosition = new MonsterPosition(2, 2);
        MonsterPlacementService.Place(state, definition, firstPosition, new SeededRandomSource(1));

        var random = new ScriptedRandomSource();
        var result = MonsterPlacementService.Place(state, definition, secondPosition, random);

        Assert.False(result.Success);
        Assert.Equal(MonsterPlacementFailureReason.UniqueUnavailable, result.FailureReason);
        Assert.Single(state.Monsters);
        Assert.False(state.IsOccupied(secondPosition));
        Assert.Empty(random.Requests);
    }

    [Fact]
    public void Place_SameNonUniqueDefinitionTwiceAtDifferentTargets_Succeeds()
    {
        var state = new MonsterRuntimeState();
        var definition = CreateMonster("orc");

        var first = MonsterPlacementService.Place(state, definition, new MonsterPosition(1, 1), new SeededRandomSource(1));
        var second = MonsterPlacementService.Place(state, definition, new MonsterPosition(2, 2), new SeededRandomSource(2));

        Assert.True(first.Success);
        Assert.True(second.Success);
        Assert.Equal(2, state.Monsters.Count);
    }

    [Fact]
    public void RuntimeIds_AreDeterministicForIdenticalSuccessfulOperations()
    {
        var firstState = new MonsterRuntimeState();
        var secondState = new MonsterRuntimeState();
        var definition = CreateMonster("orc");

        var first = MonsterPlacementService.Place(firstState, definition, new MonsterPosition(1, 1), new SeededRandomSource(1));
        var second = MonsterPlacementService.Place(secondState, definition, new MonsterPosition(1, 1), new SeededRandomSource(1));

        Assert.Equal(first.Monster, second.Monster);
    }

    [Fact]
    public void AllocationCanSelectAlreadyPlacedUniqueBeforePlacementRejectsIt()
    {
        var state = new MonsterRuntimeState();
        var definition = CreateMonster("unique_orc", options: new SpawnFixtureOptions(Unique: true));
        var initialPlacement = MonsterPlacementService.Place(state, definition, new MonsterPosition(1, 1), new SeededRandomSource(1));
        var baseEntries = MonsterAllocationTableBuilder.Build([definition]);
        var preparedEntries = MonsterAllocationTableBuilder.Prepare(baseEntries, [definition]);
        var effectiveEntries = MonsterAllocationEligibility.Apply(preparedEntries, effectiveLevel: 10);
        var selected = MonsterAllocationSelector.Select(effectiveEntries, new ScriptedRandomSource(0));

        var result = selected is null
            ? throw new InvalidOperationException("Allocation unexpectedly returned no candidate.")
            : MonsterPlacementService.Place(state, definition, new MonsterPosition(2, 2), new ScriptedRandomSource());

        Assert.True(initialPlacement.Success);
        Assert.NotNull(selected);
        Assert.Equal("unique_orc", selected.PreparedEntry.BaseEntry.MonsterDefinitionId);
        Assert.False(result.Success);
        Assert.Equal(MonsterPlacementFailureReason.UniqueUnavailable, result.FailureReason);
        Assert.Single(state.Monsters);
    }

    [Fact]
    public void Place_InitializesHpSpeedAndEnergyInSourceOrder()
    {
        var state = new MonsterRuntimeState();
        var definition = CreateMonster(
            "force_sleep",
            hpDiceCount: 2,
            hpDiceSides: 6,
            options: new SpawnFixtureOptions(MovementSpeed: 120, ForceSleep: true));
        var random = new ScriptedRandomSource(3, 6, 1, 7000, 467);

        var result = MonsterPlacementService.Place(state, definition, new MonsterPosition(2, 3), random);

        Assert.True(result.Success);
        Assert.Equal(9, result.Monster?.SpawnState.MaxHp);
        Assert.Equal(9, result.Monster?.SpawnState.CurrentHp);
        Assert.Equal(121, result.Monster?.SpawnState.MovementSpeed);
        Assert.Equal(467, result.Monster?.SpawnState.Energy);
        Assert.Equal([(1, 7), (1, 7), (-2, 3), (0, 37500), (0, 2343)], random.Requests);
    }

    [Fact]
    public void Place_ForceMaxHpSkipsHpAndUniqueMonsterSkipsSpeedVariance()
    {
        var random = new ScriptedRandomSource(7499);
        var definition = CreateMonster(
            "unique",
            hpDiceCount: 3,
            hpDiceSides: 4,
            options: new SpawnFixtureOptions(Unique: true, MovementSpeed: 110, ForceMaxHp: true));

        var result = MonsterPlacementService.Place(
            new MonsterRuntimeState(),
            definition,
            new MonsterPosition(1, 1),
            random);

        Assert.True(result.Success);
        Assert.Equal(12, result.Monster?.SpawnState.MaxHp);
        Assert.Equal(110, result.Monster?.SpawnState.MovementSpeed);
        Assert.Equal(7499, result.Monster?.SpawnState.Energy);
        Assert.Equal([(0, 37500)], random.Requests);
    }

    [Fact]
    public void Place_NormalSpeedConsumesNoVarianceRoll()
    {
        var random = new ScriptedRandomSource(42);
        var definition = CreateMonster("normal_speed", options: new SpawnFixtureOptions(MovementSpeed: 100));

        var result = MonsterPlacementService.Place(
            new MonsterRuntimeState(),
            definition,
            new MonsterPosition(1, 1),
            random);

        Assert.True(result.Success);
        Assert.Equal(100, result.Monster?.SpawnState.MovementSpeed);
        Assert.Equal(42, result.Monster?.SpawnState.Energy);
        Assert.Equal([(0, 37500)], random.Requests);
    }

    [Theory]
    [InlineData(90, 0)]
    [InlineData(100, 0)]
    [InlineData(110, 1)]
    [InlineData(120, 2)]
    [InlineData(130, 3)]
    [InlineData(140, 3)]
    [InlineData(144, 4)]
    public void Place_UsesSourceExtractEnergyVarianceBands(int baseSpeed, int variance)
    {
        var random = new ScriptedRandomSource(0, 0);
        var definition = CreateMonster("speed_test", options: new SpawnFixtureOptions(MovementSpeed: baseSpeed));

        var result = MonsterPlacementService.Place(
            new MonsterRuntimeState(),
            definition,
            new MonsterPosition(1, 1),
            random);

        Assert.True(result.Success);
        Assert.Equal(baseSpeed, result.Monster?.SpawnState.MovementSpeed);
        var expectedRequests = variance == 0
            ? new[] { (0, 37500) }
            : new[] { (-variance, variance + 1), (0, 37500) };
        Assert.Equal(expectedRequests, random.Requests);
    }

    [Fact]
    public void Place_PreCommitFailuresConsumeNoSpawnStateRng()
    {
        var definition = CreateMonster("orc", options: new SpawnFixtureOptions(MovementSpeed: 110));
        var outOfBoundsRandom = new ScriptedRandomSource();
        var outOfBounds = MonsterPlacementService.Place(
            new MonsterRuntimeState(),
            definition,
            new MonsterPosition(2, 2),
            outOfBoundsRandom,
            new MonsterPlacementSpace(2, 2));
        Assert.Equal(MonsterPlacementFailureReason.OutOfBounds, outOfBounds.FailureReason);
        Assert.Empty(outOfBoundsRandom.Requests);

        var illegalPosition = new MonsterPosition(1, 1);
        var illegalRandom = new ScriptedRandomSource();
        var illegal = MonsterPlacementService.Place(
            new MonsterRuntimeState(),
            definition,
            illegalPosition,
            illegalRandom,
            new MonsterPlacementSpace(3, 3, [illegalPosition]));
        Assert.Equal(MonsterPlacementFailureReason.IllegalCell, illegal.FailureReason);
        Assert.Empty(illegalRandom.Requests);

        var occupiedState = new MonsterRuntimeState();
        var occupiedPosition = new MonsterPosition(1, 1);
        _ = MonsterPlacementService.Place(occupiedState, definition, occupiedPosition, new SeededRandomSource(1));
        var occupiedRandom = new ScriptedRandomSource();
        var occupied = MonsterPlacementService.Place(occupiedState, definition, occupiedPosition, occupiedRandom);
        Assert.Equal(MonsterPlacementFailureReason.Occupied, occupied.FailureReason);
        Assert.Empty(occupiedRandom.Requests);

        var uniqueState = new MonsterRuntimeState();
        var unique = CreateMonster("unique", options: new SpawnFixtureOptions(Unique: true, MovementSpeed: 110));
        _ = MonsterPlacementService.Place(uniqueState, unique, new MonsterPosition(1, 1), new SeededRandomSource(1));
        var uniqueRandom = new ScriptedRandomSource();
        var uniqueRejected = MonsterPlacementService.Place(uniqueState, unique, new MonsterPosition(2, 2), uniqueRandom);
        Assert.Equal(MonsterPlacementFailureReason.UniqueUnavailable, uniqueRejected.FailureReason);
        Assert.Empty(uniqueRandom.Requests);

        var depthRandom = new ScriptedRandomSource();
        var forceDepth = CreateMonster("force_depth", options: new SpawnFixtureOptions(ForceDepth: true, MovementSpeed: 110, NativeLevel: 6));
        var depthRejected = MonsterPlacementService.Place(
            new MonsterRuntimeState(),
            forceDepth,
            new MonsterPosition(1, 1),
            depthRandom,
            depth: 5);
        Assert.Equal(MonsterPlacementFailureReason.ForceDepth, depthRejected.FailureReason);
        Assert.Empty(depthRandom.Requests);
    }

    private static MonsterDefinition CreateMonster(
        string id,
        int hpDiceCount = 1,
        int hpDiceSides = 1,
        SpawnFixtureOptions? options = null)
    {
        options ??= new SpawnFixtureOptions();
        return new MonsterDefinition(
            id,
            new DiceRollDefinition("dice", hpDiceCount, hpDiceSides),
            new MonsterAiDefinition("wanderer", 0, false, false),
            [],
            [],
            new MonsterSensesDefinition(0, MonsterTelepathyProfile.Normal),
            new SpawnPolicy(options.Unique, false, options.ForceDepth, options.ForceMaxHp, options.ForceSleep, false, false, false, false),
            null,
            NativeLevel: options.NativeLevel,
            Rarity: 1,
            MovementSpeed: options.MovementSpeed);
    }

    private sealed record SpawnFixtureOptions(
        bool Unique = false,
        int MovementSpeed = 100,
        bool ForceMaxHp = false,
        bool ForceSleep = false,
        bool ForceDepth = false,
        int NativeLevel = 1);

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
