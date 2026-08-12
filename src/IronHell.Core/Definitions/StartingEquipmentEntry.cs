namespace IronHell.Core.Definitions;

/// <summary>One entry in a class's starting_equipment list. Min and max are inclusive stack counts.</summary>
public sealed record StartingEquipmentEntry(string Id, int Min, int Max);
