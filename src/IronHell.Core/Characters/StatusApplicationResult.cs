namespace IronHell.Core.Characters;

public sealed record StatusApplicationResult(
    bool Applied,
    bool Replaced,
    ActiveStatus ActiveStatus);