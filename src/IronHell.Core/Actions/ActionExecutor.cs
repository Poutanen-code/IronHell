using IronHell.Core.Characters;

namespace IronHell.Core.Actions;

public sealed class ActionExecutor : IActionExecutor
{
    private readonly IDurationResolver _durationResolver;

    public ActionExecutor(IDurationResolver? durationResolver = null)
    {
        _durationResolver = durationResolver ?? new FixedDurationResolver();
    }

    public ActionExecutionResult Execute(ActionExecutionContext context, IReadOnlyCollection<ActionInvocation> actions)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(actions);

        var changes = new List<string>();
        var failures = new List<string>();
        var events = new List<IRuntimeEvent>();
        foreach (var action in actions)
        {
            var result = Execute(context, new ActionExecutionRequest(context.SourceType, context.Source, context.Target, action));
            changes.AddRange(result.AppliedChanges);
            failures.AddRange(result.ValidationFailures);
            events.AddRange(result.Events);
        }

        return new ActionExecutionResult(
            failures.Count == 0,
            changes.AsReadOnly(),
            failures.AsReadOnly(),
            events.AsReadOnly());
    }

    public ActionExecutionResult Execute(ActionExecutionContext context, ActionExecutionRequest request)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(request);

        var changes = new List<string>();
        var failures = new List<string>();
        var events = new List<IRuntimeEvent>();
        if (!context.Catalog.Actions.TryGet(request.Action.ActionId, out var definition))
        {
            failures.Add($"Unknown action '{request.Action.ActionId}'.");
        }
        else if (ActionExecutionValidator.Validate(request, definition) is { } failure)
        {
            failures.Add(failure);
        }
        else
        {
            var resolvedContext = new ActionExecutionContext(context.Catalog, request.SourceCharacter, request.Target, request.SourceType);
            ExecuteKnownAction(resolvedContext, request.Action, changes, failures, events);
        }

        return new ActionExecutionResult(
            failures.Count == 0,
            changes.AsReadOnly(),
            failures.AsReadOnly(),
            events.AsReadOnly());
    }

            ActionExecutionResult IActionExecutor.Execute(ActionExecutionContext context, ActionExecutionRequest request) => Execute(context, request);

    private void ExecuteKnownAction(
        ActionExecutionContext context,
        ActionInvocation action,
        List<string> changes,
        List<string> failures,
        List<IRuntimeEvent> events)
    {
        switch (action.ActionId)
        {
            case "ApplyDamage":
                ApplyDamage(context.Target.Character, action.Amount, changes, failures, events);
                break;
            case "HealHP":
                Heal(context.Target.Character, action.Amount, changes, failures, events);
                break;
            case "ApplyStatus":
                ApplyStatus(context, action, changes, failures, events);
                break;
            case "CureStatus":
                CureStatuses(context, action.StatusIds, changes, failures, events);
                break;
            default:
                failures.Add($"Action '{action.ActionId}' is not implemented.");
                break;
        }
    }

    private static void ApplyDamage(Character target, int? amount, List<string> changes, List<string> failures, List<IRuntimeEvent> events)
    {
        if (amount is null || amount < 0)
        {
            failures.Add("ApplyDamage requires a non-negative amount.");
            return;
        }

        var wasDead = target.State.IsDead;
        var result = CharacterResourceService.Modify(target, CharacterResourceType.HitPoints, -amount.Value);
        changes.Add($"ApplyDamage:{result.BeforeValue}->{result.AfterValue}");
        if (result.BeforeValue != result.AfterValue)
        {
            events.Add(new HpChangedEvent(target.CharacterId, result.BeforeValue, result.AfterValue));
        }

        if (!wasDead && target.State.IsDead)
        {
            events.Add(new CharacterDiedEvent(target.CharacterId));
        }
    }

    private static void Heal(Character target, int? amount, List<string> changes, List<string> failures, List<IRuntimeEvent> events)
    {
        if (amount is null || amount < 0)
        {
            failures.Add("HealHP requires a non-negative amount.");
            return;
        }

        var result = CharacterResourceService.Modify(target, CharacterResourceType.HitPoints, amount.Value);
        if (!result.Success)
        {
            failures.Add(result.Failure ?? "Resource modification failed.");
            return;
        }

        changes.Add($"HealHP:{result.BeforeValue}->{result.AfterValue}");
        if (result.BeforeValue != result.AfterValue)
        {
            events.Add(new HpChangedEvent(target.CharacterId, result.BeforeValue, result.AfterValue));
        }
    }

    private void ApplyStatus(ActionExecutionContext context, ActionInvocation action, List<string> changes, List<string> failures, List<IRuntimeEvent> events)
    {
        if (string.IsNullOrWhiteSpace(action.StatusId))
        {
            failures.Add("ApplyStatus requires a status ID.");
            return;
        }

        if (!context.Catalog.Statuses.TryGet(action.StatusId, out var statusDefinition))
        {
            failures.Add($"Unknown status '{action.StatusId}'.");
            return;
        }

        var result = StatusApplicationService.ApplyStatus(
            context.Target.Character,
            action.StatusId,
            _durationResolver.ResolveDuration(statusDefinition, new DurationResolutionContext(context.Source.State.Level)),
            statusDefinition.ApplicationPolicy);
        changes.Add($"ApplyStatus:{action.StatusId}");
        if (result.Applied)
        {
            events.Add(new StatusAppliedEvent(context.Target.Character.CharacterId, result.ActiveStatus.StatusId, result.ActiveStatus.RemainingDuration));
        }
    }

    private static void CureStatuses(ActionExecutionContext context, IReadOnlyCollection<string>? statusIds, List<string> changes, List<string> failures, List<IRuntimeEvent> events)
    {
        if (statusIds is null || statusIds.Count == 0)
        {
            failures.Add("CureStatus requires at least one status ID.");
            return;
        }

        foreach (var statusId in statusIds)
        {
            if (!context.Catalog.Statuses.TryGet(statusId, out _))
            {
                failures.Add($"Unknown status '{statusId}'.");
                return;
            }
        }

        var target = context.Target.Character;
        var removedStatuses = target.ActiveStatuses
            .Where(status => statusIds.Contains(status.StatusId, StringComparer.Ordinal))
            .Select(status => status.StatusId)
            .ToArray();
        var removed = target.RemoveStatuses(statusIds);
        changes.Add($"CureStatus:{removed}");
        foreach (var statusId in removedStatuses)
        {
            events.Add(new StatusRemovedEvent(target.CharacterId, statusId));
        }
    }
}