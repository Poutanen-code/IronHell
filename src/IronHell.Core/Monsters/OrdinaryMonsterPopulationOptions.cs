namespace IronHell.Core.Monsters;

public sealed record OrdinaryMonsterPopulationOptions(
    int LocationAttemptLimit,
    bool AllowGroupExpansion = false);
