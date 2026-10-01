using IronHell.Core.Definitions;
using IronHell.Core.Monsters;
using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Monsters;

public sealed class MonsterAllocationEligibilityTests
{
    [Fact]
    public void ResolveEffectiveLevel_BothRollsFail_PreservesLevelAndConsumesTwoRolls()
    {
        var random = new ScriptedRandomSource(1, 1);

        var level = MonsterAllocationLevelResolver.ResolveEffectiveLevel(20, random);

        Assert.Equal(20, level);
        Assert.Equal([(0, 50), (0, 50)], random.Requests);
    }

    [Fact]
    public void ResolveEffectiveLevel_FirstSuccessOnly_AppliesOneBoost()
    {
        var random = new ScriptedRandomSource(0, 1);

        var level = MonsterAllocationLevelResolver.ResolveEffectiveLevel(20, random);

        Assert.Equal(25, level);
    }

    [Fact]
    public void ResolveEffectiveLevel_SecondSuccessIsIndependent()
    {
        var random = new ScriptedRandomSource(1, 0);

        var level = MonsterAllocationLevelResolver.ResolveEffectiveLevel(20, random);

        Assert.Equal(25, level);
    }

    [Fact]
    public void ResolveEffectiveLevel_BothSuccessesRecalculateFromModifiedLevel()
    {
        var random = new ScriptedRandomSource(0, 0);

        var level = MonsterAllocationLevelResolver.ResolveEffectiveLevel(4, random);

        Assert.Equal(10, level);
    }

    [Fact]
    public void ResolveEffectiveLevel_CapsEachBoostAtFive()
    {
        var random = new ScriptedRandomSource(0, 1);

        var level = MonsterAllocationLevelResolver.ResolveEffectiveLevel(100, random);

        Assert.Equal(105, level);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ResolveEffectiveLevel_NonPositiveLevelConsumesNoRolls(int requestedLevel)
    {
        var random = new ScriptedRandomSource();

        var level = MonsterAllocationLevelResolver.ResolveEffectiveLevel(requestedLevel, random);

        Assert.Equal(requestedLevel, level);
        Assert.Empty(random.Requests);
    }

    [Fact]
    public void ApplyEffectiveEligibility_ZeroesOverLevelAndPositiveLevelTownEntries()
    {
        var definitions = new[]
        {
            CreateMonster("town", nativeLevel: 0, rarity: 1),
            CreateMonster("eligible", nativeLevel: 5, rarity: 2),
            CreateMonster("too_deep", nativeLevel: 11, rarity: 1),
        };
        var baseEntries = MonsterAllocationTableBuilder.Build(definitions);
        var preparedEntries = MonsterAllocationTableBuilder.Prepare(baseEntries, definitions);

        var effectiveEntries = MonsterAllocationEligibility.Apply(preparedEntries, effectiveLevel: 10);

        Assert.Equal(["town", "eligible", "too_deep"], effectiveEntries.Select(entry => entry.PreparedEntry.BaseEntry.MonsterDefinitionId));
        Assert.Equal([0, 50, 0], effectiveEntries.Select(entry => entry.EffectiveWeight));
        Assert.Equal([100, 50, 100], effectiveEntries.Select(entry => entry.PreparedEntry.PreparedWeight));
    }

    [Fact]
    public void ApplyEffectiveEligibility_NonPositiveEffectiveLevelAllowsNativeZeroOnlyWithinLevel()
    {
        var definitions = new[]
        {
            CreateMonster("town", nativeLevel: 0, rarity: 1),
            CreateMonster("deeper", nativeLevel: 1, rarity: 1),
        };
        var baseEntries = MonsterAllocationTableBuilder.Build(definitions);
        var preparedEntries = MonsterAllocationTableBuilder.Prepare(baseEntries, definitions);

        var effectiveEntries = MonsterAllocationEligibility.Apply(preparedEntries, effectiveLevel: 0);

        Assert.Equal([100, 0], effectiveEntries.Select(entry => entry.EffectiveWeight));
    }

    [Fact]
    public void ApplyEffectiveEligibility_ForceDepthEntryOverEffectiveLevelIsExcluded()
    {
        var definitions = new[]
        {
            CreateMonster("force_depth", nativeLevel: 11, rarity: 1, forceDepth: true),
        };
        var baseEntries = MonsterAllocationTableBuilder.Build(definitions);
        var preparedEntries = MonsterAllocationTableBuilder.Prepare(baseEntries, definitions);

        var effectiveEntries = MonsterAllocationEligibility.Apply(preparedEntries, effectiveLevel: 10);

        Assert.Equal(0, Assert.Single(effectiveEntries).EffectiveWeight);
    }

    [Fact]
    public void PrepareForSelection_AppliesOodBeforeEffectiveEligibility()
    {
        var definitions = new[] { CreateMonster("deep", nativeLevel: 25, rarity: 1) };
        var baseEntries = MonsterAllocationTableBuilder.Build(definitions);
        var preparedEntries = MonsterAllocationTableBuilder.Prepare(baseEntries, definitions);

        var effectiveEntries = MonsterAllocationEligibility.PrepareForSelection(
            preparedEntries,
            requestedLevel: 20,
            new ScriptedRandomSource(0, 1));

        Assert.Equal(25, Assert.Single(effectiveEntries).PreparedEntry.BaseEntry.NativeLevel);
        Assert.Equal(100, effectiveEntries[0].EffectiveWeight);
    }

    [Fact]
    public void SelectWithComparison_UsesEffectiveWeights()
    {
        var effectiveEntries = new[]
        {
            new MonsterAllocationEffectiveEntry(Prepared("excluded", 100, nativeLevel: 1), 0),
            new MonsterAllocationEffectiveEntry(Prepared("selected", 1, nativeLevel: 2), 1),
        };

        var selected = MonsterAllocationSelector.SelectWithComparison(effectiveEntries, new ScriptedRandomSource(0, 60));

        Assert.Equal("selected", selected?.PreparedEntry.BaseEntry.MonsterDefinitionId);
    }

    private static MonsterAllocationPreparedEntry Prepared(string id, int weight, int nativeLevel) =>
        new(new MonsterAllocationEntry(id, nativeLevel, weight), weight);

    private static MonsterDefinition CreateMonster(string id, int nativeLevel, int rarity, bool forceDepth = false) =>
        new(
            id,
            new DiceRollDefinition("dice", 1, 1),
            new MonsterAiDefinition("wanderer", 0, false, false),
            [],
            [],
            new MonsterSensesDefinition(0, MonsterTelepathyProfile.Normal),
            new SpawnPolicy(false, false, forceDepth, false, false, false, false, false, false),
            null,
            nativeLevel,
            rarity);

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
