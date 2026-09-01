using System.Diagnostics.CodeAnalysis;
using IronHell.Core.Actions;
using IronHell.Core.Characters;
using IronHell.Core.Definitions;
using IronHell.Core.Randomness;

namespace IronHell.Core.Spells;

public sealed class SpellExecutionService
{
    private readonly IActionExecutor _actionExecutor;
    private readonly IRandomSource _randomSource;

    public SpellExecutionService(IActionExecutor actionExecutor, IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(actionExecutor);
        ArgumentNullException.ThrowIfNull(randomSource);
        _actionExecutor = actionExecutor;
        _randomSource = randomSource;
    }

    public ActionExecutionResult Execute(
        IDefinitionCatalog catalog,
        string spellId,
        Character caster,
        Character? target = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentException.ThrowIfNullOrWhiteSpace(spellId);
        ArgumentNullException.ThrowIfNull(caster);

        var changes = new List<string>();
        var failures = new List<string>();
        var events = new List<IRuntimeEvent>();

        if (!TryResolveSpell(catalog, spellId, out var spell, out var sourceType))
        {
            failures.Add($"Unknown spell '{spellId}'.");
            return CreateResult(changes, failures, events);
        }

        if (spell.ActionRefs is not { Count: > 0 })
        {
            failures.Add($"Spell '{spellId}' has no executable action references.");
            return CreateResult(changes, failures, events);
        }

        foreach (var actionRef in spell.ActionRefs)
        {
            var (invocation, translationFailure) = TranslateInvocation(actionRef);
            if (invocation is null)
            {
                failures.Add(translationFailure ?? $"Action '{actionRef.ActionId}' could not be translated.");
                continue;
            }

            var actionTarget = ResolveTarget(actionRef.TargetMode, caster, target);
            if (actionTarget is null)
            {
                failures.Add($"Action '{actionRef.ActionId}' requires another character target.");
                continue;
            }

            var context = new ActionExecutionContext(catalog, caster, actionTarget, sourceType);
            var result = _actionExecutor.Execute(context, new ActionExecutionRequest(sourceType, caster, actionTarget, invocation));
            changes.AddRange(result.AppliedChanges);
            failures.AddRange(result.ValidationFailures);
            events.AddRange(result.Events);
        }

        return CreateResult(changes, failures, events);
    }

    private static bool TryResolveSpell(
        IDefinitionCatalog catalog,
        string spellId,
        [NotNullWhen(true)] out SpellDefinition? spell,
        out ActionSourceType sourceType)
    {
        if (catalog.MageSpells.TryGet(spellId, out spell))
        {
            sourceType = ActionSourceType.CharacterSpell;
            return true;
        }

        if (catalog.PriestPrayers.TryGet(spellId, out spell))
        {
            sourceType = ActionSourceType.CharacterPrayer;
            return true;
        }

        sourceType = ActionSourceType.CharacterSpell;
        return false;
    }

    private (ActionInvocation? Invocation, string? Failure) TranslateInvocation(SpellActionRef actionRef)
    {
        if (actionRef.Amount is null)
        {
            return (new ActionInvocation(actionRef.ActionId, null, actionRef.StatusId, actionRef.StatusIds), null);
        }

        var amount = ResolveAmount(actionRef.Amount);
        return amount is null
            ? (null, $"Action '{actionRef.ActionId}' uses unsupported amount kind '{actionRef.Amount.Kind}'.")
            : (new ActionInvocation(actionRef.ActionId, amount, actionRef.StatusId, actionRef.StatusIds), null);
    }

    private int? ResolveAmount(SpellActionAmount amount) => amount.Kind switch
    {
        "flat" => amount.Value,
        "dice" when amount.DiceCount is { } count && amount.DiceSides is { } sides => _randomSource.RollDice(count, sides),
        _ => null,
    };

    private static ActionTarget? ResolveTarget(ActionTargetMode targetMode, Character caster, Character? target) => targetMode switch
    {
        ActionTargetMode.Self => ActionTarget.Self(caster),
        ActionTargetMode.OtherCharacter when target is not null => ActionTarget.CharacterTarget(target),
        _ => null,
    };

    private static ActionExecutionResult CreateResult(
        List<string> changes,
        List<string> failures,
        List<IRuntimeEvent> events) =>
        new(failures.Count == 0, changes.AsReadOnly(), failures.AsReadOnly(), events.AsReadOnly());
}
