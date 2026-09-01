using IronHell.Core.Items;
using IronHell.Core.Definitions;

namespace IronHell.Core.Characters;

public sealed class Character
{
    public Character(string characterId, string raceId, string classId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId);
        ArgumentException.ThrowIfNullOrWhiteSpace(raceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(classId);

        CharacterId = characterId;
        RaceId = raceId;
        ClassId = classId;
    }

    public string CharacterId { get; }

    public string RaceId { get; }

    public string ClassId { get; }

    public Inventory Inventory { get; } = new();

    public Equipment Equipment { get; } = new();

    private readonly List<ActiveStatus> _activeStatuses = [];

    public IReadOnlyCollection<ActiveStatus> ActiveStatuses => _activeStatuses.AsReadOnly();

    public CharacterState State { get; } = new();

    public void Equip(IDefinitionCatalog catalog, EquipmentSlot slot, string instanceId)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var item = Inventory.GetRequired(instanceId);
        var replaced = Equipment.Equip(slot, item, catalog.Items.GetRequired(item.DefinitionId));
        Inventory.Remove(instanceId);
        if (replaced is not null)
        {
            Inventory.Add(replaced);
        }
    }

    public void Unequip(EquipmentSlot slot)
    {
        var item = Equipment.Unequip(slot);
        if (item is not null)
        {
            Inventory.Add(item);
        }
    }

    public void ApplyStatus(ActiveStatus status)
    {
        ArgumentNullException.ThrowIfNull(status);
        _activeStatuses.Add(status);
    }

    public int RemoveStatuses(IReadOnlyCollection<string> statusIds)
    {
        ArgumentNullException.ThrowIfNull(statusIds);
        return _activeStatuses.RemoveAll(status => statusIds.Contains(status.StatusId, StringComparer.Ordinal));
    }

    public bool RemoveStatus(ActiveStatus status)
    {
        ArgumentNullException.ThrowIfNull(status);
        return _activeStatuses.Remove(status);
    }
}