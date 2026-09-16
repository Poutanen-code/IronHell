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
            new DiceRollDefinition("dice", 4, 5)));

        Assert.Equal((4, 5), randomSource.LastRoll);
        Assert.Equal(13, monster.MaxHp);
        Assert.Equal(monster.MaxHp, monster.CurrentHp);
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