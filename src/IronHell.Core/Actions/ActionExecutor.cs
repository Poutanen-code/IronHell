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
        foreach (var action in actions)
        {
            if (!context.Catalog.Actions.TryGet(action.ActionId, out _))
            {
                failures.Add($"Unknown action '{action.ActionId}'.");
                continue;
            }

            ExecuteKnownAction(context, action, changes, failures);
        }

        return new ActionExecutionResult(
            failures.Count == 0,
            changes.AsReadOnly(),
            failures.AsReadOnly());
    }

    private void ExecuteKnownAction(
        ActionExecutionContext context,
        ActionInvocation action,
        List<string> changes,
        List<string> failures)
    {
        switch (action.ActionId)
        {
            case "ApplyDamage":
                ApplyDamage(context.Target, action.Amount, changes, failures);
                break;
            case "HealHP":
                Heal(context.Target, action.Amount, changes, failures);
                break;
            case "ApplyStatus":
                ApplyStatus(context, action, changes, failures);
                break;
            case "CureStatus":
                CureStatuses(context, action.StatusIds, changes, failures);
                break;
            default:
                failures.Add($"Action '{action.ActionId}' is not implemented.");
                break;
        }
    }

    private static void ApplyDamage(Character target, int? amount, List<string> changes, List<string> failures)
    {
        if (amount is null || amount < 0)
        {
            failures.Add("ApplyDamage requires a non-negative amount.");
            return;
        }

        var result = CharacterResourceService.Modify(target, CharacterResourceType.HitPoints, -amount.Value);
        changes.Add($"ApplyDamage:{result.BeforeValue}->{result.AfterValue}");
    }

    private static void Heal(Character target, int? amount, List<string> changes, List<string> failures)
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
    }

    private void ApplyStatus(ActionExecutionContext context, ActionInvocation action, List<string> changes, List<string> failures)
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

        StatusApplicationService.ApplyStatus(
            context.Target,
            action.StatusId,
            _durationResolver.ResolveDuration(statusDefinition, new DurationResolutionContext(context.Source.State.Level)),
            statusDefinition.ApplicationPolicy);
        changes.Add($"ApplyStatus:{action.StatusId}");
    }

    private static void CureStatuses(ActionExecutionContext context, IReadOnlyCollection<string>? statusIds, List<string> changes, List<string> failures)
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

        var removed = context.Target.RemoveStatuses(statusIds);
        changes.Add($"CureStatus:{removed}");
    }
}