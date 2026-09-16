namespace IronHell.Core.Items;

/// <summary>Runtime-only per-world mapping of an item kind to the flavor it renders as. Never persisted here and never part of a definition.</summary>
public sealed record FlavorAssignment(string ItemDefinitionId, string FlavorDefinitionId);
