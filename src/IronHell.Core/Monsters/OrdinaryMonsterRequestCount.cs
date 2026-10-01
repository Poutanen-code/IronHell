using IronHell.Core.Randomness;

namespace IronHell.Core.Monsters;

public static class OrdinaryMonsterRequestCount
{
    public static int Calculate(int depth, IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(randomSource);

        if (depth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(depth));
        }

        var depthTerm = Math.Clamp(depth / 3, 2, 10);
        return 14 + randomSource.Next(1, 9) + depthTerm;
    }
}
