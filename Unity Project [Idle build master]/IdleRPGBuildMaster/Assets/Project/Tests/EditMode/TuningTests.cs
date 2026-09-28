using System;
using System.Collections.Generic;
using IBM.Authoring;
using IBM.Domain;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace IBM.Tests.EditMode
{
    public sealed class TuningTests
    {
        [Test]
        public void BalanceAssetCompilesToEngineFreeCatalogAndRejectsDuplicates()
        {
            var asset = ScriptableObject.CreateInstance<BalanceTuningAsset>();
            try
            {
                var serialized = new SerializedObject(asset);
                var entries = serialized.FindProperty("parameters");
                entries.arraySize = 1;
                SetBalance(entries.GetArrayElementAtIndex(0), "test.exp_budget", BalanceValueKind.Number, "1", "1000");
                serialized.ApplyModifiedPropertiesWithoutUndo();
                var catalog = asset.Compile();
                Assert.That(catalog.Require(new ContentId("test.exp_budget"), BalanceValueKind.Number).Number,
                    Is.EqualTo(GameNumber.Parse("1", "1000")));
                Assert.That(catalog.MechanicalHash, Has.Length.EqualTo(64));
                Assert.Throws<InvalidOperationException>(() => catalog.Require(new ContentId("test.exp_budget"), BalanceValueKind.Integer));

                entries.arraySize = 2;
                SetBalance(entries.GetArrayElementAtIndex(1), "test.exp_budget", BalanceValueKind.Number, "2", "0");
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assert.Throws<ArgumentException>(() => asset.Compile());
            }
            finally { UnityEngine.Object.DestroyImmediate(asset); }
        }

        [Test]
        public void BalanceAssetRejectsInvalidProbabilityAndMissingKeys()
        {
            var asset = ScriptableObject.CreateInstance<BalanceTuningAsset>();
            try
            {
                Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => asset.Compile().Require(new ContentId("missing"), BalanceValueKind.Number));
                var serialized = new SerializedObject(asset);
                var entries = serialized.FindProperty("parameters");
                entries.arraySize = 1;
                SetBalance(entries.GetArrayElementAtIndex(0), "test.drop_chance", BalanceValueKind.Probability, "11", "-1");
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assert.Throws<InvalidOperationException>(() => asset.Compile());
            }
            finally { UnityEngine.Object.DestroyImmediate(asset); }
        }

        [Test]
        public void BalanceHashIgnoresAuthoringOrderAndChangesWithValues()
        {
            var a = new KeyValuePair<ContentId, BalanceValue>(new ContentId("a"), new BalanceValue(BalanceValueKind.Number, GameNumber.One));
            var b = new KeyValuePair<ContentId, BalanceValue>(new ContentId("b"), new BalanceValue(BalanceValueKind.Number, GameNumber.FromInt64(2)));
            var first = new BalanceCatalog("r1", new[] { a, b });
            var reordered = new BalanceCatalog("r2", new[] { b, a });
            Assert.That(first.MechanicalHash, Is.EqualTo(reordered.MechanicalHash));
            var changed = new BalanceCatalog("r1", new[] { a, new KeyValuePair<ContentId, BalanceValue>(b.Key, new BalanceValue(BalanceValueKind.Number, GameNumber.FromInt64(3))) });
            Assert.That(changed.MechanicalHash, Is.Not.EqualTo(first.MechanicalHash));
        }

        [Test]
        public void InterfaceAssetRejectsUnconfiguredAndInvalidMeasures()
        {
            var asset = ScriptableObject.CreateInstance<InterfaceTuningAsset>();
            try
            {
                Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => asset.RequireMeasure("combat.hp_bar.width", InterfaceMeasureKind.Pixels));
                var serialized = new SerializedObject(asset);
                var measures = serialized.FindProperty("measures");
                measures.arraySize = 1;
                var width = measures.GetArrayElementAtIndex(0);
                width.FindPropertyRelative("id").stringValue = "combat.hp_bar.width";
                width.FindPropertyRelative("kind").enumValueIndex = (int)InterfaceMeasureKind.Pixels;
                width.FindPropertyRelative("value").floatValue = 180f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assert.That(asset.RequireMeasure("combat.hp_bar.width", InterfaceMeasureKind.Pixels), Is.EqualTo(180f));
                width.FindPropertyRelative("value").floatValue = -1f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assert.Throws<InvalidOperationException>(() => asset.Validate());
            }
            finally { UnityEngine.Object.DestroyImmediate(asset); }
        }

        private static void SetBalance(SerializedProperty property, string id, BalanceValueKind kind, string coefficient, string exponent)
        {
            property.FindPropertyRelative("id").stringValue = id;
            property.FindPropertyRelative("kind").enumValueIndex = (int)kind;
            property.FindPropertyRelative("coefficient").stringValue = coefficient;
            property.FindPropertyRelative("exponent").stringValue = exponent;
        }
    }
}
