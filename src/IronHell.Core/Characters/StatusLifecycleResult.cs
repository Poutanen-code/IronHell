namespace IronHell.Core.Characters;

public sealed record StatusLifecycleResult(
    IReadOnlyCollection<string> AdvancedStatusIds,
    IReadOnlyCollection<string> ExpiredStatusIds);