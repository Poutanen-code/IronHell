using IronHell.Core.Characters;
using IronHell.Core.Definitions;

namespace IronHell.Core.Actions;

public sealed record ActionExecutionContext(
    IDefinitionCatalog Catalog,
    Character Source,
    ActionTarget Target,
    ActionSourceType SourceType = ActionSourceType.DirectRuntime)
{
    public ActionExecutionContext(IDefinitionCatalog catalog, Character source, Character target)
        : this(catalog, source, ActionTarget.CharacterTarget(target))
    {
    }
}