using System.Collections.Immutable;

namespace IronHell.Data.Validation;

public sealed class DefinitionValidationReport
{
    private readonly List<DefinitionValidationError> _errors = [];

    public bool HasErrors => _errors.Count > 0;

    public void Add(string documentPath, string? definitionId, string fieldPath, string code, string message)
    {
        _errors.Add(new DefinitionValidationError(documentPath, definitionId, fieldPath, code, message));
    }

    public DefinitionValidationReportSnapshot ToImmutable() => new(
        _errors
            .OrderBy(error => error.DocumentPath, StringComparer.Ordinal)
            .ThenBy(error => error.DefinitionId, StringComparer.Ordinal)
            .ThenBy(error => error.FieldPath, StringComparer.Ordinal)
            .ThenBy(error => error.Code, StringComparer.Ordinal)
            .ToImmutableArray());
}

public sealed record DefinitionValidationReportSnapshot(ImmutableArray<DefinitionValidationError> Errors);

public sealed record DefinitionValidationError(
    string DocumentPath,
    string? DefinitionId,
    string FieldPath,
    string Code,
    string Message);