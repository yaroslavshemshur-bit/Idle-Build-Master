using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace IBM.Domain
{
    public sealed class LocationDefinition
    {
        public ContentId Id { get; }
        public IReadOnlyList<ContentId> StageIds { get; }
        public LocationDefinition(ContentId id, IReadOnlyList<ContentId> stageIds) { Id = id; StageIds = Copy(stageIds); }
        internal static IReadOnlyList<T> Copy<T>(IReadOnlyList<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var result = new T[source.Count];
            for (int i = 0; i < result.Length; i++) result[i] = source[i];
            return Array.AsReadOnly(result);
        }
    }

    public sealed class StageDefinition
    {
        public ContentId Id { get; }
        public ContentId LocationId { get; }
        public IReadOnlyList<ContentId> EncounterIds { get; }
        public int BaseRequiredEncounters { get; }
        public bool IsBossStage { get; }
        public StageDefinition(ContentId id, ContentId locationId, IReadOnlyList<ContentId> encounterIds, int baseRequiredEncounters, bool isBossStage)
        {
            if (baseRequiredEncounters <= 0) throw new ArgumentOutOfRangeException(nameof(baseRequiredEncounters));
            Id = id; LocationId = locationId; EncounterIds = LocationDefinition.Copy(encounterIds);
            BaseRequiredEncounters = baseRequiredEncounters; IsBossStage = isBossStage;
        }
    }

    public sealed class EnemyDefinition
    {
        public ContentId Id { get; }
        public GameNumber MaxHp { get; }
        public GameNumber MinDamage { get; }
        public GameNumber MaxDamage { get; }
        public SimDuration AttackInterval { get; }
        public IReadOnlyList<string> Tags { get; }
        public ContentId BehaviorId { get; }
        public EnemyDefinition(ContentId id, GameNumber maxHp, GameNumber minDamage, GameNumber maxDamage,
            SimDuration attackInterval, IReadOnlyList<string> tags, ContentId behaviorId)
        {
            if (maxHp.CompareTo(GameNumber.Zero) <= 0 || minDamage.CompareTo(GameNumber.Zero) < 0 ||
                maxDamage.CompareTo(minDamage) < 0 || attackInterval.Microseconds <= 0)
                throw new ArgumentException("Enemy stats or attack interval are invalid.");
            Id = id; MaxHp = maxHp; MinDamage = minDamage; MaxDamage = maxDamage;
            AttackInterval = attackInterval; Tags = LocationDefinition.Copy(tags); BehaviorId = behaviorId;
        }
    }

    public sealed class EncounterDefinition
    {
        public ContentId Id { get; }
        public IReadOnlyList<ContentId> EnemyIds { get; }
        public GameNumber ExpBudget { get; }
        public ContentId LootTableId { get; }
        public EncounterDefinition(ContentId id, IReadOnlyList<ContentId> enemyIds, GameNumber expBudget, ContentId lootTableId)
        {
            if (expBudget.CompareTo(GameNumber.Zero) < 0) throw new ArgumentOutOfRangeException(nameof(expBudget));
            Id = id; EnemyIds = LocationDefinition.Copy(enemyIds); ExpBudget = expBudget; LootTableId = lootTableId;
        }
    }

    public enum EffectOperation { ModifyStat, DealDamage, Heal, ApplyStatus, DelayAttack, RepeatAttack, IgnoreBlock, Revive, SpawnEnemy, ModifyReward }
    public enum EffectTrigger { Passive, OnAttack, OnHit, OnMiss, OnCrit, OnKill, OnDamageDealt, OnIncomingAttack, OnHitTaken, OnBlock, OnEvade, OnDamageTaken, OnDeath, OnRevive, OnEncounterStart, OnEnemyDeath, OnEncounterWin }

    public sealed class EffectDefinition
    {
        public ContentId Id { get; }
        public EffectTrigger Trigger { get; }
        public EffectOperation Operation { get; }
        public GameNumber Magnitude { get; }
        public double Probability { get; }
        public IReadOnlyList<ContentId> ChildEffectIds { get; }
        public EffectDefinition(ContentId id, EffectTrigger trigger, EffectOperation operation, GameNumber magnitude, double probability, IReadOnlyList<ContentId> childEffectIds)
        {
            if (!Enum.IsDefined(typeof(EffectTrigger), trigger) || !Enum.IsDefined(typeof(EffectOperation), operation) ||
                double.IsNaN(probability) || double.IsInfinity(probability) || probability < 0 || probability > 1)
                throw new ArgumentException("Effect trigger, operation or probability is invalid.");
            Id = id; Trigger = trigger; Operation = operation; Magnitude = magnitude; Probability = probability;
            ChildEffectIds = LocationDefinition.Copy(childEffectIds);
        }
    }

    public sealed class PowerDefinition
    {
        public ContentId Id { get; }
        public IReadOnlyList<ContentId> EffectIds { get; }
        public PowerDefinition(ContentId id, IReadOnlyList<ContentId> effectIds) { Id = id; EffectIds = LocationDefinition.Copy(effectIds); }
    }

    public sealed class ItemDefinition
    {
        public ContentId Id { get; }
        public string Slot { get; }
        public IReadOnlyList<ContentId> EffectIds { get; }
        public ItemDefinition(ContentId id, string slot, IReadOnlyList<ContentId> effectIds)
        {
            if (string.IsNullOrWhiteSpace(slot)) throw new ArgumentException("Item slot is required.", nameof(slot));
            Id = id; Slot = slot; EffectIds = LocationDefinition.Copy(effectIds);
        }
    }

    public sealed class LootEntry
    {
        public ContentId ItemId { get; }
        public double Weight { get; }
        public LootEntry(ContentId itemId, double weight)
        {
            if (double.IsNaN(weight) || double.IsInfinity(weight) || weight <= 0) throw new ArgumentOutOfRangeException(nameof(weight));
            ItemId = itemId; Weight = weight;
        }
    }

    public sealed class LootTableDefinition
    {
        public ContentId Id { get; }
        public IReadOnlyList<LootEntry> Entries { get; }
        public LootTableDefinition(ContentId id, IReadOnlyList<LootEntry> entries) { Id = id; Entries = LocationDefinition.Copy(entries); }
    }

    public sealed class ContentCatalog
    {
        public IReadOnlyDictionary<ContentId, LocationDefinition> Locations { get; }
        public IReadOnlyDictionary<ContentId, StageDefinition> Stages { get; }
        public IReadOnlyDictionary<ContentId, EncounterDefinition> Encounters { get; }
        public IReadOnlyDictionary<ContentId, EnemyDefinition> Enemies { get; }
        public IReadOnlyDictionary<ContentId, EffectDefinition> Effects { get; }
        public IReadOnlyDictionary<ContentId, PowerDefinition> Powers { get; }
        public IReadOnlyDictionary<ContentId, ItemDefinition> Items { get; }
        public IReadOnlyDictionary<ContentId, LootTableDefinition> LootTables { get; }
        public string MechanicalHash { get; }

        public ContentCatalog(IEnumerable<LocationDefinition> locations, IEnumerable<StageDefinition> stages,
            IEnumerable<EncounterDefinition> encounters, IEnumerable<EnemyDefinition> enemies,
            IEnumerable<EffectDefinition> effects, IEnumerable<PowerDefinition> powers,
            IEnumerable<ItemDefinition> items, IEnumerable<LootTableDefinition> lootTables)
        {
            Locations = Map(locations, x => x.Id, "location"); Stages = Map(stages, x => x.Id, "stage");
            Encounters = Map(encounters, x => x.Id, "encounter"); Enemies = Map(enemies, x => x.Id, "enemy");
            Effects = Map(effects, x => x.Id, "effect"); Powers = Map(powers, x => x.Id, "power");
            Items = Map(items, x => x.Id, "item"); LootTables = Map(lootTables, x => x.Id, "loot table");
            ValidateReferences();
            MechanicalHash = ComputeHash();
        }

        private static IReadOnlyDictionary<ContentId, T> Map<T>(IEnumerable<T> values, Func<T, ContentId> id, string kind)
        {
            var map = new SortedDictionary<ContentId, T>();
            foreach (var value in values)
            {
                var key = id(value);
                if (string.IsNullOrEmpty(key.Value) || map.ContainsKey(key)) throw new ArgumentException("Missing or duplicate " + kind + " ID: " + key);
                map.Add(key, value);
            }
            return new System.Collections.ObjectModel.ReadOnlyDictionary<ContentId, T>(map);
        }

        private void ValidateReferences()
        {
            foreach (var location in Locations.Values)
                foreach (var id in location.StageIds)
                {
                    if (!Stages.TryGetValue(id, out var stage) || stage.LocationId != location.Id)
                        throw new ArgumentException("Location " + location.Id + " has missing or foreign stage " + id);
                }
            foreach (var stage in Stages.Values)
            {
                if (!Locations.ContainsKey(stage.LocationId)) throw new ArgumentException("Stage " + stage.Id + " has missing location " + stage.LocationId);
                if (stage.EncounterIds.Count == 0) throw new ArgumentException("Stage " + stage.Id + " has no encounters.");
                foreach (var id in stage.EncounterIds)
                    if (!Encounters.ContainsKey(id)) throw new ArgumentException("Stage " + stage.Id + " has missing encounter " + id);
            }
            foreach (var encounter in Encounters.Values)
            {
                if (encounter.EnemyIds.Count == 0) throw new ArgumentException("Encounter " + encounter.Id + " has no enemies.");
                foreach (var id in encounter.EnemyIds)
                    if (!Enemies.ContainsKey(id)) throw new ArgumentException("Encounter " + encounter.Id + " has missing enemy " + id);
                if (!string.IsNullOrEmpty(encounter.LootTableId.Value) && !LootTables.ContainsKey(encounter.LootTableId))
                    throw new ArgumentException("Encounter " + encounter.Id + " has missing loot table " + encounter.LootTableId);
            }
            foreach (var power in Powers.Values)
                foreach (var id in power.EffectIds)
                    if (!Effects.ContainsKey(id)) throw new ArgumentException("Power " + power.Id + " has missing effect " + id);
            foreach (var item in Items.Values)
                foreach (var id in item.EffectIds)
                    if (!Effects.ContainsKey(id)) throw new ArgumentException("Item " + item.Id + " has missing effect " + id);
            foreach (var table in LootTables.Values)
                foreach (var entry in table.Entries)
                    if (!Items.ContainsKey(entry.ItemId)) throw new ArgumentException("Loot table " + table.Id + " has missing item " + entry.ItemId);
            var marks = new Dictionary<ContentId, int>();
            var path = new List<ContentId>();
            foreach (var effect in Effects.Values) Visit(effect.Id, marks, path);
        }

        private void Visit(ContentId id, Dictionary<ContentId, int> marks, List<ContentId> path)
        {
            if (!Effects.ContainsKey(id)) throw new ArgumentException("Missing child effect " + id);
            if (marks.TryGetValue(id, out int mark))
            {
                if (mark == 1) throw new ArgumentException("Effect dependency cycle: " + string.Join(" -> ", path) + " -> " + id);
                return;
            }
            marks[id] = 1;
            path.Add(id);
            foreach (var child in Effects[id].ChildEffectIds) Visit(child, marks, path);
            path.RemoveAt(path.Count - 1);
            marks[id] = 2;
        }

        private string ComputeHash()
        {
            var text = new StringBuilder();
            foreach (var value in Locations.Values) { text.Append("L|").Append(value.Id); foreach (var id in value.StageIds) text.Append('|').Append(id); text.Append('\n'); }
            foreach (var value in Stages.Values) { text.Append("S|").Append(value.Id).Append('|').Append(value.LocationId).Append('|').Append(value.BaseRequiredEncounters).Append('|').Append(value.IsBossStage); foreach (var id in value.EncounterIds) text.Append('|').Append(id); text.Append('\n'); }
            foreach (var value in Encounters.Values) { text.Append("C|").Append(value.Id).Append('|').Append(value.ExpBudget).Append('|').Append(value.LootTableId); foreach (var id in value.EnemyIds) text.Append('|').Append(id); text.Append('\n'); }
            foreach (var value in Enemies.Values) { text.Append("N|").Append(value.Id).Append('|').Append(value.MaxHp).Append('|').Append(value.MinDamage).Append('|').Append(value.MaxDamage).Append('|').Append(value.AttackInterval.Microseconds).Append('|').Append(value.BehaviorId); foreach (var tag in value.Tags) text.Append('|').Append(tag); text.Append('\n'); }
            foreach (var value in Effects.Values) { text.Append("F|").Append(value.Id).Append('|').Append((int)value.Trigger).Append('|').Append((int)value.Operation).Append('|').Append(value.Magnitude).Append('|').Append(value.Probability.ToString("R", System.Globalization.CultureInfo.InvariantCulture)); foreach (var id in value.ChildEffectIds) text.Append('|').Append(id); text.Append('\n'); }
            foreach (var value in Powers.Values) { text.Append("P|").Append(value.Id); foreach (var id in value.EffectIds) text.Append('|').Append(id); text.Append('\n'); }
            foreach (var value in Items.Values) { text.Append("I|").Append(value.Id).Append('|').Append(value.Slot); foreach (var id in value.EffectIds) text.Append('|').Append(id); text.Append('\n'); }
            foreach (var value in LootTables.Values) { text.Append("T|").Append(value.Id); foreach (var entry in value.Entries) text.Append('|').Append(entry.ItemId).Append(':').Append(entry.Weight.ToString("R", System.Globalization.CultureInfo.InvariantCulture)); text.Append('\n'); }
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text.ToString()))).Replace("-", "").ToLowerInvariant();
        }
    }
}
