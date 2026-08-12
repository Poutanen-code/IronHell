namespace IronHell.Core.Definitions;

/// <summary>Immutable class definition loaded from classes.json.</summary>
public sealed record ClassDefinition(
    string Id,
    string Name,
    StatModifiers StatModifiers,
    SkillSet BaseSkills,
    SkillSet SkillGrowth,
    int HitDie,
    int ExpFactor,
    string? SpellStat,
    int FirstSpellLevel,
    int SpellWeight,
    int MaxAttacks,
    int MinWeight,
    int AttackMultiplier,
    int SenseBase,
    int SenseDiv,
    IReadOnlyList<string> CapabilityIds,
    IReadOnlyList<StartingEquipmentEntry> StartingEquipment);
