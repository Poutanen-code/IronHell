using System.Collections.Frozen;
using IronHell.Data.Validation;

namespace IronHell.Data.Visuals;

public sealed class TerrainVisualCatalog
{
    private readonly FrozenDictionary<string, string> _spritePaths;

    internal TerrainVisualCatalog(Dictionary<string, string> spritePaths)
    {
        _spritePaths = spritePaths.ToFrozenDictionary(StringComparer.Ordinal);
    }

    public IReadOnlyDictionary<string, string> SpritePaths => _spritePaths;
}

public sealed record TerrainVisualCatalogLoadResult(
    TerrainVisualCatalog? Catalog,
    DefinitionValidationReportSnapshot Report)
{
    public bool Succeeded => Catalog is not null && Report.Errors.IsEmpty;
}