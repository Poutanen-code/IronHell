namespace IronHell.Core.Dungeon;

public enum DoorCondition
{
    Closed,
    Locked,
    Stuck,
}

public readonly record struct DoorState
{
    public DoorState(DoorCondition condition, int power)
    {
        if (condition is not (DoorCondition.Closed or DoorCondition.Locked or DoorCondition.Stuck))
        {
            throw new ArgumentOutOfRangeException(nameof(condition));
        }

        var validPower = condition switch
        {
            DoorCondition.Closed => power == 0,
            DoorCondition.Locked => power is >= 1 and <= 7,
            DoorCondition.Stuck => power is >= 0 and <= 7,
            _ => false,
        };

        if (!validPower)
        {
            throw new ArgumentOutOfRangeException(nameof(power));
        }

        Condition = condition;
        Power = power;
    }

    public DoorCondition Condition { get; }

    public int Power { get; }
}