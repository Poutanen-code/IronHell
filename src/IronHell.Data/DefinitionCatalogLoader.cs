using IronHell.Core.Definitions;
using IronHell.Data.Loading;
using IronHell.Data.Validation;

namespace IronHell.Data;

public static class DefinitionCatalogLoader
{
    public static async Task<IDefinitionLoadResult> LoadAsync(string definitionsRoot, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definitionsRoot);

        var report = new DefinitionValidationReport();
        var documents = await DefinitionDocumentLoader.LoadAsync(
            definitionsRoot,
            DefinitionManifest.CharacterCreation,
            report,
            cancellationToken);

        if (report.HasErrors)
        {
            return new DefinitionLoadFailure(report.ToImmutable());
        }

        var catalog = DefinitionCatalogBuilder.Build(documents, report);
        return report.HasErrors
            ? new DefinitionLoadFailure(report.ToImmutable())
            : new DefinitionLoadSuccess(catalog!);
    }
}