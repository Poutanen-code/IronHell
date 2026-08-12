namespace IronHell.Core.Definitions;

/// <summary>Immutable race definition loaded from races.json.</summary>
public sealed record RaceDefinition(
    string Id,
    string Name,
    StatModifiers StatModifiers,
    SkillSet SkillModifiers,
    int HitDie,
    int ExpFactor,
    int Infravision,
    int HistoryChart,
    IReadOnlyList<string> CapabilityIds);
