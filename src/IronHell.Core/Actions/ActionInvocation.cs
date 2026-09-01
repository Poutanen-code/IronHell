namespace IronHell.Core.Actions;

public sealed record ActionInvocation(
    string ActionId,
    int? Amount = null,
    string? StatusId = null,
    IReadOnlyCollection<string>? StatusIds = null);