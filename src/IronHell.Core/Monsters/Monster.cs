using IronHell.Core.Definitions;

namespace IronHell.Core.Monsters;

public sealed class Monster
{
    internal Monster(
        string instanceId,
        string definitionId,
        int maxHp,
        MonsterAiDefinition ai,
        IReadOnlyList<string> capabilities)
    {
        InstanceId = instanceId;
        DefinitionId = definitionId;
        MaxHp = maxHp;
        CurrentHp = maxHp;
        Ai = ai;
        Capabilities = capabilities;
    }

    public string InstanceId { get; }

    public string DefinitionId { get; }

    public int MaxHp { get; }

    public int CurrentHp { get; set; }

    public MonsterAiDefinition Ai { get; }

    public IReadOnlyList<string> Capabilities { get; }
}