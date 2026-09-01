using System.Collections.ObjectModel;
using IronHell.Core.Definitions;

namespace IronHell.Core.Items;

public sealed class Equipment
{
    private readonly Dictionary<EquipmentSlot, ItemInstance> _items = [];

    public IReadOnlyCollection<ItemInstance> Items => new ReadOnlyCollection<ItemInstance>(_items.OrderBy(pair => pair.Key).Select(pair => pair.Value).ToList());

    public ItemInstance? Equip(EquipmentSlot slot, ItemInstance item, ItemDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(definition);
        if (!string.Equals(item.DefinitionId, definition.Id, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Item instance and definition IDs do not match.");
        }

        if (GetSlot(definition) != slot)
        {
            throw new InvalidOperationException($"Item definition '{definition.Id}' cannot be equipped in slot '{slot}'.");
        }

        if (_items.Values.Any(existing => string.Equals(existing.InstanceId, item.InstanceId, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException($"Item instance '{item.InstanceId}' is already equipped.");
        }

        _items.Remove(slot, out var replaced);
        _items.Add(slot, item);
        return replaced;
    }

    public ItemInstance? Unequip(EquipmentSlot slot)
    {
        return _items.Remove(slot, out var item) ? item : null;
    }

    public bool IsEquipped(EquipmentSlot slot) => _items.ContainsKey(slot);

    public ItemInstance? GetEquipped(EquipmentSlot slot) => _items.GetValueOrDefault(slot);

    private static EquipmentSlot GetSlot(ItemDefinition definition) => definition.Category switch
    {
        ItemCategory.Weapon when definition.Type is "bow" or "crossbow" => EquipmentSlot.Bow,
        ItemCategory.Weapon => EquipmentSlot.Weapon,
        ItemCategory.Light => EquipmentSlot.Light,
        ItemCategory.Armor when definition.Type == "chest" => EquipmentSlot.Body,
        ItemCategory.Armor when definition.Type is "helmet" or "crown" => EquipmentSlot.Head,
        ItemCategory.Armor when definition.Type == "cloak" => EquipmentSlot.Cloak,
        ItemCategory.Armor when definition.Type == "shield" => EquipmentSlot.Shield,
        ItemCategory.Armor when definition.Type == "gloves" => EquipmentSlot.Gloves,
        ItemCategory.Armor when definition.Type == "boots" => EquipmentSlot.Boots,
        _ => throw new InvalidOperationException($"Item definition '{definition.Id}' has no supported equipment slot."),
    };
}