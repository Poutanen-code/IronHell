using IronHell.Core.Definitions;
using IronHell.Core.Monsters;
using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Monsters;

public sealed class MonsterFactoryTests
{
    [Fact]
    public void Create_RollsHpAndInitializesCurrentAndMaximumHp()
    {
        var randomSource = new RecordingRandomSource(rolledValue: 13);
        var factory = new MonsterFactory(randomSource);

        var monster = factory.Create("monster-1", new MonsterDefinition(
            "test_monster",
            new DiceRollDefinition("dice", 4, 5),
            new MonsterAiDefinition("coward", 75, true, false),
            ["open_doors", "take_items"],
            ["imm_fire", "imm_sleep"],
            new MonsterSensesDefinition(20, MonsterTelepathyProfile.WeirdMind),
            new SpawnPolicy(false, false, false, false, false, false, false, false, false),
            "test_loot"));

        Assert.Equal((4, 5), randomSource.LastRoll);
        Assert.Equal(13, monster.MaxHp);
        Assert.Equal(monster.MaxHp, monster.CurrentHp);
        Assert.Equal(new MonsterAiDefinition("coward", 75, true, false), monster.Ai);
        Assert.Equal(["open_doors", "take_items"], monster.Capabilities);
        Assert.Equal(["imm_fire", "imm_sleep"], monster.Resistances);
        Assert.Equal(new MonsterSensesDefinition(20, MonsterTelepathyProfile.WeirdMind), monster.Senses);
        Assert.Equal(new SpawnPolicy(false, false, false, false, false, false, false, false, false), monster.SpawnPolicy);
        Assert.Equal("test_loot", monster.LootProfileId);
    }

    private sealed class RecordingRandomSource(int rolledValue) : IRandomSource
    {
        public (int Count, int Sides)? LastRoll { get; private set; }

        public int Next(int minInclusive, int maxExclusive) => throw new NotSupportedException();

        public int RollDice(int count, int sides)
        {
            LastRoll = (count, sides);
            return rolledValue;
        }
    }
}