using IronHell.Core.Definitions;

namespace IronHell.Core.Characters;

public static class CharacterFactory
{
    /// <summary>
    /// Creates deterministic definition-derived character data from a race and class definition.
    /// No RNG is used. Fields requiring dice rolls (stats, HP, mana, age, gold) are not populated.
    /// </summary>
    /// <exception cref="ArgumentNullException">Any argument is null.</exception>
    /// <exception cref="InvalidOperationException">The race/class combination is not in allowedCombinations.</exception>
    public static CharacterDefinitionState Create(
        RaceDefinition race,
        ClassDefinition @class,
        IReadOnlyCollection<RaceClassRule> allowedCombinations)
    {
        ArgumentNullException.ThrowIfNull(race);
        ArgumentNullException.ThrowIfNull(@class);
        ArgumentNullException.ThrowIfNull(allowedCombinations);

        if (!allowedCombinations.Any(r => r.RaceId == race.Id && r.ClassId == @class.Id))
            throw new InvalidOperationException(
                $"Race '{race.Id}' cannot be class '{@class.Id}'.");

        // Race capabilities come first; class capabilities follow. Duplicates removed, order preserved.
        var capabilities = race.CapabilityIds
            .Concat(@class.CapabilityIds)
            .Distinct()
            .ToList()
            .AsReadOnly();

        return new CharacterDefinitionState(
            RaceId:                race.Id,
            ClassId:               @class.Id,
            CombinedStatModifiers: race.StatModifiers + @class.StatModifiers,
            BaseSkills:            race.SkillModifiers + @class.BaseSkills,
            CapabilityIds:         capabilities,
            StartingEquipment:     @class.StartingEquipment,
            HitDie:                race.HitDie  + @class.HitDie,
            ExpFact:               race.ExpFactor + @class.ExpFactor);
    }
}
