using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Validation;

namespace IronHell.Data.Serialization;

internal static class MonsterDefinitionReader
{
    public static List<MonsterAbilityDefinition> ReadAbilities(JsonObject document, DefinitionValidationReport report) =>
        SimpleDefinitionReader.Read<MonsterAbilityDefinition>(document, "abilities", "id", id => new MonsterAbilityDefinition(id), report);

    public static List<MonsterDefinition> ReadMonsters(JsonObject document, DefinitionValidationReport report)
    {
        var definitions = new List<MonsterDefinition>();
        foreach (var monster in document["monsters"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var id = monster["id"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(id))
            {
                report.Add("monsters/monsters.json", null, "$.monsters", "missing_id", "Definition identifier is required.");
                continue;
            }

            var hpRoll = monster["hp_roll"]?.AsObject();
            definitions.Add(new MonsterDefinition(id, new DiceRollDefinition(
                hpRoll?["kind"]?.GetValue<string>() ?? string.Empty,
                hpRoll?["count"]?.GetValue<int>() ?? 0,
                hpRoll?["sides"]?.GetValue<int>() ?? 0)));
        }

        ValidationHelpers.ValidateDuplicates("monsters", definitions, report);
        return definitions;
    }
}
