using IronHell.Core.Definitions;
using IronHell.Data.Validation;

namespace IronHell.Data;

public interface IDefinitionLoadResult;

public sealed record DefinitionLoadSuccess(
	IDefinitionCatalog Catalog,
	DefinitionValidationReportSnapshot Report) : IDefinitionLoadResult;

public sealed record DefinitionLoadFailure(DefinitionValidationReportSnapshot Report) : IDefinitionLoadResult;