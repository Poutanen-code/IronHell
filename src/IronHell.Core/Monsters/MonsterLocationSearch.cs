using IronHell.Core.Randomness;

namespace IronHell.Core.Monsters;

public static class MonsterLocationSearch
{
    public static MonsterPosition? FindPosition(
        MonsterPlacementSpace space,
        MonsterRuntimeState state,
        IRandomSource randomSource,
        int maxAttempts)
    {
        ArgumentNullException.ThrowIfNull(space);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(randomSource);

        if (maxAttempts < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAttempts));
        }

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var position = new MonsterPosition(
                randomSource.Next(0, space.Width),
                randomSource.Next(0, space.Height));
            if (space.IsAvailable(position, state))
            {
                return position;
            }
        }

        return null;
    }
}
