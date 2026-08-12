namespace IronHell.Core.Definitions;

/// <summary>One allowed race/class pair from race_class_rules.json. Absence means the combination is forbidden.</summary>
public sealed record RaceClassRule(string RaceId, string ClassId);
