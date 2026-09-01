namespace IronHell.Core.Actions;

public sealed record HpChangedEvent(string CharacterId, int BeforeValue, int AfterValue) : IRuntimeEvent;