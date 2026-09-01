namespace IronHell.Core.Randomness;

public interface IRandomSource
{
    int Next(int minInclusive, int maxExclusive);

    int RollDice(int count, int sides);
}