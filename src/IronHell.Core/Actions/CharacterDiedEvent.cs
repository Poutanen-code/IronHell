namespace IronHell.Core.Actions;

public sealed record CharacterDiedEvent(string CharacterId) : IRuntimeEvent;