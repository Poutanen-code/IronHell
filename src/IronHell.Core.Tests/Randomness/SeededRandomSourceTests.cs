using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Randomness;

public sealed class SeededRandomSourceTests
{
    [Fact]
    public void IdenticalSeeds_ProduceIdenticalSequences()
    {
        var first = new SeededRandomSource(12345);
        var second = new SeededRandomSource(12345);

        Assert.Equal(
            [first.Next(0, 100), first.Next(0, 100), first.Next(0, 100)],
            [second.Next(0, 100), second.Next(0, 100), second.Next(0, 100)]);
    }

    [Fact]
    public void DifferentSeeds_ProduceDifferentSequences()
    {
        var first = new SeededRandomSource(12345);
        var second = new SeededRandomSource(54321);

        Assert.NotEqual(
            [first.Next(0, 100), first.Next(0, 100), first.Next(0, 100)],
            [second.Next(0, 100), second.Next(0, 100), second.Next(0, 100)]);
    }

    [Fact]
    public void RollDice_ConsumesTheSameSequenceAsIndividualRolls()
    {
        var dice = new SeededRandomSource(12345);
        var values = new SeededRandomSource(12345);

        Assert.Equal(values.Next(1, 7) + values.Next(1, 7) + values.Next(1, 7), dice.RollDice(3, 6));
    }
}