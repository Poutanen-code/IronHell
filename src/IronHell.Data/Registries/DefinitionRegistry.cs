using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using IronHell.Core.Definitions;

namespace IronHell.Data.Registries;

internal sealed class DefinitionRegistry<T> : IDefinitionRegistry<T>
    where T : IIdentifiedDefinition
{
    private readonly FrozenDictionary<string, T> _byId;
    private readonly IReadOnlyCollection<T> _all;

    public DefinitionRegistry(IEnumerable<T> definitions)
    {
        var ordered = definitions.OrderBy(definition => definition.Id, StringComparer.Ordinal).ToArray();
        _byId = ordered.ToFrozenDictionary(definition => definition.Id, StringComparer.Ordinal);
        _all = Array.AsReadOnly(ordered);
    }

    public IReadOnlyCollection<T> All => _all;

    public bool TryGet(string id, [NotNullWhen(true)] out T? definition) => _byId.TryGetValue(id, out definition);

    public T GetRequired(string id) => TryGet(id, out var definition)
        ? definition
        : throw new KeyNotFoundException($"Definition '{id}' was not found in {typeof(T).Name} registry.");
}