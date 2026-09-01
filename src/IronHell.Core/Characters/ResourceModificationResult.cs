namespace IronHell.Core.Characters;

public sealed record ResourceModificationResult(
    CharacterResourceType ResourceType,
    int BeforeValue,
    int AfterValue,
    bool Success,
    string? Failure);