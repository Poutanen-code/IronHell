using IronHell.Core.Characters;
using IronHell.Core.Definitions;

namespace IronHell.Core.Actions;

public sealed record ActionExecutionContext(
    IDefinitionCatalog Catalog,
    Character Source,
    Character Target);