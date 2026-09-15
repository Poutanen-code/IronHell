namespace IronHell.Core.Actions;

public interface ITargetResolver
{
    ResolvedTargets Resolve(TargetResolutionRequest request);
}

public sealed record TargetMode
{
    public TargetMode(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public static readonly TargetMode Self = new("self");
    public static readonly TargetMode Direction = new("direction");
    public static readonly TargetMode Radius = new("radius");
    public static readonly TargetMode LineOfSight = new("line_of_sight");
    public static readonly TargetMode Level = new("level");
    public static readonly TargetMode Item = new("item");
    public static readonly TargetMode EquipmentSlot = new("equipment_slot");
    public static readonly TargetMode Ally = new("ally");
    public static readonly TargetMode Point = new("point");
    public static readonly TargetMode Aimed = new("aimed");
    public static readonly TargetMode Area = new("area");
    public static readonly TargetMode Target = new("target");
    public static readonly TargetMode TriggeringActor = new("triggering_actor");
}

public sealed record TargetContext
{
    public TargetContext(TargetMode mode, IReadOnlyList<string> targetIds)
    {
        ArgumentNullException.ThrowIfNull(mode);
        ArgumentNullException.ThrowIfNull(targetIds);
        Mode = mode;
        TargetIds = targetIds;
    }

    public TargetMode Mode { get; }
    public IReadOnlyList<string> TargetIds { get; }
}

public sealed record ResolvedTargets
{
    public ResolvedTargets(IReadOnlyList<string> targetIds)
    {
        ArgumentNullException.ThrowIfNull(targetIds);
        TargetIds = targetIds;
    }

    public IReadOnlyList<string> TargetIds { get; }
}

public sealed record TargetResolutionRequest
{
    public TargetResolutionRequest(ActionCommand command, TargetContext context)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(context);
        Command = command;
        Context = context;
    }

    public ActionCommand Command { get; }
    public TargetContext Context { get; }
}