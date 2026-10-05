using IronHell.Core.Definitions;
using IronHell.Core.Monsters;
using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Monsters;

public sealed class MonsterEscortTests
{
    [Fact]
    public void EscortCandidate_SameSymbolAndAllowedLevel_IsEligible()
    {
        var leader = CreateMonster("leader", symbol: "o", nativeLevel: 10);
        var candidate = CreateMonster("escort", symbol: "o", nativeLevel: 10);
        var entry = new MonsterAllocationEntry(candidate.Id, candidate.NativeLevel, 100);

        Assert.True(MonsterEscortEligibility.IsEligible(entry, leader, candidate));
    }

    [Theory]
    [InlineData("x", 10, false)]
    [InlineData("o", 11, false)]
    public void EscortCandidate_InvalidSymbolOrLevel_IsRejected(string symbol, int nativeLevel, bool expected)
    {
        var leader = CreateMonster("leader", symbol: "o", nativeLevel: 10);
        var candidate = CreateMonster("escort", symbol, nativeLevel);
        var entry = new MonsterAllocationEntry(candidate.Id, candidate.NativeLevel, 100);

        Assert.Equal(expected, MonsterEscortEligibility.IsEligible(entry, leader, candidate));
    }

    [Fact]
    public void EscortCandidate_UniqueOrLeaderRace_IsRejected()
    {
        var leader = CreateMonster("leader", symbol: "o", nativeLevel: 10);
        var unique = CreateMonster("unique", symbol: "o", nativeLevel: 5, options: new MonsterSpawnOptions(Unique: true));
        var leaderEntry = new MonsterAllocationEntry(leader.Id, leader.NativeLevel, 100);
        var uniqueEntry = new MonsterAllocationEntry(unique.Id, unique.NativeLevel, 100);

        Assert.False(MonsterEscortEligibility.IsEligible(leaderEntry, leader, leader));
        Assert.False(MonsterEscortEligibility.IsEligible(uniqueEntry, leader, unique));
    }

    [Fact]
    public void Escort_GroupExpansionDisabled_PlacesNoEscorts()
    {
        var state = new MonsterRuntimeState();
        var leaderDefinition = CreateMonster("leader", symbol: "o", nativeLevel: 10, options: new MonsterSpawnOptions(Escort: true));
        var escortDefinition = CreateMonster("escort", symbol: "o", nativeLevel: 5);
        var leaderPlacement = MonsterPlacementService.Place(state, leaderDefinition, new MonsterPosition(3, 3), new SeededRandomSource(1));
        var entries = MonsterAllocationTableBuilder.Build([leaderDefinition, escortDefinition]);

        var result = MonsterEscortExpander.Expand(
            state,
            leaderPlacement.Monster!,
            leaderDefinition,
            new MonsterEscortExpansionOptions(
                entries,
                [leaderDefinition, escortDefinition],
                new MonsterPlacementSpace(7, 7),
                10,
                AllowGroupExpansion: false,
                new ScriptedRandomSource()));

        Assert.Equal(0, result.Attempts);
        Assert.Equal(0, result.SuccessfulEscorts);
        Assert.Single(state.Monsters);
    }

    [Fact]
    public void Escort_EligibleCandidate_IsSelectedWithNormalAllocationAndPlaced()
    {
        var state = new MonsterRuntimeState();
        var leaderDefinition = CreateMonster("leader", symbol: "o", nativeLevel: 10, options: new MonsterSpawnOptions(Escort: true));
        var escortDefinition = CreateMonster("escort", symbol: "o", nativeLevel: 5, rarity: 2);
        var leaderPlacement = MonsterPlacementService.Place(state, leaderDefinition, new MonsterPosition(3, 3), new SeededRandomSource(1));
        var entries = MonsterAllocationTableBuilder.Build([leaderDefinition, escortDefinition]);
        var result = MonsterEscortExpander.Expand(
            state,
            leaderPlacement.Monster!,
            leaderDefinition,
            new MonsterEscortExpansionOptions(
                entries,
                [leaderDefinition, escortDefinition],
                new MonsterPlacementSpace(7, 7),
                10,
                AllowGroupExpansion: true,
                new RepeatingEscortRandomSource()));

        Assert.Equal(1, result.SuccessfulEscorts);
        Assert.Equal(1, result.TotalRuntimeMonstersAdded);
        Assert.Contains(state.Monsters, monster => monster.DefinitionId == "escort");
    }

    [Fact]
    public void EscortPreparation_DoesNotContaminateOrdinaryAllocation()
    {
        var leader = CreateMonster("leader", symbol: "o", nativeLevel: 10, options: new MonsterSpawnOptions(Escort: true));
        var escort = CreateMonster("escort", symbol: "o", nativeLevel: 5);
        var other = CreateMonster("other", symbol: "x", nativeLevel: 5);
        var entries = MonsterAllocationTableBuilder.Build([leader, escort, other]);

        _ = MonsterAllocationTableBuilder.Prepare(
            entries,
            [leader, escort, other],
            candidate => candidate.Symbol == leader.Symbol && candidate.Id != leader.Id);
        var ordinary = MonsterAllocationTableBuilder.Prepare(entries, [leader, escort, other]);

        Assert.Equal([100, 100, 100], ordinary.Select(entry => entry.PreparedWeight));
    }

    private sealed record MonsterSpawnOptions(
        bool Unique = false,
        bool Friends = false,
        bool Escort = false,
        bool Escorts = false);

    private static MonsterDefinition CreateMonster(
        string id,
        string symbol,
        int nativeLevel,
        MonsterSpawnOptions? options = null,
        int rarity = 1) =>
        new(
            id,
            new DiceRollDefinition("dice", 1, 1),
            new MonsterAiDefinition("wanderer", 0, false, false),
            [],
            [],
            new MonsterSensesDefinition(0, MonsterTelepathyProfile.Normal),
            new SpawnPolicy(
                options?.Unique ?? false,
                false,
                false,
                false,
                false,
                options?.Escort ?? false,
                options?.Escorts ?? false,
                options?.Friends ?? false,
                false),
            null,
            nativeLevel,
            rarity,
            symbol,
            MovementSpeed: 100);

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

    private sealed class RepeatingEscortRandomSource : IRandomSource
    {
        public int Next(int minInclusive, int maxExclusive) => maxExclusive switch
        {
            7 => 1,
            50 => 1,
            100 => 60,
            _ => minInclusive,
        };

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
