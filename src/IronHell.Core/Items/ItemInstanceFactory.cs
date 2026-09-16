using IronHell.Core.Definitions;

namespace IronHell.Core.Items;

/// <summary>Canonical creation point for ItemInstance: flavors are resolved from the world assignment, never rolled per instance.</summary>
public static class ItemInstanceFactory
{
    public static ItemInstance Create(string instanceId, ItemDefinition definition, IFlavorAssignmentService? flavorAssignments = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);
        ArgumentNullException.ThrowIfNull(definition);

        var flavorId = flavorAssignments is not null && flavorAssignments.TryGetFlavorForItemKind(definition.Id, out var flavor)
            ? flavor.Id
            : null;
        return new ItemInstance(instanceId, definition.Id, flavorId);
    }
}
