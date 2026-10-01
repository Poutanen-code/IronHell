using IronHell.Core.Definitions;

namespace IronHell.Core.Monsters;

public sealed record MonsterAllocationEntry(
    string MonsterDefinitionId,
    int NativeLevel,
    int BaseWeight);

public sealed record MonsterAllocationPreparedEntry(
    MonsterAllocationEntry BaseEntry,
    int PreparedWeight);

public static class MonsterAllocationTableBuilder
{
    public static IReadOnlyList<MonsterAllocationEntry> Build(IEnumerable<MonsterDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        return definitions
            .Where(definition => definition.Rarity != 0)
            .Select(definition => new MonsterAllocationEntry(
                definition.Id,
                definition.NativeLevel,
                100 / definition.Rarity))
            .OrderBy(entry => entry.NativeLevel)
            .ToArray();
    }

    public static IReadOnlyList<MonsterAllocationPreparedEntry> Prepare(
        IEnumerable<MonsterAllocationEntry> entries,
        IEnumerable<MonsterDefinition> definitions,
        Func<MonsterDefinition, bool>? eligibilityHook = null)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(definitions);

        var definitionsById = definitions.ToDictionary(definition => definition.Id, StringComparer.Ordinal);
        return entries
            .Select(entry =>
            {
                if (!definitionsById.TryGetValue(entry.MonsterDefinitionId, out var definition))
                {
                    throw new KeyNotFoundException($"Monster definition '{entry.MonsterDefinitionId}' was not provided.");
                }

                var preparedWeight = eligibilityHook is null || eligibilityHook(definition)
                    ? entry.BaseWeight
                    : 0;
                return new MonsterAllocationPreparedEntry(entry, preparedWeight);
            })
            .ToArray();
    }
}
