namespace IronHell.Core.Actions;

public interface IActionExecutor
{
    ActionExecutionResult Execute(ActionExecutionContext context, IReadOnlyCollection<ActionInvocation> actions);

    ActionExecutionResult Execute(ActionExecutionContext context, ActionExecutionRequest request);
}