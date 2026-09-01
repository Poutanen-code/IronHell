namespace IronHell.Core.Actions;

public sealed record StatusRemovedEvent(string CharacterId, string StatusId) : IRuntimeEvent;