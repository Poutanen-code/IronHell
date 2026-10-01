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
        var definition = CreateMonster("orc", unique: false);
        var position = new MonsterPosition(2, 3);

        var result = MonsterPlacementService.Place(state, definition, position);

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
        var first = CreateMonster("first", unique: false);
        var second = CreateMonster("second", unique: false);
        var position = new MonsterPosition(2, 3);
        var firstResult = MonsterPlacementService.Place(state, first, position);
        var before = state.Monsters.ToArray();

        var result = MonsterPlacementService.Place(state, second, position);

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
        var definition = CreateMonster("unique_orc", unique: true);

        var result = MonsterPlacementService.Place(state, definition, new MonsterPosition(1, 1));

        Assert.True(result.Success);
        Assert.False(state.HasUniqueCapacity(definition));
    }

    [Fact]
    public void Place_SecondInstanceOfSameUnique_IsRejected()
    {
        var state = new MonsterRuntimeState();
        var definition = CreateMonster("unique_orc", unique: true);
        var firstPosition = new MonsterPosition(1, 1);
        var secondPosition = new MonsterPosition(2, 2);
        MonsterPlacementService.Place(state, definition, firstPosition);

        var result = MonsterPlacementService.Place(state, definition, secondPosition);

        Assert.False(result.Success);
        Assert.Equal(MonsterPlacementFailureReason.UniqueUnavailable, result.FailureReason);
        Assert.Single(state.Monsters);
        Assert.False(state.IsOccupied(secondPosition));
    }

    [Fact]
    public void Place_SameNonUniqueDefinitionTwiceAtDifferentTargets_Succeeds()
    {
        var state = new MonsterRuntimeState();
        var definition = CreateMonster("orc", unique: false);

        var first = MonsterPlacementService.Place(state, definition, new MonsterPosition(1, 1));
        var second = MonsterPlacementService.Place(state, definition, new MonsterPosition(2, 2));

        Assert.True(first.Success);
        Assert.True(second.Success);
        Assert.Equal(2, state.Monsters.Count);
    }

    [Fact]
    public void RuntimeIds_AreDeterministicForIdenticalSuccessfulOperations()
    {
        var firstState = new MonsterRuntimeState();
        var secondState = new MonsterRuntimeState();
        var definition = CreateMonster("orc", unique: false);

        var first = MonsterPlacementService.Place(firstState, definition, new MonsterPosition(1, 1));
        var second = MonsterPlacementService.Place(secondState, definition, new MonsterPosition(1, 1));

        Assert.Equal(first.Monster, second.Monster);
    }

    [Fact]
    public void AllocationCanSelectAlreadyPlacedUniqueBeforePlacementRejectsIt()
    {
        var state = new MonsterRuntimeState();
        var definition = CreateMonster("unique_orc", unique: true);
        var initialPlacement = MonsterPlacementService.Place(state, definition, new MonsterPosition(1, 1));
        var baseEntries = MonsterAllocationTableBuilder.Build([definition]);
        var preparedEntries = MonsterAllocationTableBuilder.Prepare(baseEntries, [definition]);
        var effectiveEntries = MonsterAllocationEligibility.Apply(preparedEntries, effectiveLevel: 10);
        var selected = MonsterAllocationSelector.Select(effectiveEntries, new ScriptedRandomSource(0));

        var result = selected is null
            ? throw new InvalidOperationException("Allocation unexpectedly returned no candidate.")
            : MonsterPlacementService.Place(state, definition, new MonsterPosition(2, 2));

        Assert.True(initialPlacement.Success);
        Assert.NotNull(selected);
        Assert.Equal("unique_orc", selected.PreparedEntry.BaseEntry.MonsterDefinitionId);
        Assert.False(result.Success);
        Assert.Equal(MonsterPlacementFailureReason.UniqueUnavailable, result.FailureReason);
        Assert.Single(state.Monsters);
    }

    private static MonsterDefinition CreateMonster(string id, bool unique) =>
        new(
            id,
            new DiceRollDefinition("dice", 1, 1),
            new MonsterAiDefinition("wanderer", 0, false, false),
            [],
            [],
            new MonsterSensesDefinition(0, MonsterTelepathyProfile.Normal),
            new SpawnPolicy(unique, false, false, false, false, false, false, false, false),
            null,
            NativeLevel: 1,
            Rarity: 1);

    private sealed class ScriptedRandomSource(params int[] values) : IRandomSource
    {
        private readonly Queue<int> _values = new(values);

        public int Next(int minInclusive, int maxExclusive)
        {
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
