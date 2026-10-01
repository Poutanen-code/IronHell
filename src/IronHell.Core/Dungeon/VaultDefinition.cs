using IronHell.Core.Definitions;

namespace IronHell.Core.Dungeon;

public sealed record VaultDefinition(
    string Id,
    int SourceType,
    int Rating,
    IReadOnlyList<string> Layout) : IIdentifiedDefinition
{
    public int EffectiveType => SourceType == 9 ? 8 : SourceType;

    public RoomFamily Family => EffectiveType switch
    {
        7 => RoomFamily.LesserVault,
        8 => RoomFamily.GreaterVault,
        _ => throw new InvalidOperationException($"Unsupported vault type {SourceType}.")
    };

    public int Rows => Layout.Count;

    public int Columns => Layout.Count == 0 ? 0 : Layout[0].Length;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new InvalidOperationException("Vault id is required.");
        }

        if (SourceType is not (7 or 8 or 9))
        {
            throw new InvalidOperationException($"Vault '{Id}' has unsupported type {SourceType}.");
        }

        var metadata = RoomFamilies.Get(Family);
        if (Rows == 0 || Columns == 0 || Layout.Any(row => row.Length != Columns))
        {
            throw new InvalidOperationException($"Vault '{Id}' has invalid layout dimensions.");
        }

        var maxRows = metadata.Footprint.HeightBlocks * DungeonGrid.BlockHeight;
        var maxColumns = metadata.Footprint.WidthBlocks * DungeonGrid.BlockWidth;
        if (Rows > maxRows || Columns > maxColumns)
        {
            throw new InvalidOperationException($"Vault '{Id}' does not fit its {metadata.Family} footprint.");
        }
    }
}