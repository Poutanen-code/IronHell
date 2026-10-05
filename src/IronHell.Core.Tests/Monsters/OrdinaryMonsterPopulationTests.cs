using IronHell.Core.Definitions;
using IronHell.Core.Monsters;
using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Monsters;

public sealed class OrdinaryMonsterPopulationTests
{
    [Fact]
    public void RequestCount_RandomMinimum_UsesOneThroughEight()
    {
        var count = OrdinaryMonsterRequestCount.Calculate(1, new ScriptedRandomSource(1));

        Assert.Equal(17, count);
    }

    [Fact]
    public void RequestCount_RandomMaximum_UsesEight()
    {
        var count = OrdinaryMonsterRequestCount.Calculate(1, new ScriptedRandomSource(8));

        Assert.Equal(24, count);
    }

    [Fact]
    public void RequestCount_DepthClampMinimum_IsTwo()
    {
        var count = OrdinaryMonsterRequestCount.Calculate(2, new ScriptedRandomSource(8));

        Assert.Equal(24, count);
    }

    [Fact]
    public void RequestCount_DepthTermUsesIntegerDivision()
    {
        var count = OrdinaryMonsterRequestCount.Calculate(10, new ScriptedRandomSource(1));

        Assert.Equal(18, count);
    }

    [Fact]
    public void RequestCount_DepthClampMaximum_IsTen()
    {
        var count = OrdinaryMonsterRequestCount.Calculate(100, new ScriptedRandomSource(1));

        Assert.Equal(25, count);
    }

    [Fact]
    public void RequestCount_ConsumesExactlyOneCountRngDraw()
    {
        var random = new ScriptedRandomSource(1);

        _ = OrdinaryMonsterRequestCount.Calculate(1, random);

        Assert.Equal([(1, 9)], random.Requests);
    }

    [Fact]
    public void Populate_SomeRequestsFail_RequestedCountExceedsSuccessfulPlacements()
    {
        var definition = CreateMonster("orc");
        var space = new MonsterPlacementSpace(1, 1, [new MonsterPosition(0, 0)]);
        var state = new MonsterRuntimeState();
        var random = new ScriptedRandomSource([1, .. Enumerable.Repeat(0, 34)]);
        var prepared = PreparedEntries(definition);

        var result = OrdinaryMonsterPopulation.Populate(
            depth: 1,
            prepared,
            [definition],
            space,
            state,
            random,
            new OrdinaryMonsterPopulationOptions(1));

        Assert.Equal(17, result.RequestedCount);
        Assert.Equal(0, result.SuccessfulPlacements);
        Assert.Empty(state.Monsters);
    }

    [Fact]
    public void Populate_AllRequestsFail_ReturnsFormulaCountAndLeavesStateEmpty()
    {
        var definition = CreateMonster("orc");
        var space = new MonsterPlacementSpace(1, 1, [new MonsterPosition(0, 0)]);
        var state = new MonsterRuntimeState();
        var random = new ScriptedRandomSource([8, .. Enumerable.Repeat(0, 48)]);

        var result = OrdinaryMonsterPopulation.Populate(
            depth: 1,
            PreparedEntries(definition),
            [definition],
            space,
            state,
            random,
            new OrdinaryMonsterPopulationOptions(1));

        Assert.Equal(24, result.RequestedCount);
        Assert.Equal(0, result.SuccessfulPlacements);
        Assert.Empty(state.Monsters);
    }

    [Fact]
    public void Populate_SuccessfulRequestsMatchRuntimeMonsterCount()
    {
        var definition = CreateMonster("orc");
        var positions = Enumerable.Range(0, 17)
            .Select(index => new MonsterPosition(index % 5, index / 5))
            .ToArray();
        var space = new MonsterPlacementSpace(5, 5);
        var state = new MonsterRuntimeState();
        var random = new ScriptedRandomSource(PopulationValues(1, positions));

        var result = OrdinaryMonsterPopulation.Populate(
            depth: 1,
            PreparedEntries(definition),
            [definition],
            space,
            state,
            random,
            new OrdinaryMonsterPopulationOptions(1));

        Assert.Equal(17, result.RequestedCount);
        Assert.Equal(17, result.SuccessfulPlacements);
        Assert.Equal(result.SuccessfulPlacements, state.Monsters.Count);
    }

    [Fact]
    public void Populate_UniqueRejectionDoesNotReselect()
    {
        var definition = CreateMonster("unique_orc", unique: true);
        var space = new MonsterPlacementSpace(2, 1);
        var state = new MonsterRuntimeState();
        MonsterPlacementService.Place(state, definition, new MonsterPosition(1, 0), new SeededRandomSource(1));
        var random = new ScriptedRandomSource(PopulationValues(
            1,
            Enumerable.Repeat(new MonsterPosition(0, 0), 17).ToArray(),
            includeSpawnStateDraws: false));

        var result = OrdinaryMonsterPopulation.Populate(
            depth: 1,
            PreparedEntries(definition),
            [definition],
            space,
            state,
            random,
            new OrdinaryMonsterPopulationOptions(1));

        Assert.Equal(17, result.RequestedCount);
        Assert.Equal(0, result.SuccessfulPlacements);
        Assert.Single(state.Monsters);
    }

    [Fact]
    public void Populate_IdenticalSeedAndInputs_ReproducesObservableState()
    {
        var definition = CreateMonster("orc");
        var firstState = new MonsterRuntimeState();
        var secondState = new MonsterRuntimeState();
        var first = OrdinaryMonsterPopulation.Populate(
            1,
            PreparedEntries(definition),
            [definition],
            new MonsterPlacementSpace(10, 10),
            firstState,
            new SeededRandomSource(42),
            new OrdinaryMonsterPopulationOptions(3));
        var second = OrdinaryMonsterPopulation.Populate(
            1,
            PreparedEntries(definition),
            [definition],
            new MonsterPlacementSpace(10, 10),
            secondState,
            new SeededRandomSource(42),
            new OrdinaryMonsterPopulationOptions(3));

        Assert.Equal(first, second);
        Assert.Equal(firstState.Monsters, secondState.Monsters);
    }

    [Fact]
    public void Populate_ExplicitGroupExpansionAddsMembersWithoutIncreasingRequestCount()
    {
        var definition = CreateMonster("friend", unique: false, friends: true);

        var state = new MonsterRuntimeState();
        var result = OrdinaryMonsterPopulation.Populate(
            depth: 1,
            PreparedEntries(definition),
            [definition],
            new MonsterPlacementSpace(20, 20),
            state,
            new GroupPopulationRandomSource(),
            new OrdinaryMonsterPopulationOptions(1, AllowGroupExpansion: true));

        Assert.Equal(17, result.RequestedCount);
        Assert.Equal(17, result.SuccessfulPlacements);
        Assert.Equal(18, result.TotalRuntimeMonstersAdded);
        Assert.Equal(18, state.Monsters.Count);
    }

    private static IReadOnlyList<MonsterAllocationPreparedEntry> PreparedEntries(MonsterDefinition definition) =>
        MonsterAllocationTableBuilder.Prepare(
            MonsterAllocationTableBuilder.Build([definition]),
            [definition]);

    private static int[] PopulationValues(
        int countRoll,
        IReadOnlyList<MonsterPosition> positions,
        bool includeSpawnStateDraws = true) =>
        new[] { countRoll }
            .Concat(positions.SelectMany(position => includeSpawnStateDraws
                ? new[] { position.X, position.Y, 1, 1, 0, 60, 0 }
                : new[] { position.X, position.Y, 1, 1, 0, 60 }))
            .ToArray();

    private static MonsterDefinition CreateMonster(string id, bool unique = false, bool friends = false) =>
        new(
            id,
            new DiceRollDefinition("dice", 1, 1),
            new MonsterAiDefinition("wanderer", 0, false, false),
            [],
            [],
            new MonsterSensesDefinition(0, MonsterTelepathyProfile.Normal),
            new SpawnPolicy(unique, false, false, false, false, false, false, friends, false),
            null,
            NativeLevel: 1,
            Rarity: 1,
            MovementSpeed: 100);

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

    private sealed class GroupPopulationRandomSource : IRandomSource
    {
        private int _coordinateDraw;
        private int _selectionDraw;
        private int _groupDraw;

        public int Next(int minInclusive, int maxExclusive)
        {
            var value = maxExclusive switch
            {
                9 => 1,
                50 => 1,
                2 => minInclusive,
                37_500 => minInclusive,
                14 => _groupDraw++ == 0 ? 2 : 1,
                100 => _selectionDraw++ % 2 == 0 ? 0 : 60,
                20 => NextCoordinate(),
                _ => throw new InvalidOperationException($"Unexpected RNG range [{minInclusive}, {maxExclusive})."),
            };

            if (value < minInclusive || value >= maxExclusive)
            {
                throw new InvalidOperationException($"Generated value {value} is outside [{minInclusive}, {maxExclusive}).");
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

        private int NextCoordinate()
        {
            var pair = _coordinateDraw / 2;
            var value = _coordinateDraw % 2 == 0 ? pair + 1 : 0;
            _coordinateDraw++;
            return value;
        }
    }
}
