using IronHell.Core.Definitions;
using IronHell.Core.Randomness;

namespace IronHell.Core.Monsters;

[Flags]
public enum MonsterDragonBreathMasks
{
    None = 0,
    Acid = 1,
    Electric = 2,
    Fire = 4,
    Cold = 8,
    Poison = 16,
    MultiHued = Acid | Electric | Fire | Cold | Poison,
}

public enum MonsterPitFamily
{
    Orc,
    Troll,
    Giant,
    Dragon,
    Demon,
}

public sealed record MonsterPitSelection(
    MonsterPitFamily Family,
    MonsterDragonBreathMasks? DragonMask);

public sealed record MonsterPitPreparationResult(
    bool IsSuccess,
    MonsterPitSelection Selection,
    IReadOnlyList<string> SampledCandidateDefinitionIds,
    IReadOnlyList<string> SortedCandidateDefinitionIds,
    IReadOnlyList<string> TierDefinitionIds);

public static class MonsterPitPreparer
{
    public const int SampleCount = 16;

    public static MonsterPitSelection SelectFamily(int depth, IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(randomSource);

        if (depth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(depth));
        }

        var familyRoll = randomSource.Next(1, depth + 1);
        if (familyRoll < 20)
        {
            return new MonsterPitSelection(MonsterPitFamily.Orc, null);
        }

        if (familyRoll < 40)
        {
            return new MonsterPitSelection(MonsterPitFamily.Troll, null);
        }

        if (familyRoll < 60)
        {
            return new MonsterPitSelection(MonsterPitFamily.Giant, null);
        }

        if (familyRoll < 80)
        {
            var maskRoll = randomSource.Next(0, 6);
            var mask = maskRoll switch
            {
                0 => MonsterDragonBreathMasks.Acid,
                1 => MonsterDragonBreathMasks.Electric,
                2 => MonsterDragonBreathMasks.Fire,
                3 => MonsterDragonBreathMasks.Cold,
                4 => MonsterDragonBreathMasks.Poison,
                5 => MonsterDragonBreathMasks.MultiHued,
                _ => throw new InvalidOperationException("Unsupported dragon mask roll."),
            };
            return new MonsterPitSelection(MonsterPitFamily.Dragon, mask);
        }

        return new MonsterPitSelection(MonsterPitFamily.Demon, null);
    }

    public static MonsterPitPreparationResult Prepare(
        IEnumerable<MonsterDefinition> definitions,
        IReadOnlyList<MonsterAllocationEntry> allocationEntries,
        int depth,
        IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(allocationEntries);
        ArgumentNullException.ThrowIfNull(randomSource);

        var selection = SelectFamily(depth, randomSource);
        var definitionList = definitions.ToArray();
        var prepared = MonsterAllocationTableBuilder.Prepare(
            allocationEntries,
            definitionList,
            definition => MonsterPitEligibility.IsEligible(definition, selection));
        var definitionsById = definitionList.ToDictionary(definition => definition.Id, StringComparer.Ordinal);
        var sampled = new List<string>(SampleCount);

        for (var index = 0; index < SampleCount; index++)
        {
            var effectiveEntries = MonsterAllocationEligibility.PrepareForSelection(
                prepared,
                depth + 10,
                randomSource);
            var selected = MonsterAllocationSelector.SelectWithComparison(effectiveEntries, randomSource);
            if (selected is null)
            {
                return new MonsterPitPreparationResult(false, selection, [], [], []);
            }

            sampled.Add(selected.PreparedEntry.BaseEntry.MonsterDefinitionId);
        }

        var sorted = sampled
            .Select(id => definitionsById[id])
            .OrderBy(definition => definition.NativeLevel)
            .Select(definition => definition.Id)
            .ToArray();
        var tiers = sorted
            .Where((_, index) => index % 2 == 0)
            .ToArray();

        return new MonsterPitPreparationResult(
            true,
            selection,
            sampled.AsReadOnly(),
            sorted,
            tiers);
    }
}

public static class MonsterPitEligibility
{
    public static bool IsEligible(
        MonsterDefinition candidate,
        MonsterPitSelection selection)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(selection);

        if (candidate.SpawnPolicy.Unique)
        {
            return false;
        }

        return selection.Family switch
        {
            MonsterPitFamily.Orc => candidate.Symbol == "o",
            MonsterPitFamily.Troll => candidate.Symbol == "T",
            MonsterPitFamily.Giant => candidate.Symbol == "P",
            MonsterPitFamily.Demon => candidate.Symbol == "U",
            MonsterPitFamily.Dragon => candidate.Symbol is "d" or "D" &&
                MonsterDragonBreath.GetExactMask(candidate) == selection.DragonMask,
            _ => false,
        };
    }
}

public static class MonsterDragonBreath
{
    public static MonsterDragonBreathMasks GetExactMask(MonsterDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var mask = MonsterDragonBreathMasks.None;
        foreach (var ability in definition.Abilities ?? [])
        {
            mask |= ability switch
            {
                "breath_acid" => MonsterDragonBreathMasks.Acid,
                "breath_elec" => MonsterDragonBreathMasks.Electric,
                "breath_fire" => MonsterDragonBreathMasks.Fire,
                "breath_cold" => MonsterDragonBreathMasks.Cold,
                "breath_poison" => MonsterDragonBreathMasks.Poison,
                _ => MonsterDragonBreathMasks.None,
            };
        }

        return mask;
    }
}
