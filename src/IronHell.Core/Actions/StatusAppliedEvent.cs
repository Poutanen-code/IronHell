namespace IronHell.Core.Actions;

public sealed record StatusAppliedEvent(string CharacterId, string StatusId, int RemainingDuration) : IRuntimeEvent;