namespace IronHell.Core.Monsters;

using IronHell.Core.Randomness;

public sealed record MonsterAllocationEffectiveEntry(
    MonsterAllocationPreparedEntry PreparedEntry,
    int EffectiveWeight);

public static class MonsterAllocationEligibility
{
    public static IReadOnlyList<MonsterAllocationEffectiveEntry> PrepareForSelection(
        IReadOnlyList<MonsterAllocationPreparedEntry> entries,
        int requestedLevel,
        IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(randomSource);

        var effectiveLevel = MonsterAllocationLevelResolver.ResolveEffectiveLevel(requestedLevel, randomSource);
        return Apply(entries, effectiveLevel);
    }

    public static IReadOnlyList<MonsterAllocationEffectiveEntry> Apply(
        IReadOnlyList<MonsterAllocationPreparedEntry> entries,
        int effectiveLevel)
    {
        ArgumentNullException.ThrowIfNull(entries);

        return entries
            .Select(entry => new MonsterAllocationEffectiveEntry(
                entry,
                IsEligible(entry, effectiveLevel) ? entry.PreparedWeight : 0))
            .ToArray();
    }

    private static bool IsEligible(MonsterAllocationPreparedEntry entry, int effectiveLevel)
    {
        if (entry.PreparedWeight <= 0 || entry.BaseEntry.NativeLevel > effectiveLevel)
        {
            return false;
        }

        return effectiveLevel <= 0 || entry.BaseEntry.NativeLevel > 0;
    }
}
