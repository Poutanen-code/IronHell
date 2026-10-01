using IronHell.Core.Definitions;
using IronHell.Core.Monsters;
using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Monsters;

public sealed class MonsterPlacementSpaceTests
{
    [Fact]
    public void PlacementSpace_InBoundsLegalCell_IsAvailableWhenUnoccupied()
    {
        var space = new MonsterPlacementSpace(3, 3);
        var state = new MonsterRuntimeState();

        Assert.True(space.IsAvailable(new MonsterPosition(1, 2), state));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(3, 0)]
    [InlineData(0, 3)]
    public void PlacementSpace_OutOfBounds_IsUnavailable(int x, int y)
    {
        var space = new MonsterPlacementSpace(3, 3);
        var state = new MonsterRuntimeState();

        Assert.False(space.IsAvailable(new MonsterPosition(x, y), state));
    }

    [Fact]
    public void PlacementSpace_StaticIllegalCell_IsUnavailable()
    {
        var illegal = new MonsterPosition(1, 1);
        var space = new MonsterPlacementSpace(3, 3, [illegal]);
        var state = new MonsterRuntimeState();

        Assert.False(space.IsAvailable(illegal, state));
        Assert.True(space.IsInBounds(illegal));
        Assert.False(space.IsMonsterPlacementLegal(illegal));
    }

    [Fact]
    public void PlacementSpace_OccupiedLegalCell_IsUnavailable()
    {
        var state = new MonsterRuntimeState();
        var position = new MonsterPosition(1, 1);
        MonsterPlacementService.Place(state, CreateMonster("orc"), position);
        var space = new MonsterPlacementSpace(3, 3);

        Assert.False(space.IsAvailable(position, state));
    }

    [Fact]
    public void FindPosition_FirstCandidateLegal_ReturnsImmediately()
    {
        var space = new MonsterPlacementSpace(3, 3);
        var state = new MonsterRuntimeState();
        var random = new ScriptedRandomSource(1, 2);

        var position = MonsterLocationSearch.FindPosition(space, state, random, maxAttempts: 3);

        Assert.Equal(new MonsterPosition(1, 2), position);
        Assert.Equal([(0, 3), (0, 3)], random.Requests);
    }

    [Fact]
    public void FindPosition_IllegalCandidateThenLegalCandidate_ConsumesBothAttemptsInOrder()
    {
        var space = new MonsterPlacementSpace(3, 3, [new MonsterPosition(0, 0)]);
        var state = new MonsterRuntimeState();
        var random = new ScriptedRandomSource(0, 0, 1, 2);

        var position = MonsterLocationSearch.FindPosition(space, state, random, maxAttempts: 2);

        Assert.Equal(new MonsterPosition(1, 2), position);
        Assert.Equal([(0, 3), (0, 3), (0, 3), (0, 3)], random.Requests);
    }

    [Fact]
    public void FindPosition_OccupiedCandidateThenLegalCandidate_Retries()
    {
        var space = new MonsterPlacementSpace(3, 3);
        var state = new MonsterRuntimeState();
        MonsterPlacementService.Place(state, CreateMonster("orc"), new MonsterPosition(0, 0));
        var random = new ScriptedRandomSource(0, 0, 1, 2);

        var position = MonsterLocationSearch.FindPosition(space, state, random, maxAttempts: 2);

        Assert.Equal(new MonsterPosition(1, 2), position);
        Assert.Equal([(0, 3), (0, 3), (0, 3), (0, 3)], random.Requests);
    }

    [Fact]
    public void FindPosition_AllAttemptsFail_ReturnsNoPosition()
    {
        var space = new MonsterPlacementSpace(2, 2, [new MonsterPosition(0, 0), new MonsterPosition(1, 1)]);
        var state = new MonsterRuntimeState();
        var random = new ScriptedRandomSource(0, 0, 1, 1);

        var position = MonsterLocationSearch.FindPosition(space, state, random, maxAttempts: 2);

        Assert.Null(position);
        Assert.Equal(4, random.Requests.Count);
    }

    [Fact]
    public void FindPosition_ZeroAttempts_ReturnsNoPositionWithoutRng()
    {
        var space = new MonsterPlacementSpace(2, 2);
        var state = new MonsterRuntimeState();
        var random = new ScriptedRandomSource();

        var position = MonsterLocationSearch.FindPosition(space, state, random, maxAttempts: 0);

        Assert.Null(position);
        Assert.Empty(random.Requests);
    }

    [Fact]
    public void FindPosition_DoesNotMutateRuntimeState()
    {
        var space = new MonsterPlacementSpace(2, 2);
        var state = new MonsterRuntimeState();
        var random = new ScriptedRandomSource(0, 0);

        _ = MonsterLocationSearch.FindPosition(space, state, random, maxAttempts: 1);

        Assert.Empty(state.Monsters);
    }

    [Fact]
    public void FindPositionThenPlace_LegalCandidate_PlacesAtReturnedPosition()
    {
        var space = new MonsterPlacementSpace(3, 3);
        var state = new MonsterRuntimeState();
        var definition = CreateMonster("orc");
        var position = MonsterLocationSearch.FindPosition(
            space,
            state,
            new ScriptedRandomSource(1, 2),
            maxAttempts: 1);

        Assert.NotNull(position);
        var result = MonsterPlacementService.Place(state, definition, position.Value, space);

        Assert.True(result.Success);
        Assert.Equal(position, result.Monster?.Position);
    }

    [Fact]
    public void Place_OutOfBoundsTarget_FailsWithoutMutation()
    {
        var space = new MonsterPlacementSpace(2, 2);
        var state = new MonsterRuntimeState();

        var result = MonsterPlacementService.Place(state, CreateMonster("orc"), new MonsterPosition(2, 0), space);

        Assert.False(result.Success);
        Assert.Equal(MonsterPlacementFailureReason.OutOfBounds, result.FailureReason);
        Assert.Empty(state.Monsters);
    }

    [Fact]
    public void Place_StaticIllegalTarget_FailsWithoutMutation()
    {
        var illegal = new MonsterPosition(1, 1);
        var space = new MonsterPlacementSpace(2, 2, [illegal]);
        var state = new MonsterRuntimeState();

        var result = MonsterPlacementService.Place(state, CreateMonster("orc"), illegal, space);

        Assert.False(result.Success);
        Assert.Equal(MonsterPlacementFailureReason.IllegalCell, result.FailureReason);
        Assert.Empty(state.Monsters);
    }

    private static MonsterDefinition CreateMonster(string id) =>
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
            Rarity: 1);

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
