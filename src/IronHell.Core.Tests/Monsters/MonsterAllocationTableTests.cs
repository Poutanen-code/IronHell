using IronHell.Core.Definitions;
using IronHell.Core.Monsters;
using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Monsters;

public sealed class MonsterAllocationTableTests
{
    [Fact]
    public void Build_CreatesOneEntryPerEligibleDefinitionWithIdentityLevelAndWeight()
    {
        var definitions = new[]
        {
            CreateMonster("orc", nativeLevel: 10, rarity: 2),
            CreateMonster("rat", nativeLevel: 1, rarity: 1),
        };

        var entries = MonsterAllocationTableBuilder.Build(definitions);

        Assert.Collection(
            entries,
            entry =>
            {
                Assert.Equal("rat", entry.MonsterDefinitionId);
                Assert.Equal(1, entry.NativeLevel);
                Assert.Equal(100, entry.BaseWeight);
            },
            entry =>
            {
                Assert.Equal("orc", entry.MonsterDefinitionId);
                Assert.Equal(10, entry.NativeLevel);
                Assert.Equal(50, entry.BaseWeight);
            });
    }

    [Fact]
    public void Build_UsesIntegerDivisionForBaseWeight()
    {
        var entries = MonsterAllocationTableBuilder.Build([
            CreateMonster("third", nativeLevel: 3, rarity: 3),
        ]);

        Assert.Equal(33, Assert.Single(entries).BaseWeight);
    }

    [Fact]
    public void Build_ExcludesRarityZeroDefinitions()
    {
        var entries = MonsterAllocationTableBuilder.Build([
            CreateMonster("eligible", nativeLevel: 1, rarity: 1),
            CreateMonster("excluded", nativeLevel: 2, rarity: 0),
        ]);

        var entry = Assert.Single(entries);
        Assert.Equal("eligible", entry.MonsterDefinitionId);
    }

    [Fact]
    public void Build_OrdersByNativeLevelAndPreservesInputOrderForEqualLevels()
    {
        var entries = MonsterAllocationTableBuilder.Build([
            CreateMonster("late", nativeLevel: 20, rarity: 1),
            CreateMonster("same_first", nativeLevel: 5, rarity: 1),
            CreateMonster("early", nativeLevel: 1, rarity: 1),
            CreateMonster("same_second", nativeLevel: 5, rarity: 1),
        ]);

        Assert.Equal(
            ["early", "same_first", "same_second", "late"],
            entries.Select(entry => entry.MonsterDefinitionId));
    }

    [Fact]
    public void Prepare_WithoutHook_PreservesEveryBaseWeight()
    {
        var definitions = new[]
        {
            CreateMonster("rat", nativeLevel: 1, rarity: 1),
            CreateMonster("orc", nativeLevel: 10, rarity: 2),
        };
        var entries = MonsterAllocationTableBuilder.Build(definitions);

        var prepared = MonsterAllocationTableBuilder.Prepare(entries, definitions);

        Assert.Equal(entries.Select(entry => entry.BaseWeight), prepared.Select(entry => entry.PreparedWeight));
    }

    [Fact]
    public void Prepare_AcceptedEntry_PreservesBaseWeight()
    {
        var definitions = new[] { CreateMonster("orc", nativeLevel: 10, rarity: 2) };
        var entries = MonsterAllocationTableBuilder.Build(definitions);

        var prepared = MonsterAllocationTableBuilder.Prepare(entries, definitions, definition => definition.Id == "orc");

        Assert.Equal(50, Assert.Single(prepared).PreparedWeight);
    }

    [Fact]
    public void Prepare_RejectedEntry_RetainsEntryWithZeroPreparedWeight()
    {
        var definitions = new[] { CreateMonster("orc", nativeLevel: 10, rarity: 2) };
        var entries = MonsterAllocationTableBuilder.Build(definitions);

        var prepared = MonsterAllocationTableBuilder.Prepare(entries, definitions, _ => false);

        var preparedEntry = Assert.Single(prepared);
        Assert.Equal(entries[0], preparedEntry.BaseEntry);
        Assert.Equal(0, preparedEntry.PreparedWeight);
    }

    [Fact]
    public void Prepare_MixedFiltering_PreservesOrderAndBaseEntries()
    {
        var definitions = new[]
        {
            CreateMonster("late", nativeLevel: 20, rarity: 1),
            CreateMonster("early", nativeLevel: 1, rarity: 2),
            CreateMonster("middle", nativeLevel: 10, rarity: 5),
        };
        var entries = MonsterAllocationTableBuilder.Build(definitions);
        var originalEntries = entries.ToArray();

        var prepared = MonsterAllocationTableBuilder.Prepare(
            entries,
            definitions,
            definition => definition.Id is "early" or "middle");

        Assert.Equal(["early", "middle", "late"], prepared.Select(entry => entry.BaseEntry.MonsterDefinitionId));
        Assert.Equal([50, 20, 0], prepared.Select(entry => entry.PreparedWeight));
        Assert.Equal(originalEntries, entries);
    }

    [Fact]
    public void Prepare_RepeatedCalls_IsolateHookResultsAndLeaveBaseTableUnchanged()
    {
        var definitions = new[]
        {
            CreateMonster("rat", nativeLevel: 1, rarity: 1),
            CreateMonster("orc", nativeLevel: 10, rarity: 2),
        };
        var entries = MonsterAllocationTableBuilder.Build(definitions);

        var ratsOnly = MonsterAllocationTableBuilder.Prepare(entries, definitions, definition => definition.Id == "rat");
        var orcsOnly = MonsterAllocationTableBuilder.Prepare(entries, definitions, definition => definition.Id == "orc");

        Assert.Equal([100, 0], ratsOnly.Select(entry => entry.PreparedWeight));
        Assert.Equal([0, 50], orcsOnly.Select(entry => entry.PreparedWeight));
        Assert.Equal([100, 50], entries.Select(entry => entry.BaseWeight));
    }

    [Fact]
    public void Select_SinglePositiveEntry_ReturnsThatEntry()
    {
        MonsterAllocationPreparedEntry[] entries = [Prepared("rat", 20)];

        var selected = MonsterAllocationSelector.Select(entries, new ScriptedRandomSource(19));

        Assert.Equal("rat", selected?.BaseEntry.MonsterDefinitionId);
    }

    [Theory]
    [InlineData(0, "a")]
    [InlineData(19, "a")]
    [InlineData(20, "b")]
    [InlineData(24, "b")]
    public void Select_UsesExclusiveCumulativeBoundaries(int draw, string expectedId)
    {
        MonsterAllocationPreparedEntry[] entries = [Prepared("a", 20), Prepared("b", 5)];

        var selected = MonsterAllocationSelector.Select(entries, new ScriptedRandomSource(draw));

        Assert.Equal(expectedId, selected?.BaseEntry.MonsterDefinitionId);
    }

    [Theory]
    [InlineData(0, "a")]
    [InlineData(19, "a")]
    [InlineData(20, "c")]
    [InlineData(24, "c")]
    public void Select_ZeroWeightEntriesNeverWin(int draw, string expectedId)
    {
        MonsterAllocationPreparedEntry[] entries = [Prepared("a", 20), Prepared("b", 0), Prepared("c", 5)];

        var selected = MonsterAllocationSelector.Select(entries, new ScriptedRandomSource(draw));

        Assert.Equal(expectedId, selected?.BaseEntry.MonsterDefinitionId);
    }

    [Fact]
    public void Select_ZeroTotal_ReturnsNoSelectionWithoutConsumingRandomness()
    {
        var random = new ScriptedRandomSource();

        var selected = MonsterAllocationSelector.Select(
            [Prepared("a", 0), Prepared("b", 0)],
            random);

        Assert.Null(selected);
        Assert.Empty(random.Requests);
    }

    [Fact]
    public void Select_UsesPreparedIntegerWeightsAndDoesNotMutateState()
    {
        MonsterAllocationPreparedEntry[] entries = [Prepared("a", 2), Prepared("b", 1)];
        var originalEntries = entries.ToArray();

        var selected = MonsterAllocationSelector.Select(entries, new ScriptedRandomSource(2));

        Assert.Equal("b", selected?.BaseEntry.MonsterDefinitionId);
        Assert.Equal(originalEntries, entries);
        Assert.Equal([2, 1], entries.Select(entry => entry.BaseEntry.BaseWeight));
        Assert.Equal([2, 1], entries.Select(entry => entry.PreparedWeight));
    }

    [Fact]
    public void SelectWithComparison_PAtLeast60_PerformsNoAdditionalPick()
    {
        MonsterAllocationPreparedEntry[] entries = [Prepared("a", 20, nativeLevel: 20)];
        var random = new ScriptedRandomSource(0, 60);

        var selected = MonsterAllocationSelector.SelectWithComparison(entries, random);

        Assert.Equal("a", selected?.BaseEntry.MonsterDefinitionId);
        Assert.Equal([(0, 20), (0, 100)], random.Requests);
    }

    [Fact]
    public void SelectWithComparison_P59_PerformsExactlyOneAdditionalPick()
    {
        MonsterAllocationPreparedEntry[] entries =
        [
            Prepared("a", 10, nativeLevel: 20),
            Prepared("b", 10, nativeLevel: 10),
        ];
        var random = new ScriptedRandomSource(0, 59, 10);

        var selected = MonsterAllocationSelector.SelectWithComparison(entries, random);

        Assert.Equal("b", selected?.BaseEntry.MonsterDefinitionId);
        Assert.Equal([(0, 20), (0, 100), (0, 20)], random.Requests);
    }

    [Fact]
    public void SelectWithComparison_P10_PerformsExactlyOneAdditionalPick()
    {
        MonsterAllocationPreparedEntry[] entries =
        [
            Prepared("a", 10, nativeLevel: 20),
            Prepared("b", 10, nativeLevel: 10),
        ];
        var random = new ScriptedRandomSource(0, 10, 10);

        var selected = MonsterAllocationSelector.SelectWithComparison(entries, random);

        Assert.Equal("b", selected?.BaseEntry.MonsterDefinitionId);
        Assert.Equal([(0, 20), (0, 100), (0, 20)], random.Requests);
    }

    [Fact]
    public void SelectWithComparison_P9_PerformsTwoAdditionalPicks()
    {
        MonsterAllocationPreparedEntry[] entries =
        [
            Prepared("a", 10, nativeLevel: 20),
            Prepared("b", 10, nativeLevel: 10),
        ];
        var random = new ScriptedRandomSource(0, 9, 10, 0);

        var selected = MonsterAllocationSelector.SelectWithComparison(entries, random);

        Assert.Equal("b", selected?.BaseEntry.MonsterDefinitionId);
        Assert.Equal([(0, 20), (0, 100), (0, 20), (0, 20)], random.Requests);
    }

    [Fact]
    public void SelectWithComparison_LowerAbsoluteNativeLevel_ReplacesCurrent()
    {
        MonsterAllocationPreparedEntry[] entries =
        [
            Prepared("current", 10, nativeLevel: 20),
            Prepared("closer", 10, nativeLevel: 10),
        ];
        var random = new ScriptedRandomSource(0, 59, 10);

        var selected = MonsterAllocationSelector.SelectWithComparison(entries, random);

        Assert.Equal("closer", selected?.BaseEntry.MonsterDefinitionId);
    }

    [Fact]
    public void SelectWithComparison_HigherAbsoluteNativeLevel_DoesNotReplaceCurrent()
    {
        MonsterAllocationPreparedEntry[] entries =
        [
            Prepared("current", 10, nativeLevel: 10),
            Prepared("farther", 10, nativeLevel: 20),
        ];
        var random = new ScriptedRandomSource(0, 59, 10);

        var selected = MonsterAllocationSelector.SelectWithComparison(entries, random);

        Assert.Equal("current", selected?.BaseEntry.MonsterDefinitionId);
    }

    [Fact]
    public void SelectWithComparison_EqualAbsoluteNativeLevel_RetainsCurrent()
    {
        MonsterAllocationPreparedEntry[] entries =
        [
            Prepared("current", 10, nativeLevel: 10),
            Prepared("equal", 10, nativeLevel: 10),
        ];
        var random = new ScriptedRandomSource(0, 59, 10);

        var selected = MonsterAllocationSelector.SelectWithComparison(entries, random);

        Assert.Equal("current", selected?.BaseEntry.MonsterDefinitionId);
    }

    [Fact]
    public void SelectWithComparison_SamplesWithReplacement()
    {
        MonsterAllocationPreparedEntry[] entries = [Prepared("same", 20, nativeLevel: 10)];
        var random = new ScriptedRandomSource(0, 9, 0, 0);

        var selected = MonsterAllocationSelector.SelectWithComparison(entries, random);

        Assert.Equal("same", selected?.BaseEntry.MonsterDefinitionId);
        Assert.Equal([(0, 20), (0, 100), (0, 20), (0, 20)], random.Requests);
    }

    [Fact]
    public void SelectWithComparison_ZeroTotal_ReturnsNoSelectionWithoutConsumingRandomness()
    {
        var random = new ScriptedRandomSource();

        var selected = MonsterAllocationSelector.SelectWithComparison(
            [Prepared("a", 0), Prepared("b", 0)],
            random);

        Assert.Null(selected);
        Assert.Empty(random.Requests);
    }

    private static MonsterAllocationPreparedEntry Prepared(string id, int weight, int nativeLevel = 0) =>
        new(new MonsterAllocationEntry(id, nativeLevel, weight), weight);

    private static MonsterDefinition CreateMonster(string id, int nativeLevel, int rarity) =>
        new(
            id,
            new DiceRollDefinition("dice", 1, 1),
            new MonsterAiDefinition("wanderer", 0, false, false),
            [],
            [],
            new MonsterSensesDefinition(0, MonsterTelepathyProfile.Normal),
            new SpawnPolicy(false, false, false, false, false, false, false, false, false),
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
