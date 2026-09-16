namespace IronHell.Core.Monsters;

public sealed class Monster
{
    internal Monster(string instanceId, string definitionId, int maxHp)
    {
        InstanceId = instanceId;
        DefinitionId = definitionId;
        MaxHp = maxHp;
        CurrentHp = maxHp;
    }

    public string InstanceId { get; }

    public string DefinitionId { get; }

    public int MaxHp { get; }

    public int CurrentHp { get; set; }
}