using IronHell.Core.Definitions;
using IronHell.Core.Monsters;
using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Monsters;

public sealed class MonsterNestPreparationTests
{
    [Theory]
    [InlineData(29, 29, MonsterNestFamily.Jelly)]
    [InlineData(30, 30, MonsterNestFamily.Animal)]
    [InlineData(49, 49, MonsterNestFamily.Animal)]
    [InlineData(50, 50, MonsterNestFamily.Undead)]
    public void SelectFamily_BoundariesMatchVerifiedSource(int depth, int draw, MonsterNestFamily expected)
    {
        var family = MonsterNestPreparer.SelectFamily(depth, new ScriptedRandomSource(draw));

        Assert.Equal(expected, family);
    }

    [Fact]
    public void JellyPredicate_UsesVerifiedSymbolsAndRejectsUnique()
    {
        var definitions = new[]
        {
            CreateMonster("jelly", "i"),
            CreateMonster("wrong", "o"),
            CreateMonster("unique_jelly", "j", unique: true),
        };
        var result = MonsterNestPreparer.Prepare(
            definitions,
            BuildEntries(definitions),
            depth: 29,
            SuccessfulSampleRandom(1));

        Assert.True(result.IsSuccess);
        Assert.All(result.CandidateDefinitionIds, id => Assert.Equal("jelly", id));
    }

    [Fact]
    public void AnimalPredicate_UsesExistingAnimalCategory()
    {
        var definitions = new[]
        {
            CreateMonster("animal", "a", categories: ["animal"]),
            CreateMonster("wrong", "a"),
        };
        var result = MonsterNestPreparer.Prepare(
            definitions,
            BuildEntries(definitions),
            depth: 30,
            SuccessfulSampleRandom(30));

        Assert.True(result.IsSuccess);
        Assert.All(result.CandidateDefinitionIds, id => Assert.Equal("animal", id));
    }

    [Fact]
    public void UndeadPredicate_UsesExistingUndeadCategory()
    {
        var definitions = new[]
        {
            CreateMonster("undead", "U", categories: ["undead"]),
            CreateMonster("wrong", "U"),
        };
        var result = MonsterNestPreparer.Prepare(
            definitions,
            BuildEntries(definitions),
            depth: 50,
            SuccessfulSampleRandom(50));

        Assert.True(result.IsSuccess);
        Assert.All(result.CandidateDefinitionIds, id => Assert.Equal("undead", id));
    }

    [Fact]
    public void Prepare_SuccessProducesExactly64CandidatesWithReplacement()
    {
        var definitions = new[] { CreateMonster("jelly", "i") };

        var result = MonsterNestPreparer.Prepare(
            definitions,
            BuildEntries(definitions),
            depth: 29,
            SuccessfulSampleRandom(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(MonsterNestPreparer.SampleCount, result.CandidateDefinitionIds.Count);
        Assert.All(result.CandidateDefinitionIds, id => Assert.Equal("jelly", id));
    }

    [Fact]
    public void Prepare_NoEligibleCandidate_AbortsWithoutPartialResult()
    {
        var definitions = new[] { CreateMonster("orc", "o") };

        var result = MonsterNestPreparer.Prepare(
            definitions,
            BuildEntries(definitions),
            depth: 29,
            new ScriptedRandomSource(1, 1, 1));

        Assert.False(result.IsSuccess);
        Assert.Empty(result.CandidateDefinitionIds);
    }

    [Fact]
    public void Prepare_DoesNotContaminateOrdinaryAllocation()
    {
        var definitions = new[]
        {
            CreateMonster("jelly", "i"),
            CreateMonster("orc", "o"),
        };
        var entries = BuildEntries(definitions);

        _ = MonsterNestPreparer.Prepare(
            definitions,
            entries,
            depth: 29,
            SuccessfulSampleRandom(1));
        var ordinary = MonsterAllocationTableBuilder.Prepare(entries, definitions);

        Assert.Equal([100, 100], ordinary.Select(entry => entry.PreparedWeight));
    }

    [Fact]
    public void Prepare_IdenticalInputAndRng_ReproducesFamilyAndCandidates()
    {
        var definitions = new[] { CreateMonster("jelly", "i") };
        var first = MonsterNestPreparer.Prepare(
            definitions,
            BuildEntries(definitions),
            depth: 29,
            new SeededRandomSource(42));
        var second = MonsterNestPreparer.Prepare(
            definitions,
            BuildEntries(definitions),
            depth: 29,
            new SeededRandomSource(42));

        Assert.Equal(first.IsSuccess, second.IsSuccess);
        Assert.Equal(first.Family, second.Family);
        Assert.Equal(first.CandidateDefinitionIds, second.CandidateDefinitionIds);
    }

    private static IReadOnlyList<MonsterAllocationEntry> BuildEntries(IReadOnlyList<MonsterDefinition> definitions) =>
        MonsterAllocationTableBuilder.Build(definitions);

    private static IRandomSource SuccessfulSampleRandom(int familyRoll) =>
        new ScriptedRandomSource(
            new[] { familyRoll }
                .Concat(Enumerable.Repeat(new[] { 1, 1, 1, 60 }, 64).SelectMany(values => values))
                .ToArray());

    private static MonsterDefinition CreateMonster(
        string id,
        string symbol,
        IReadOnlyList<string>? categories = null,
        bool unique = false) =>
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
            Rarity: 1,
            Symbol: symbol,
            Categories: categories ?? []);

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
