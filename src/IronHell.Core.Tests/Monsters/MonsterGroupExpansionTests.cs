using IronHell.Core.Definitions;
using IronHell.Core.Monsters;
using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Monsters;

public sealed class MonsterGroupExpansionTests
{
    [Fact]
    public void Friends_GroupExpansionDisabled_PlacesNoAdditionalMembers()
    {
        var state = new MonsterRuntimeState();
        var definition = CreateMonster("friend", friends: true);
        var leader = MonsterPlacementService.Place(state, definition, new MonsterPosition(2, 2));
        var leaderMonster = leader.Monster;
        Assert.NotNull(leaderMonster);

        var result = MonsterGroupExpander.Expand(
            state,
            leaderMonster,
            definition,
            new MonsterPlacementSpace(5, 5),
            depth: 10,
            allowGroupExpansion: false,
            new ScriptedRandomSource());

        Assert.Equal(1, result.DesiredGroupSize);
        Assert.Equal(1, result.SuccessfulGroupSize);
        Assert.Single(state.Monsters);
    }

    [Fact]
    public void NonFriends_GroupExpansionEnabled_PlacesNoAdditionalMembers()
    {
        var state = new MonsterRuntimeState();
        var definition = CreateMonster("ordinary", friends: false);
        var leader = MonsterPlacementService.Place(state, definition, new MonsterPosition(2, 2));
        var leaderMonster = leader.Monster;
        Assert.NotNull(leaderMonster);

        var result = MonsterGroupExpander.Expand(
            state,
            leaderMonster,
            definition,
            new MonsterPlacementSpace(5, 5),
            depth: 10,
            allowGroupExpansion: true,
            new ScriptedRandomSource());

        Assert.Equal(1, result.DesiredGroupSize);
        Assert.Equal(1, result.SuccessfulGroupSize);
        Assert.Single(state.Monsters);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(13, 13)]
    public void Friends_GroupSizeRollUsesOneThroughThirteen(int roll, int expected)
    {
        var size = MonsterGroupSizeCalculator.Calculate(10, 10, new ScriptedRandomSource(roll));

        Assert.Equal(expected, size);
    }

    [Fact]
    public void Friends_EasierThanDepth_BiasesSizeAccordingToVerifiedFormula()
    {
        var size = MonsterGroupSizeCalculator.Calculate(5, 10, new ScriptedRandomSource(1, 5));

        Assert.Equal(6, size);
    }

    [Fact]
    public void Friends_DeeperThanDepth_BiasesSizeAccordingToVerifiedFormula()
    {
        var size = MonsterGroupSizeCalculator.Calculate(15, 10, new ScriptedRandomSource(13, 5));

        Assert.Equal(8, size);
    }

    [Fact]
    public void Friends_AdjustedSizeNeverFallsBelowOne()
    {
        var size = MonsterGroupSizeCalculator.Calculate(15, 10, new ScriptedRandomSource(1, 5));

        Assert.Equal(1, size);
    }

    [Fact]
    public void Friends_GroupSizeNeverExceeds32()
    {
        var size = MonsterGroupSizeCalculator.Calculate(1, 100, new ScriptedRandomSource(13, 99));

        Assert.InRange(size, 1, 32);
    }

    [Fact]
    public void Friends_DesiredGroupSizeOne_LeavesLeaderOnly()
    {
        var state = new MonsterRuntimeState();
        var definition = CreateMonster("friend", friends: true);
        var leader = MonsterPlacementService.Place(state, definition, new MonsterPosition(2, 2));
        var leaderMonster = leader.Monster;
        Assert.NotNull(leaderMonster);

        var result = MonsterGroupExpander.Expand(
            state,
            leaderMonster,
            definition,
            new MonsterPlacementSpace(5, 5),
            depth: 1,
            allowGroupExpansion: true,
            new ScriptedRandomSource(1));

        Assert.Equal(1, result.DesiredGroupSize);
        Assert.Equal(1, result.SuccessfulGroupSize);
        Assert.Single(state.Monsters);
    }

    [Fact]
    public void Friends_AllAdditionalMembersUseLeaderDefinitionId()
    {
        var state = new MonsterRuntimeState();
        var definition = CreateMonster("friend", friends: true);
        var leader = MonsterPlacementService.Place(state, definition, new MonsterPosition(2, 2));
        var leaderMonster = leader.Monster;
        Assert.NotNull(leaderMonster);

        var result = MonsterGroupExpander.Expand(
            state,
            leaderMonster,
            definition,
            new MonsterPlacementSpace(5, 5),
            depth: 1,
            allowGroupExpansion: true,
            new ScriptedRandomSource(3));

        Assert.Equal(3, result.DesiredGroupSize);
        Assert.Equal(3, result.SuccessfulGroupSize);
        Assert.All(state.Monsters, monster => Assert.Equal("friend", monster.DefinitionId));
    }

    [Fact]
    public void Friends_ExpansionIsBreadthFirst()
    {
        var leaderPosition = new MonsterPosition(2, 2);
        var firstFriendPosition = new MonsterPosition(2, 3);
        var secondFriendPosition = new MonsterPosition(2, 4);
        var legal = new[] { leaderPosition, firstFriendPosition, secondFriendPosition };
        var space = new MonsterPlacementSpace(
            5,
            5,
            Enumerable.Range(0, 5)
                .SelectMany(x => Enumerable.Range(0, 5).Select(y => new MonsterPosition(x, y)))
                .Except(legal));
        var state = new MonsterRuntimeState();
        var definition = CreateMonster("friend", friends: true);
        var leader = MonsterPlacementService.Place(state, definition, leaderPosition);
        var leaderMonster = leader.Monster;
        Assert.NotNull(leaderMonster);

        var result = MonsterGroupExpander.Expand(
            state,
            leaderMonster,
            definition,
            space,
            depth: 1,
            allowGroupExpansion: true,
            new ScriptedRandomSource(3));

        Assert.Equal(3, result.SuccessfulGroupSize);
        Assert.Contains(state.Monsters, monster => monster.Position == firstFriendPosition);
        Assert.Contains(state.Monsters, monster => monster.Position == secondFriendPosition);
    }

    [Fact]
    public void Friends_BlockedAdjacentCells_DoNotAbortLeader()
    {
        var position = new MonsterPosition(1, 1);
        var state = new MonsterRuntimeState();
        var definition = CreateMonster("friend", friends: true);
        var leader = MonsterPlacementService.Place(state, definition, position);
        var leaderMonster = leader.Monster;
        Assert.NotNull(leaderMonster);
        var space = new MonsterPlacementSpace(
            3,
            3,
            Enumerable.Range(0, 3)
                .SelectMany(x => Enumerable.Range(0, 3).Select(y => new MonsterPosition(x, y)))
                .Where(candidate => candidate != position));

        var result = MonsterGroupExpander.Expand(
            state,
            leaderMonster,
            definition,
            space,
            depth: 1,
            allowGroupExpansion: true,
            new ScriptedRandomSource(13));

        Assert.Equal(1, result.SuccessfulGroupSize);
        Assert.Single(state.Monsters);
    }

    private static MonsterDefinition CreateMonster(string id, bool friends) =>
        new(
            id,
            new DiceRollDefinition("dice", 1, 1),
            new MonsterAiDefinition("wanderer", 0, false, false),
            [],
            [],
            new MonsterSensesDefinition(0, MonsterTelepathyProfile.Normal),
            new SpawnPolicy(false, false, false, false, false, false, false, friends, false),
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
