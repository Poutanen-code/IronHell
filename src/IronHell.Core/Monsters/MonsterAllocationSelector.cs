using IronHell.Core.Randomness;

namespace IronHell.Core.Monsters;

public static class MonsterAllocationSelector
{
    public static MonsterAllocationPreparedEntry? Select(
        IReadOnlyList<MonsterAllocationPreparedEntry> entries,
        IRandomSource randomSource)
        => SelectWeighted(entries, entry => entry.PreparedWeight, randomSource);

    public static MonsterAllocationEffectiveEntry? Select(
        IReadOnlyList<MonsterAllocationEffectiveEntry> entries,
        IRandomSource randomSource)
        => SelectWeighted(entries, entry => entry.EffectiveWeight, randomSource);

    public static MonsterAllocationPreparedEntry? SelectWithComparison(
        IReadOnlyList<MonsterAllocationPreparedEntry> entries,
        IRandomSource randomSource)
        => SelectWithComparisonCore(
            entries,
            entry => entry.PreparedWeight,
            entry => entry.BaseEntry.NativeLevel,
            randomSource);

    public static MonsterAllocationEffectiveEntry? SelectWithComparison(
        IReadOnlyList<MonsterAllocationEffectiveEntry> entries,
        IRandomSource randomSource)
        => SelectWithComparisonCore(
            entries,
            entry => entry.EffectiveWeight,
            entry => entry.PreparedEntry.BaseEntry.NativeLevel,
            randomSource);

    private static T? SelectWeighted<T>(
        IReadOnlyList<T> entries,
        Func<T, int> weightSelector,
        IRandomSource randomSource)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(randomSource);

        var totalWeight = entries
            .Select(weightSelector)
            .Where(weight => weight > 0)
            .Aggregate(0, (total, weight) => checked(total + weight));

        if (totalWeight == 0)
        {
            return null;
        }

        var draw = randomSource.Next(0, totalWeight);
        var cumulativeWeight = 0;
        foreach (var entry in entries)
        {
            var weight = weightSelector(entry);
            if (weight <= 0)
            {
                continue;
            }

            cumulativeWeight = checked(cumulativeWeight + weight);
            if (draw < cumulativeWeight)
            {
                return entry;
            }
        }

        throw new InvalidOperationException("The weighted allocation draw did not select an entry.");
    }

    private static T? SelectWithComparisonCore<T>(
        IReadOnlyList<T> entries,
        Func<T, int> weightSelector,
        Func<T, int> nativeLevelSelector,
        IRandomSource randomSource)
        where T : class
    {
        var current = SelectWeighted(entries, weightSelector, randomSource);
        if (current is null)
        {
            return null;
        }

        var comparisonControl = randomSource.Next(0, 100);
        if (comparisonControl < 60)
        {
            var comparisonCandidate = SelectWeighted(entries, weightSelector, randomSource);
            if (comparisonCandidate is not null && IsCloserToZero(comparisonCandidate, current, nativeLevelSelector))
            {
                current = comparisonCandidate;
            }
        }

        if (comparisonControl < 10)
        {
            var comparisonCandidate = SelectWeighted(entries, weightSelector, randomSource);
            if (comparisonCandidate is not null && IsCloserToZero(comparisonCandidate, current, nativeLevelSelector))
            {
                current = comparisonCandidate;
            }
        }

        return current;
    }

    private static bool IsCloserToZero<T>(
        T candidate,
        T current,
        Func<T, int> nativeLevelSelector) =>
        Math.Abs(nativeLevelSelector(candidate)) < Math.Abs(nativeLevelSelector(current));
}
