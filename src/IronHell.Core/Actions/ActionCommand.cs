using IronHell.Core.Definitions;

namespace IronHell.Core.Actions;

public sealed record ActionCommand
{
    public ActionCommand(
        string actionId,
        ActionSourceFamily sourceFamily,
        IReadOnlyDictionary<string, ActionParameterValue> parameters,
        ActionSourceActor sourceActor,
        TargetContext targetContext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actionId);
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(sourceActor);
        ArgumentNullException.ThrowIfNull(targetContext);
        ActionId = actionId;
        SourceFamily = sourceFamily;
        Parameters = parameters;
        SourceActor = sourceActor;
        TargetContext = targetContext;
    }

    public string ActionId { get; }
    public ActionSourceFamily SourceFamily { get; }
    public IReadOnlyDictionary<string, ActionParameterValue> Parameters { get; }
    public ActionSourceActor SourceActor { get; }
    public TargetContext TargetContext { get; }
}

public sealed record ActionParameterValue(object? Value);

public sealed record ActionSourceActor
{
    public ActionSourceActor(string actorId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actorId);
        ActorId = actorId;
    }

    public string ActorId { get; }
}