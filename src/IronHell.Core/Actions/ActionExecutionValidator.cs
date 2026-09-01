using IronHell.Core.Definitions;

namespace IronHell.Core.Actions;

public static class ActionExecutionValidator
{
    public static string? Validate(ActionExecutionRequest request, ActionDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(definition);

        if (request.SourceType != ActionSourceType.DirectRuntime &&
            definition.AllowedSourceTypes is not null &&
            !definition.AllowedSourceTypes.Contains(request.SourceType))
        {
            return $"Action '{definition.Id}' does not allow source type '{request.SourceType}'.";
        }

        var targetMode = ReferenceEquals(request.SourceCharacter, request.Target.Character)
            ? ActionTargetMode.Self
            : ActionTargetMode.OtherCharacter;
        if (definition.AllowedTargetModes is not null &&
            !definition.AllowedTargetModes.Contains(targetMode))
        {
            return $"Action '{definition.Id}' does not allow target mode '{targetMode}'.";
        }

        return null;
    }
}