using System;
using System.Collections.Generic;
using IBM.Domain;
using UnityEngine;

namespace IBM.Authoring
{
    [Serializable]
    public sealed class AuthoredNumber
    {
        public string coefficient = "0";
        public string exponent = "0";
        public GameNumber Compile() => GameNumber.Parse(coefficient, exponent);
    }

    [Serializable] public sealed class ProgressMilestoneRecord
    {
        public string id;
        public int point;
        public ProgressMilestoneKind kind;
        public string rewardId;
        public bool firstRunOnly;
        public string[] fixedOfferOptionIds = Array.Empty<string>();
    }
    [Serializable] public sealed class LocationRecord
    {
        public string id;
        public string[] stageIds = Array.Empty<string>();
        public int progressUnits = 100;
        public int progressStageCount = 10;
        public ProgressMilestoneRecord[] milestones = Array.Empty<ProgressMilestoneRecord>();
    }
    [Serializable] public sealed class StageRecord { public string id; public string locationId; public string[] encounterIds = Array.Empty<string>(); public int baseRequiredEncounters; public bool isBossStage; }
    [Serializable] public sealed class EnemyRecord
    {
        public string id;
        public AuthoredNumber maxHp = new AuthoredNumber();
        public AuthoredNumber minDamage = new AuthoredNumber();
        public AuthoredNumber maxDamage = new AuthoredNumber();
        public double attackIntervalSeconds;
        public string[] tags = Array.Empty<string>();
        public string behaviorId;
    }
    [Serializable] public sealed class EncounterRecord
    {
        public string id;
        public string[] enemyIds = Array.Empty<string>();
        public AuthoredNumber expBudget = new AuthoredNumber();
        public string lootTableId;
    }
    [Serializable] public sealed class EffectRecord
    {
        public string id;
        public EffectTrigger trigger;
        public EffectOperation operation;
        public AuthoredNumber magnitude = new AuthoredNumber();
        [Range(0f, 1f)] public double probability = 1d;
        public string[] childEffectIds = Array.Empty<string>();
    }
    [Serializable] public sealed class PowerRecord { public string id; public string[] effectIds = Array.Empty<string>(); }
    [Serializable] public sealed class ItemRecord { public string id; public string slot; public string[] effectIds = Array.Empty<string>(); }
    [Serializable] public sealed class LootEntryRecord { public string itemId; public double weight; }
    [Serializable] public sealed class LootTableRecord { public string id; public LootEntryRecord[] entries = Array.Empty<LootEntryRecord>(); }

    // Unity authoring data never enters the simulation. Compile and validate before session creation.
    [CreateAssetMenu(fileName = "GameContent", menuName = "Idle Build Master/Content/Game Catalog")]
    public sealed class GameContentAsset : ScriptableObject
    {
        public int schemaVersion = 1;
        public string contentVersion = "content-v1";
        public string startingStageId;
        public LocationRecord[] locations = Array.Empty<LocationRecord>();
        public StageRecord[] stages = Array.Empty<StageRecord>();
        public EncounterRecord[] encounters = Array.Empty<EncounterRecord>();
        public EnemyRecord[] enemies = Array.Empty<EnemyRecord>();
        public EffectRecord[] effects = Array.Empty<EffectRecord>();
        public PowerRecord[] powers = Array.Empty<PowerRecord>();
        public ItemRecord[] items = Array.Empty<ItemRecord>();
        public LootTableRecord[] lootTables = Array.Empty<LootTableRecord>();

        public ContentCatalog Compile(ISet<EffectOperation> supportedOperations = null, BalanceCatalog balance = null)
        {
            if (schemaVersion != 1 || string.IsNullOrWhiteSpace(contentVersion))
                throw new InvalidOperationException(SourceLabel + ": unsupported content schema or version.");
            if (effects == null) throw new InvalidOperationException(SourceLabel + ": effect array is null.");
            for (int i = 0; i < effects.Length; i++)
                if (effects[i] != null && (supportedOperations == null || !supportedOperations.Contains(effects[i].operation)))
                    throw new InvalidOperationException(SourceLabel + ": unsupported effect operation at effects[" + i + "]: " + effects[i].operation);
            var locationDefs = Convert(locations, "location", x => new LocationDefinition(Id(x.id), Ids(x.stageIds),
                x.progressUnits, Convert(x.milestones, "milestone", m => new ProgressMilestone(Id(m.id), m.point,
                    m.kind, OptionalId(m.rewardId), m.firstRunOnly, Ids(m.fixedOfferOptionIds))), x.progressStageCount));
            var stageDefs = Convert(stages, "stage", x => new StageDefinition(Id(x.id), Id(x.locationId), Ids(x.encounterIds), x.baseRequiredEncounters, x.isBossStage));
            var encounterDefs = Convert(encounters, "encounter", x => new EncounterDefinition(Id(x.id), Ids(x.enemyIds), x.expBudget.Compile(), OptionalId(x.lootTableId)));
            var enemyDefs = Convert(enemies, "enemy", x => new EnemyDefinition(Id(x.id), x.maxHp.Compile(), x.minDamage.Compile(), x.maxDamage.Compile(), SimDuration.FromSeconds(x.attackIntervalSeconds), x.tags, OptionalId(x.behaviorId)));
            var effectDefs = Convert(effects, "effect", x => new EffectDefinition(Id(x.id), x.trigger, x.operation, x.magnitude.Compile(), x.probability, Ids(x.childEffectIds)));
            var powerDefs = Convert(powers, "power", x => new PowerDefinition(Id(x.id), Ids(x.effectIds)));
            var itemDefs = Convert(items, "item", x => new ItemDefinition(Id(x.id), x.slot, Ids(x.effectIds)));
            var lootDefs = Convert(lootTables, "loot table", x => new LootTableDefinition(Id(x.id), Convert(x.entries, "loot entry", e => new LootEntry(Id(e.itemId), e.weight))));
            try { return new ContentCatalog(locationDefs, stageDefs, encounterDefs, enemyDefs, effectDefs, powerDefs, itemDefs, lootDefs,
                Id(startingStageId), balance); }
            catch (Exception error) when (error is ArgumentException || error is InvalidOperationException)
            { throw new InvalidOperationException(SourceLabel + ": content catalog validation failed: " + error.Message, error); }
        }

        private TOutput[] Convert<TInput, TOutput>(TInput[] source, string kind, Func<TInput, TOutput> convert)
        {
            if (source == null) throw new InvalidOperationException(SourceLabel + ": " + kind + " array is null.");
            var result = new TOutput[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                try
                {
                    if (source[i] == null) throw new ArgumentException("Entry is null.");
                    result[i] = convert(source[i]);
                }
                catch (Exception error) when (error is ArgumentException || error is FormatException || error is OverflowException || error is NullReferenceException)
                { throw new InvalidOperationException(SourceLabel + ": invalid " + kind + " [" + i + "]: " + error.Message, error); }
            }
            return result;
        }

        private static ContentId Id(string value) => new ContentId(value);
        private static ContentId OptionalId(string value) => string.IsNullOrEmpty(value) ? default : new ContentId(value);
        private string SourceLabel
        {
            get
            {
#if UNITY_EDITOR
                string path = UnityEditor.AssetDatabase.GetAssetPath(this);
                if (!string.IsNullOrEmpty(path)) return path;
#endif
                return name;
            }
        }
        private static ContentId[] Ids(string[] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            var result = new ContentId[values.Length];
            for (int i = 0; i < result.Length; i++) result[i] = Id(values[i]);
            return result;
        }
    }
}
