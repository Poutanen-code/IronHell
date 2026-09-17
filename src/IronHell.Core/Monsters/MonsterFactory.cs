using IronHell.Core.Definitions;
using IronHell.Core.Randomness;

namespace IronHell.Core.Monsters;

public sealed class MonsterFactory
{
    private readonly IRandomSource _randomSource;

    public MonsterFactory(IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(randomSource);
        _randomSource = randomSource;
    }

    public Monster Create(string instanceId, MonsterDefinition definition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);
        ArgumentNullException.ThrowIfNull(definition);

        var hpRoll = definition.HpRoll;
        if (hpRoll.Kind != "dice" || hpRoll.Count <= 0 || hpRoll.Sides <= 0)
        {
            throw new ArgumentException("Monster HP roll must use positive dice count and sides.", nameof(definition));
        }

        var maxHp = _randomSource.RollDice(hpRoll.Count, hpRoll.Sides);
        return new Monster(
            instanceId,
            definition.Id,
            maxHp,
            definition.Ai,
            Array.AsReadOnly(definition.Capabilities.ToArray()),
            Array.AsReadOnly(definition.Resistances.ToArray()),
            definition.Senses);
    }
}