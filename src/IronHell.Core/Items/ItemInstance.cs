namespace IronHell.Core.Items;

public sealed record ItemInstance(string InstanceId, string DefinitionId, string? FlavorId = null);
