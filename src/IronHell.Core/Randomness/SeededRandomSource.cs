namespace IronHell.Core.Randomness;

public sealed class SeededRandomSource : IRandomSource
{
    private uint _state;

    public SeededRandomSource(int seed)
    {
        _state = unchecked((uint)seed);
        if (_state == 0)
        {
            _state = 0x6D2B79F5;
        }
    }

    public int Next(int minInclusive, int maxExclusive)
    {
        if (minInclusive >= maxExclusive)
        {
            throw new ArgumentOutOfRangeException(nameof(maxExclusive));
        }

        var range = (uint)(maxExclusive - minInclusive);
        return minInclusive + (int)(NextUInt32() % range);
    }

    public int RollDice(int count, int sides)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        if (sides < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(sides));
        }

        var total = 0;
        for (var index = 0; index < count; index++)
        {
            total += Next(1, sides + 1);
        }

        return total;
    }

    private uint NextUInt32()
    {
        var state = _state;
        state ^= state << 13;
        state ^= state >> 17;
        state ^= state << 5;
        _state = state;
        return state;
    }
}