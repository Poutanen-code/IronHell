namespace IronHell.Core.Actions;

public sealed record ActionExecutionResult(
    bool Success,
    IReadOnlyCollection<string> AppliedChanges,
    IReadOnlyCollection<string> ValidationFailures,
    IReadOnlyCollection<IRuntimeEvent> Events);