using System.Collections.Frozen;
using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Registries;

namespace IronHell.Data.Validation;

internal static class SpellValidator
{
    private const string MagicRealm = "magic";

    public static void Validate(
        FrozenDictionary<string, JsonObject> documents,
        ValidationRegistries registries,
        DefinitionValidationReport report)
    {
        ValidateSpellBookReferences(documents["spell_books"], registries.MageSpells, registries.PriestPrayers, report);
        ValidateSpellDefinitions(documents["mage_spells"], MagicRealm, "magic/mage_spells.json", registries, report);
        ValidateSpellDefinitions(documents["priest_prayers"], "prayer", "magic/priest_prayers.json", registries, report);
    }

    private static void ValidateSpellBookReferences(
        JsonObject document,
        IDefinitionRegistry<SpellDefinition> mageSpells,
        IDefinitionRegistry<SpellDefinition> priestPrayers,
        DefinitionValidationReport report)
    {
        foreach (var book in document["books"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var bookId = book["id"]?.GetValue<string>() ?? string.Empty;
            var spells = book["realm"]?.GetValue<string>() == "magic" ? mageSpells : priestPrayers;
            foreach (var spellId in book["spell_ids"]?.AsArray() ?? [])
            {
                ValidationHelpers.ValidateReference(spellId?.GetValue<string>(), spells, "magic/spell_books.json", bookId, "spell_ids", "unknown_spell", report);
            }
        }
    }

    private static void ValidateSpellDefinitions(
        JsonObject document,
        string expectedRealm,
        string documentPath,
        ValidationRegistries registries,
        DefinitionValidationReport report)
    {
        foreach (var spell in document["spells"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var spellId = spell["id"]?.GetValue<string>() ?? string.Empty;
            var policy = spell["policy"]?.AsObject();
            ValidateSpellPolicy(policy, expectedRealm, documentPath, spellId, registries.Items, report);

            foreach (var action in spell["action_refs"]?.AsArray().OfType<JsonObject>() ?? [])
            {
                ValidationHelpers.ValidateActionReference(action, registries.Actions, registries.Statuses, documentPath, spellId, report);
            }
        }
    }

    private static void ValidateSpellPolicy(
        JsonObject? policy,
        string expectedRealm,
        string documentPath,
        string spellId,
        IDefinitionRegistry<ItemDefinition> items,
        DefinitionValidationReport report)
    {
        if (policy?["realm"]?.GetValue<string>() != expectedRealm)
        {
            report.Add(documentPath, spellId, "policy.realm", "invalid_spell_realm", "Spell policy realm does not match its catalog.");
        }

        var bookId = policy?["book_id"]?.GetValue<string>();
        if (!items.TryGet(bookId ?? string.Empty, out var book) || book.Category != ItemCategory.SpellBook)
        {
            report.Add(documentPath, spellId, "policy.book_id", "unknown_spell_book", $"Spell book '{bookId}' does not resolve to a spell book item.");
        }
    }
}
