using IronHell.Core.Definitions;

namespace IronHell.Core.Actions;

public sealed class ActionValidationService
{
    private readonly IActionReferenceResolver? _referenceResolver;

    public ActionValidationService(IActionReferenceResolver? referenceResolver = null)
    {
        _referenceResolver = referenceResolver;
    }

    public ActionValidationResult Validate(
        ActionCommand command,
        IDefinitionRegistry<ActionDefinition> actionDefinitions)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(actionDefinitions);

        if (!actionDefinitions.TryGet(command.ActionId, out var definition))
        {
            return ActionValidationResult.Failure($"Unknown action '{command.ActionId}'.");
        }

        var errors = new List<string>();
        if (definition.AllowedSourceFamilies is not null &&
            !definition.AllowedSourceFamilies.Contains(command.SourceFamily))
        {
            errors.Add($"Action '{command.ActionId}' does not allow source family '{command.SourceFamily}'.");
        }

        var contract = definition.ParameterContract;
        var rules = definition.ValidationRules;
        if (contract is null || rules is null)
        {
            return new ActionValidationResult(errors.Count == 0, errors.AsReadOnly());
        }

        var declared = contract.Parameters.ToDictionary(parameter => parameter.Id, StringComparer.Ordinal);
        if (contract.Closed && rules.RejectUnknownParameters)
        {
            errors.AddRange(command.Parameters.Keys
                .Where(parameterId => !declared.ContainsKey(parameterId))
                .Order(StringComparer.Ordinal)
                .Select(parameterId => $"Action '{command.ActionId}' does not declare parameter '{parameterId}'."));
        }

        foreach (var parameter in contract.Parameters)
        {
            if (!command.Parameters.TryGetValue(parameter.Id, out var value))
            {
                if (parameter.Required && rules.RequireDeclaredRequiredParameters)
                {
                    errors.Add($"Action '{command.ActionId}' requires parameter '{parameter.Id}'.");
                }

                continue;
            }

            ValidateValue(command.ActionId, parameter, value.Value, errors, rules.EnforceDeclaredValueTypes);
        }

        return new ActionValidationResult(errors.Count == 0, errors.AsReadOnly());
    }

    private void ValidateValue(
        string actionId,
        ActionParameterDefinition definition,
        object? value,
        List<string> errors,
        bool enforceValueType)
    {
        if (enforceValueType && !MatchesType(definition.ValueType, value))
        {
            errors.Add($"Action '{actionId}' parameter '{definition.Id}' has invalid value type.");
            return;
        }

        if (value is int integer)
        {
            if (definition.Minimum is { } minimum && integer < minimum)
            {
                errors.Add($"Action '{actionId}' parameter '{definition.Id}' must be at least {minimum}.");
            }

            if (definition.Maximum is { } maximum && integer > maximum)
            {
                errors.Add($"Action '{actionId}' parameter '{definition.Id}' must be at most {maximum}.");
            }
        }

        var values = value switch
        {
            string text => [text],
            IReadOnlyCollection<string> strings => strings,
            _ => [],
        };
        if (definition.AllowedValues is not null)
        {
            errors.AddRange(values.Where(item => !definition.AllowedValues.Contains(item))
                .Order(StringComparer.Ordinal)
                .Select(item => $"Action '{actionId}' parameter '{definition.Id}' has invalid value '{item}'."));
        }

        if (definition.ValueType is ActionParameterValueType.Id or ActionParameterValueType.IdList &&
            _referenceResolver is not null)
        {
            errors.AddRange(values.Where(item => !_referenceResolver.Exists(definition.ReferenceDomain!, item))
                .Order(StringComparer.Ordinal)
                .Select(item => $"Action '{actionId}' parameter '{definition.Id}' references unknown '{item}'."));
        }

        if (value is IReadOnlyCollection<string> collection)
        {
            if (definition.MinItems is { } minItems && collection.Count < minItems)
            {
                errors.Add($"Action '{actionId}' parameter '{definition.Id}' must contain at least {minItems} item(s).");
            }

            if (definition.MaxItems is { } maxItems && collection.Count > maxItems)
            {
                errors.Add($"Action '{actionId}' parameter '{definition.Id}' must contain at most {maxItems} item(s).");
            }
        }
    }

    private static bool MatchesType(ActionParameterValueType type, object? value) => type switch
    {
        ActionParameterValueType.Integer => value is int,
        ActionParameterValueType.Boolean => value is bool,
        ActionParameterValueType.Id or ActionParameterValueType.Enum or ActionParameterValueType.Token => value is string,
        ActionParameterValueType.IdList or ActionParameterValueType.EnumList => value is IReadOnlyCollection<string>,
        ActionParameterValueType.StructuredAmount or ActionParameterValueType.Duration => value is string or int,
        _ => false,
    };
}

public interface IActionReferenceResolver
{
    bool Exists(string domain, string id);
}

public sealed record ActionValidationResult(bool IsValid, IReadOnlyList<string> Errors)
{
    public static ActionValidationResult Failure(string error) => new(false, [error]);
}