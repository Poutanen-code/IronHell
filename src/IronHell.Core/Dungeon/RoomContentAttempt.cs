namespace IronHell.Core.Dungeon;

public enum RoomContentAttemptKind
{
    SecretDoor,
    LockedDoor,
    Monster,
    Trap,
    Object,
    ObjectOrGold,
    SpecialObject,
    RandomStair,
}

public sealed record RoomContentAttempt(
    RoomContentAttemptKind Kind,
    DungeonPosition Origin,
    int RadiusRows = 0,
    int RadiusColumns = 0,
    int GenerationDepthOffset = 0,
    bool Special = false);
