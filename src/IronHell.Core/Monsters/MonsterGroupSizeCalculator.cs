using IronHell.Core.Randomness;

namespace IronHell.Core.Monsters;

public static class MonsterGroupSizeCalculator
{
    public const int GroupMax = 32;

    public static int Calculate(int nativeLevel, int depth, IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(randomSource);

        var total = randomSource.Next(1, 14);
        var extra = 0;
        if (nativeLevel > depth)
        {
            extra = -randomSource.Next(1, nativeLevel - depth + 1);
        }
        else if (nativeLevel < depth)
        {
            extra = randomSource.Next(1, depth - nativeLevel + 1);
            if (extra > 12)
            {
                extra = 12;
            }
        }

        total += extra;
        return Math.Clamp(total, 1, GroupMax);
    }
}
