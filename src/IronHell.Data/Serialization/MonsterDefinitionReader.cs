using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Validation;

namespace IronHell.Data.Serialization;

internal static class MonsterDefinitionReader
{
    public static List<MonsterAbilityDefinition> ReadAbilities(JsonObject document, DefinitionValidationReport report) =>
        SimpleDefinitionReader.Read<MonsterAbilityDefinition>(document, "abilities", "id", id => new MonsterAbilityDefinition(id), report);

    public static List<MonsterDefinition> ReadMonsters(JsonObject document, DefinitionValidationReport report) =>
        SimpleDefinitionReader.Read<MonsterDefinition>(document, "monsters", "id", id => new MonsterDefinition(id), report);
}
