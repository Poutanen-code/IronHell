using IronHell.Core.Definitions;
using IronHell.Core.Monsters;
using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Monsters;

public sealed class MonsterPlacementEligibilityTests
{
    [Fact]
    public void Evaluate_OrdinaryMonster_IsEligible()
    {
        var definition = CreateMonster("orc", unique: false);

        var result = MonsterPlacementEligibility.Evaluate(definition, new MonsterPlacementEligibilityContext());

        Assert.True(result.IsEligible);
        Assert.Equal(MonsterPlacementRejectionReason.None, result.RejectionReason);
    }

    [Fact]
    public void Evaluate_UniqueWithRemainingCapacity_IsEligible()
    {
        var definition = CreateMonster("unique_orc", unique: true);

        var result = MonsterPlacementEligibility.Evaluate(
            definition,
            new MonsterPlacementEligibilityContext(UniqueHasRemainingCapacity: true));

        Assert.True(result.IsEligible);
        Assert.Equal(MonsterPlacementRejectionReason.None, result.RejectionReason);
    }

    [Fact]
    public void Evaluate_ExhaustedUnique_IsRejectedAfterAllocation()
    {
        var definition = CreateMonster("unique_orc", unique: true);

        var result = MonsterPlacementEligibility.Evaluate(
            definition,
            new MonsterPlacementEligibilityContext(UniqueHasRemainingCapacity: false));

        Assert.False(result.IsEligible);
        Assert.Equal(MonsterPlacementRejectionReason.UniqueUnavailable, result.RejectionReason);
    }

    [Fact]
    public void AllocationCanSelectUniqueBeforePlacementRejectsIt()
    {
        var definition = CreateMonster("unique_orc", unique: true);
        var baseEntries = MonsterAllocationTableBuilder.Build([definition]);
        var preparedEntries = MonsterAllocationTableBuilder.Prepare(baseEntries, [definition]);
        var effectiveEntries = MonsterAllocationEligibility.Apply(preparedEntries, effectiveLevel: 10);
        var selected = MonsterAllocationSelector.Select(effectiveEntries, new ScriptedRandomSource(0));

        Assert.NotNull(selected);
        Assert.Equal("unique_orc", selected.PreparedEntry.BaseEntry.MonsterDefinitionId);
        Assert.Equal(100, selected.EffectiveWeight);

        var result = MonsterPlacementEligibility.Evaluate(
            definition,
            new MonsterPlacementEligibilityContext(UniqueHasRemainingCapacity: false));

        Assert.False(result.IsEligible);
        Assert.Equal(100, selected.EffectiveWeight);
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
