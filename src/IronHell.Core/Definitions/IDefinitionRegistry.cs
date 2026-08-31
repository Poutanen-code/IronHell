using System.Diagnostics.CodeAnalysis;

namespace IronHell.Core.Definitions;

public interface IDefinitionRegistry<T>
    where T : IIdentifiedDefinition
{
    bool TryGet(string id, [NotNullWhen(true)] out T? definition);

    T GetRequired(string id);

    IReadOnlyCollection<T> All { get; }
}