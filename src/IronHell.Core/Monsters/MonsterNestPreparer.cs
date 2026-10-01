using IronHell.Core.Definitions;
using IronHell.Core.Randomness;

namespace IronHell.Core.Monsters;

public enum MonsterNestFamily
{
    Jelly,
    Animal,
    Undead,
}

public sealed record MonsterNestPreparationResult(
    bool IsSuccess,
    MonsterNestFamily Family,
    IReadOnlyList<string> CandidateDefinitionIds);

public static class MonsterNestPreparer
{
    public const int SampleCount = 64;

    public static MonsterNestFamily SelectFamily(int depth, IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(randomSource);

        if (depth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(depth));
        }

        var familyRoll = randomSource.Next(1, depth + 1);
        if (familyRoll < 30)
        {
            return MonsterNestFamily.Jelly;
        }

        return familyRoll < 50
            ? MonsterNestFamily.Animal
            : MonsterNestFamily.Undead;
    }

    public static MonsterNestPreparationResult Prepare(
        IEnumerable<MonsterDefinition> definitions,
        IReadOnlyList<MonsterAllocationEntry> allocationEntries,
        int depth,
        IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(allocationEntries);
        ArgumentNullException.ThrowIfNull(randomSource);

        var family = SelectFamily(depth, randomSource);
        var definitionList = definitions.ToArray();
        var prepared = MonsterAllocationTableBuilder.Prepare(
            allocationEntries,
            definitionList,
            definition => IsEligible(definition, family));
        var candidates = new List<string>(SampleCount);

        for (var index = 0; index < SampleCount; index++)
        {
            var effectiveEntries = MonsterAllocationEligibility.PrepareForSelection(
                prepared,
                depth + 10,
                randomSource);
            var selected = MonsterAllocationSelector.SelectWithComparison(effectiveEntries, randomSource);
            if (selected is null)
            {
                return new MonsterNestPreparationResult(false, family, []);
            }

            candidates.Add(selected.PreparedEntry.BaseEntry.MonsterDefinitionId);
        }

        return new MonsterNestPreparationResult(true, family, candidates.AsReadOnly());
    }

    private static bool IsEligible(MonsterDefinition definition, MonsterNestFamily family) =>
        !definition.SpawnPolicy.Unique && family switch
        {
            MonsterNestFamily.Jelly => definition.Symbol.Length == 1 && "ijm,".Contains(definition.Symbol[0]),
            MonsterNestFamily.Animal => definition.Categories?.Contains("animal", StringComparer.Ordinal) == true,
            MonsterNestFamily.Undead => definition.Categories?.Contains("undead", StringComparer.Ordinal) == true,
            _ => false,
        };
}
