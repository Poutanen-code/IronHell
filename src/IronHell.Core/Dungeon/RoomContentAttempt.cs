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

public enum RoomMonsterAttemptSource
{
    Unspecified,
    PreparedNestPit,
    OrdinaryRoomVaultMonsters,
    VaultGlyph,
}

public sealed record RoomContentAttempt(
    RoomContentAttemptKind Kind,
    DungeonPosition Origin,
    int RadiusRows = 0,
    int RadiusColumns = 0,
    int GenerationDepthOffset = 0,
    bool Special = false,
    string? DefinitionId = null,
    bool AllowGroupExpansion = true,
    DoorState? PreparedDoorState = null,
    string? PreparedStairFeatureId = null,
    RoomMonsterAttemptSource MonsterSource = RoomMonsterAttemptSource.Unspecified,
    bool SleepOnSpawn = false);
