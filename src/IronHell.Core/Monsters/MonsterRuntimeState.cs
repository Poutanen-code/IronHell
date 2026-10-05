using IronHell.Core.Definitions;

namespace IronHell.Core.Monsters;

public sealed record MonsterRuntimeInstance(
    string InstanceId,
    string DefinitionId,
    MonsterPosition Position,
    MonsterSpawnState SpawnState);

public sealed record MonsterSpawnState(
    int MaxHp,
    int CurrentHp,
    int MovementSpeed,
    int Energy);

public sealed class MonsterRuntimeState
{
    private readonly Dictionary<string, MonsterRuntimeInstance> _monstersById = new(StringComparer.Ordinal);
    private readonly Dictionary<MonsterPosition, string> _monsterIdByPosition = [];
    private int _nextInstanceNumber = 1;

    public IReadOnlyCollection<MonsterRuntimeInstance> Monsters => _monstersById.Values.ToArray();

    public bool Contains(MonsterRuntimeInstance monster) =>
        monster is not null && _monstersById.TryGetValue(monster.InstanceId, out var existing) && existing == monster;

    public bool IsOccupied(MonsterPosition position) => _monsterIdByPosition.ContainsKey(position);

    public bool HasUniqueCapacity(MonsterDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return !definition.SpawnPolicy.Unique ||
            !_monstersById.Values.Any(monster => monster.DefinitionId == definition.Id);
    }

    internal MonsterRuntimeInstance Register(
        string definitionId,
        MonsterPosition position,
        MonsterSpawnState spawnState)
    {
        var instanceId = $"monster-{_nextInstanceNumber}";
        _nextInstanceNumber++;
        var monster = new MonsterRuntimeInstance(instanceId, definitionId, position, spawnState);
        _monstersById.Add(instanceId, monster);
        _monsterIdByPosition.Add(position, instanceId);
        return monster;
    }
}
