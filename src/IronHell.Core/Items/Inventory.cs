namespace IronHell.Core.Items;

public sealed class Inventory
{
    private readonly List<ItemInstance> _items = [];

    public IReadOnlyCollection<ItemInstance> Items => _items.AsReadOnly();

    public void Add(ItemInstance item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (_items.Any(existing => string.Equals(existing.InstanceId, item.InstanceId, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException($"Item instance '{item.InstanceId}' is already in the inventory.");
        }

        _items.Add(item);
    }

    public bool Remove(string instanceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);
        var item = _items.FirstOrDefault(candidate => string.Equals(candidate.InstanceId, instanceId, StringComparison.Ordinal));
        return item is not null && _items.Remove(item);
    }

    public ItemInstance GetRequired(string instanceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);
        return _items.FirstOrDefault(candidate => string.Equals(candidate.InstanceId, instanceId, StringComparison.Ordinal))
            ?? throw new KeyNotFoundException($"Item instance '{instanceId}' was not found in the inventory.");
    }
}