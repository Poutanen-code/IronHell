using IronHell.Core.Definitions;
using IronHell.Core.Items;
using IronHell.Core.Randomness;

namespace IronHell.Core.World;

/// <summary>
/// Owns runtime state shared by every character in a single world/save (never per-character,
/// never per-instance). Currently owns flavor-to-item-kind assignment; future world-scoped
/// state (e.g. scroll title assignments) belongs here too. Reconstructing a WorldRuntimeState
/// from the same catalog and FlavorSeed always reproduces the same assignments.
/// </summary>
public sealed class WorldRuntimeState
{
    public WorldRuntimeState(IDefinitionCatalog catalog, FlavorSeed flavorSeed)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        FlavorSeed = flavorSeed;
        FlavorAssignments = new FlavorAssignmentService(catalog.Items, catalog.Flavors, flavorSeed);
    }

    public FlavorSeed FlavorSeed { get; }

    public IFlavorAssignmentService FlavorAssignments { get; }
}
