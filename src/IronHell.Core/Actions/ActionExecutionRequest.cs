using IronHell.Core.Characters;
using IronHell.Core.Definitions;

namespace IronHell.Core.Actions;

public sealed record ActionExecutionRequest(
    ActionSourceType SourceType,
    Character SourceCharacter,
    ActionTarget Target,
    ActionInvocation Action);