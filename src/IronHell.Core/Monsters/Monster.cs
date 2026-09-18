using IronHell.Core.Definitions;

namespace IronHell.Core.Monsters;

public sealed class Monster
{
    internal Monster(
        string instanceId,
        MonsterDefinition definition,
        int maxHp)
    {
        InstanceId = instanceId;
        DefinitionId = definition.Id;
        MaxHp = maxHp;
        CurrentHp = maxHp;
        Ai = definition.Ai;
        Capabilities = Array.AsReadOnly(definition.Capabilities.ToArray());
        Resistances = Array.AsReadOnly(definition.Resistances.ToArray());
        Senses = definition.Senses;
        SpawnPolicy = definition.SpawnPolicy;
        LootProfileId = definition.LootProfileId;
    }

    public string InstanceId { get; }

    public string DefinitionId { get; }

    public int MaxHp { get; }

    public int CurrentHp { get; set; }

    public MonsterAiDefinition Ai { get; }

    public IReadOnlyList<string> Capabilities { get; }

    public IReadOnlyList<string> Resistances { get; }

    public MonsterSensesDefinition Senses { get; }

    public SpawnPolicy SpawnPolicy { get; }

    public string? LootProfileId { get; }
}