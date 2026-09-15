using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Serialization;
using IronHell.Data.Validation;
using Xunit;

namespace IronHell.Data.Tests;

public sealed class ActionDefinitionProjectionTests
{
    [Fact]
    public void Read_PreservesActionMetadataAndParameterContract()
    {
        var document = JsonNode.Parse("""
        {
          "schema_version": 1,
          "actions": [
            {
              "action_id": "TestAction",
              "name": "Test Action",
              "description": "A test action.",
              "category": "damage",
              "confidence": "high",
              "allowed_source_families": ["spell", "wand"],
              "parameter_contract": {
                "closed": true,
                "parameters": [
                  {
                    "id": "mode",
                    "description": "Mode.",
                    "value_type": "enum",
                    "required": true,
                    "allowed_values": ["one", "two"]
                  }
                ]
              },
              "validation": {
                "reject_unknown_parameters": true,
                "require_declared_required_parameters": true,
                "enforce_declared_value_types": true
              },
              "provenance_status": "verified",
              "provenance": { "summary": "Test provenance." }
            }
          ]
        }
        """)!.AsObject();

        var report = new DefinitionValidationReport();
        var action = Assert.Single(ActionDefinitionReader.Read(document, report));

        Assert.Equal("TestAction", action.Id);
        Assert.Equal("Test Action", action.Name);
        Assert.Equal(ActionCategory.Damage, action.Category);
        Assert.Equal(ActionConfidence.High, action.Confidence);
        Assert.Equal([ActionSourceFamily.Spell, ActionSourceFamily.Wand], action.AllowedSourceFamilies!.OrderBy(value => value));
        var parameter = Assert.Single(action.ParameterContract!.Parameters);
        Assert.Equal(["one", "two"], parameter.AllowedValues!.OrderBy(value => value));
        Assert.True(action.ValidationRules!.EnforceDeclaredValueTypes);
        Assert.Equal("verified", action.ProvenanceStatus);
        Assert.Equal("Test provenance.", action.Provenance!.Summary);
        Assert.False(report.HasErrors);
    }
}