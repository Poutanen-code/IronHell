using IronHell.Core.Definitions;
using IronHell.Core.Monsters;
using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Monsters;

public sealed class MonsterPitPreparationTests
{
    [Theory]
    [InlineData(1, 1, MonsterPitFamily.Orc)]
    [InlineData(20, 20, MonsterPitFamily.Troll)]
    [InlineData(39, 39, MonsterPitFamily.Troll)]
    [InlineData(40, 40, MonsterPitFamily.Giant)]
    [InlineData(59, 59, MonsterPitFamily.Giant)]
    [InlineData(80, 80, MonsterPitFamily.Demon)]
    public void SelectFamily_BoundariesMatchVerifiedSource(int draw, int expectedDraw, MonsterPitFamily expected)
    {
        var random = new ScriptedRandomSource(draw);

        var selection = MonsterPitPreparer.SelectFamily(100, random);

        Assert.Equal(expected, selection.Family);
        Assert.Equal(expectedDraw, random.ValuesConsumed[0]);
    }

    [Theory]
    [InlineData(0, MonsterDragonBreathMasks.Acid)]
    [InlineData(1, MonsterDragonBreathMasks.Electric)]
    [InlineData(2, MonsterDragonBreathMasks.Fire)]
    [InlineData(3, MonsterDragonBreathMasks.Cold)]
    [InlineData(4, MonsterDragonBreathMasks.Poison)]
    [InlineData(5, MonsterDragonBreathMasks.MultiHued)]
    public void SelectFamily_DragonMaskBoundariesMatchVerifiedSource(int maskRoll, MonsterDragonBreathMasks expected)
    {
        var selection = MonsterPitPreparer.SelectFamily(100, new ScriptedRandomSource(60, maskRoll));

        Assert.Equal(MonsterPitFamily.Dragon, selection.Family);
        Assert.Equal(expected, selection.DragonMask);
    }

    [Theory]
    [InlineData(MonsterPitFamily.Orc, "o")]
    [InlineData(MonsterPitFamily.Troll, "T")]
    [InlineData(MonsterPitFamily.Giant, "P")]
    [InlineData(MonsterPitFamily.Demon, "U")]
    public void PitPredicate_SymbolFamiliesRequireExactSymbol(MonsterPitFamily family, string symbol)
    {
        var eligible = CreateMonster("eligible", symbol, 5);
        var wrong = CreateMonster("wrong", symbol == "o" ? "O" : "x", 5);
        var selection = new MonsterPitSelection(family, null);

        Assert.True(MonsterPitEligibility.IsEligible(eligible, selection));
        Assert.False(MonsterPitEligibility.IsEligible(wrong, selection));
    }

    [Fact]
    public void PitPredicate_RejectsUnique()
    {
        var candidate = CreateMonster("unique", "o", 5, unique: true);

        Assert.False(MonsterPitEligibility.IsEligible(candidate, new MonsterPitSelection(MonsterPitFamily.Orc, null)));
    }

    [Fact]
    public void DragonPredicate_RequiresExactSelectedBreathMask()
    {
        var acid = CreateMonster("acid", "d", 5, abilities: ["breath_acid"]);
        var mixed = CreateMonster("mixed", "d", 5, abilities: ["breath_acid", "breath_fire"]);
        var selection = new MonsterPitSelection(MonsterPitFamily.Dragon, MonsterDragonBreathMasks.Acid);

        Assert.True(MonsterPitEligibility.IsEligible(acid, selection));
        Assert.False(MonsterPitEligibility.IsEligible(mixed, selection));
    }

    [Fact]
    public void DragonPredicate_AcceptsLowerAndUppercaseDragonSymbols()
    {
        var lower = CreateMonster("lower", "d", 5, abilities: ["breath_fire"]);
        var upper = CreateMonster("upper", "D", 5, abilities: ["breath_fire"]);
        var selection = new MonsterPitSelection(MonsterPitFamily.Dragon, MonsterDragonBreathMasks.Fire);

        Assert.True(MonsterPitEligibility.IsEligible(lower, selection));
        Assert.True(MonsterPitEligibility.IsEligible(upper, selection));
    }

    [Fact]
    public void Prepare_SuccessProduces16CandidatesAndEightEvenIndexTiers()
    {
        var definitions = Enumerable.Range(1, 16)
            .Select(level => CreateMonster($"orc_{level}", "o", level))
            .ToArray();
        var sampledIndexes = Enumerable.Range(0, 16).Reverse().ToArray();
        var values = new List<int> { 1 };
        foreach (var index in sampledIndexes)
        {
            values.Add(1);
            values.Add(1);
            values.Add(index * 100);
            values.Add(60);
        }

        var result = MonsterPitPreparer.Prepare(
            definitions,
            MonsterAllocationTableBuilder.Build(definitions),
            depth: 100,
            new ScriptedRandomSource(values.ToArray()));

        Assert.True(result.IsSuccess);
        Assert.Equal(16, result.SampledCandidateDefinitionIds.Count);
        Assert.Equal(
            Enumerable.Range(1, 16).Select(level => $"orc_{level}"),
            result.SortedCandidateDefinitionIds);
        Assert.Equal(
            ["orc_1", "orc_3", "orc_5", "orc_7", "orc_9", "orc_11", "orc_13", "orc_15"],
            result.TierDefinitionIds);
    }

    [Fact]
    public void Prepare_SingleEligibleCandidateSamplesWithReplacement()
    {
        var definitions = new[] { CreateMonster("orc", "o", 5) };
        var values = new List<int> { 1 };
        values.AddRange(Enumerable.Repeat(new[] { 1, 1, 0, 60 }, 16).SelectMany(value => value));

        var result = MonsterPitPreparer.Prepare(
            definitions,
            MonsterAllocationTableBuilder.Build(definitions),
            depth: 10,
            new ScriptedRandomSource(values.ToArray()));

        Assert.True(result.IsSuccess);
        Assert.Equal(16, result.SampledCandidateDefinitionIds.Count);
        Assert.All(result.SampledCandidateDefinitionIds, id => Assert.Equal("orc", id));
    }

    [Fact]
    public void Prepare_NoEligibleCandidate_AbortsWithoutPartialResult()
    {
        var definitions = new[] { CreateMonster("troll", "T", 5) };

        var result = MonsterPitPreparer.Prepare(
            definitions,
            MonsterAllocationTableBuilder.Build(definitions),
            depth: 10,
            new ScriptedRandomSource(1, 1, 1));

        Assert.False(result.IsSuccess);
        Assert.Empty(result.SampledCandidateDefinitionIds);
        Assert.Empty(result.TierDefinitionIds);
    }

    [Fact]
    public void Prepare_DoesNotContaminateOrdinaryAllocation()
    {
        var definitions = new[]
        {
            CreateMonster("orc", "o", 5),
            CreateMonster("troll", "T", 5),
        };
        var entries = MonsterAllocationTableBuilder.Build(definitions);
        var values = new List<int> { 1 };
        values.AddRange(Enumerable.Repeat(new[] { 1, 1, 0, 60 }, 16).SelectMany(value => value));

        _ = MonsterPitPreparer.Prepare(definitions, entries, 10, new ScriptedRandomSource(values.ToArray()));
        var ordinary = MonsterAllocationTableBuilder.Prepare(entries, definitions);

        Assert.Equal([100, 100], ordinary.Select(entry => entry.PreparedWeight));
    }

    private static MonsterDefinition CreateMonster(
        string id,
        string symbol,
        int nativeLevel,
        bool unique = false,
        IReadOnlyList<string>? abilities = null) =>
        new(
            id,
            new DiceRollDefinition("dice", 1, 1),
            new MonsterAiDefinition("wanderer", 0, false, false),
            [],
            [],
            new MonsterSensesDefinition(0, MonsterTelepathyProfile.Normal),
            new SpawnPolicy(unique, false, false, false, false, false, false, false, false),
            null,
            nativeLevel,
            Rarity: 1,
            Symbol: symbol,
            Categories: [],
            Abilities: abilities ?? []);

    private sealed class ScriptedRandomSource(params int[] values) : IRandomSource
    {
        private readonly Queue<int> _values = new(values);

        public List<int> ValuesConsumed { get; } = [];

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

            ValuesConsumed.Add(value);
            return value;
        }

        public int RollDice(int count, int sides) => throw new NotSupportedException();
    }
}
