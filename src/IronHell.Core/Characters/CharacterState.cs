using IronHell.Core.Definitions;

namespace IronHell.Core.Characters;

/// <summary>
/// Deterministic character state produced by CharacterFactory.
/// Fields that require RNG (HP array, mana, stats after rolling, age, gold) are not included here.
/// </summary>
public sealed record CharacterState(
    string RaceId,
    string ClassId,
    /// <summary>race.stat_modifiers + class.stat_modifiers. Applied to rolled stats via modify_stat_value at rolling time.</summary>
    StatModifiers CombinedStatModifiers,
    /// <summary>race.skill_modifiers + class.base_skills. Skill growth (class.skill_growth × level / 10) is applied at runtime.</summary>
    SkillSet BaseSkills,
    /// <summary>Race capability IDs followed by class capability IDs, deduplicated, order preserved.</summary>
    IReadOnlyList<string> CapabilityIds,
    /// <summary>Class starting equipment with verified min/max counts. Copied verbatim from class definition.</summary>
    IReadOnlyList<StartingEquipmentEntry> StartingEquipment,
    /// <summary>race.hit_die + class.hit_die. Die size used in HP pre-roll array.</summary>
    int HitDie,
    /// <summary>race.exp_factor + class.exp_factor. Multiplier for XP thresholds.</summary>
    int ExpFact);
