using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IBM.Application;
using IBM.Authoring;
using IBM.Domain;
using IBM.Infrastructure;
using NUnit.Framework;
using UnityEngine;

namespace IBM.Tests.EditMode
{
    public sealed class ArchitectureTests
    {
        private static readonly ContentId Location = new ContentId("test.location");
        private static readonly ContentId Stage = new ContentId("test.stage");
        private static readonly ContentId Encounter = new ContentId("test.encounter");
        private static readonly ContentId Enemy = new ContentId("test.enemy");

        [Test]
        public void AuthoredContentCompilesAndRejectsBrokenReferences()
        {
            var asset = ScriptableObject.CreateInstance<GameContentAsset>();
            try
            {
                asset.locations = new[] { new LocationRecord { id = Location.Value, stageIds = new[] { Stage.Value } } };
                asset.stages = new[] { new StageRecord { id = Stage.Value, locationId = Location.Value, encounterIds = new[] { Encounter.Value }, baseRequiredEncounters = 3 } };
                asset.encounters = new[] { new EncounterRecord { id = Encounter.Value, enemyIds = new[] { Enemy.Value }, expBudget = new AuthoredNumber { coefficient = "10", exponent = "0" } } };
                asset.enemies = new[] { new EnemyRecord { id = Enemy.Value, maxHp = new AuthoredNumber { coefficient = "100", exponent = "0" },
                    minDamage = new AuthoredNumber { coefficient = "1", exponent = "0" },
                    maxDamage = new AuthoredNumber { coefficient = "2", exponent = "0" }, attackIntervalSeconds = 1 } };
                var catalog = asset.Compile();
                Assert.That(catalog.Stages[Stage].BaseRequiredEncounters, Is.EqualTo(3));
                Assert.That(catalog.MechanicalHash, Has.Length.EqualTo(64));
                asset.encounters[0].enemyIds = new[] { "missing.enemy" };
                Assert.Throws<InvalidOperationException>(() => asset.Compile());
            }
            finally { UnityEngine.Object.DestroyImmediate(asset); }
        }

        [Test]
        public void SessionRejectsWithoutChangingRevisionAndDeduplicatesAppliedCommand()
        {
            var session = NewSession();
            var rejected = session.Execute(new SelectStageCommand("select-1", 0, new ContentId("unknown")));
            Assert.That(rejected.Status, Is.EqualTo(CommandStatus.Rejected));
            Assert.That(session.ReadView().Revision, Is.EqualTo(0));
            var applied = session.Execute(new SetAutoPushCommand("toggle-1", 0, false));
            Assert.That(applied.Status, Is.EqualTo(CommandStatus.Applied));
            Assert.That(session.ReadView().AutoPush, Is.False);
            Assert.That(session.Execute(new SetAutoPushCommand("toggle-1", 0, true)).Status, Is.EqualTo(CommandStatus.AlreadyApplied));
            Assert.That(session.Execute(new SetAutoPushCommand("toggle-2", 0, true)).Status, Is.EqualTo(CommandStatus.Rejected));
            Assert.That(session.ReadView().Revision, Is.EqualTo(1));
        }

        [Test]
        public void SchedulerOrdersEqualTimeAndRestoresContinuation()
        {
            var scheduler = new SimulationScheduler(new SimTime(0));
            var actor = new InstanceId(1);
            scheduler.Schedule(new SimTime(100), 2, ScheduledEventKind.Attack, actor, 0, Enemy);
            scheduler.Schedule(new SimTime(100), 1, ScheduledEventKind.EffectExpire, actor, 0, Enemy);
            scheduler.Schedule(new SimTime(100), 2, ScheduledEventKind.Attack, actor, 0, Enemy);
            var restored = SimulationScheduler.Restore(scheduler.Now, scheduler.NextSequence, scheduler.CapturePending());
            var handler = new RecordingHandler();
            Assert.That(restored.AdvanceTo(new SimTime(100), 2, handler).Status, Is.EqualTo(AdvanceStatus.Yielded));
            Assert.That(restored.AdvanceTo(new SimTime(100), 2, handler).Status, Is.EqualTo(AdvanceStatus.ReachedTarget));
            Assert.That(handler.Sequence, Is.EqualTo(new ulong[] { 2, 1, 3 }));
        }

        [Test]
        public void EventBudgetYieldsWithoutDroppingReaction()
        {
            var processor = new CombatEventProcessor(8, 32);
            processor.Begin(1, 2, new ContentId("attack"), new SimTime(0), CombatEventPriority.DamageOrHealing, CombatEventOrigin.DirectAttack);
            var seen = new List<ulong>();
            Action<CombatEvent, CombatEventProcessor> handler = (entry, queue) =>
            {
                seen.Add(entry.ActionId);
                if (entry.ActionId == 1) queue.Emit(entry, 2, 1, new ContentId("reflect"), CombatEventPriority.Reaction, CombatEventOrigin.Reactive);
            };
            Assert.That(processor.Resume(1, handler), Is.False);
            Assert.That(processor.Resume(1, handler), Is.True);
            Assert.That(seen, Is.EqualTo(new ulong[] { 1, 2 }));
        }

        [Test]
        public void SaveRestoresStateAndFallsBackFromCorruptLatestGeneration()
        {
            string directory = Path.Combine(Path.GetTempPath(), "ibm-save-test-" + Guid.NewGuid().ToString("N"));
            try
            {
                var catalog = Catalog();
                var versions = new VersionStamp(1, 1, 1, Pcg32.AlgorithmVersion, "test-content");
                var store = new FileSaveStore(directory);
                var saves = new SaveCoordinator(new UnityJsonSaveCodec(), store, catalog, versions);
                var session = NewSession(catalog, versions);
                saves.Save(session);
                session.Execute(new SetAutoPushCommand("toggle", 0, false));
                saves.Save(session);
                Assert.That(saves.RestoreOrThrow().Run.AutoPush, Is.False);
                string newest = Directory.GetFiles(directory, "save-*.bin").OrderBy(x => x).Last();
                File.WriteAllText(newest, "corrupt");
                var recovered = saves.RestoreOrThrow();
                Assert.That(recovered.Revision, Is.EqualTo(0));
                Assert.That(recovered.Run.AutoPush, Is.True);
            }
            finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
        }

        [Test]
        public void OfflineFormulaUsesAuthoredValuesAndRetainsResidual()
        {
            var values = new[]
            {
                Pair("offline.max_seconds", BalanceValueKind.DurationSeconds, "21600", "0"),
                Pair("offline.base_efficiency", BalanceValueKind.Probability, "5", "-1"),
                Pair("offline.min_encounter_seconds", BalanceValueKind.DurationSeconds, "60", "0")
            };
            var balance = new BalanceCatalog("test", values);
            var first = OfflineCalculator.Calculate(TimeSpan.FromSeconds(30), 0, 1, balance);
            Assert.That(first.VirtualClears, Is.EqualTo(0));
            Assert.That(first.Residual, Is.EqualTo(0.25m));
            var second = OfflineCalculator.Calculate(TimeSpan.FromSeconds(90), first.Residual, 1, balance);
            Assert.That(second.VirtualClears, Is.EqualTo(1));
            Assert.That(second.Residual, Is.EqualTo(0m));
            var capped = OfflineCalculator.Calculate(TimeSpan.FromHours(12), 0, 1, balance);
            Assert.That(capped.VirtualClears, Is.EqualTo(180));
            Assert.That(capped.CapReached, Is.True);
        }

        [Test]
        public void ApprovedCombatBaselineUsesBalanceParameters()
        {
            var values = new[]
            {
                Pair("combat.min_damage.base", BalanceValueKind.Number, "5", "0"),
                Pair("combat.min_damage.dex_factor", BalanceValueKind.Number, "5", "-1"),
                Pair("combat.max_damage.str_factor", BalanceValueKind.Number, "2", "0"),
                Pair("combat.max_hp.base", BalanceValueKind.Number, "20", "0"),
                Pair("combat.max_hp.vit_factor", BalanceValueKind.Number, "100", "0"),
                Pair("combat.block.str_factor", BalanceValueKind.Number, "1", "-1"),
                Pair("combat.block.vit_factor", BalanceValueKind.Number, "5", "-1"),
                Pair("combat.block.reference", BalanceValueKind.Number, "100", "0"),
                Pair("combat.accuracy.base", BalanceValueKind.Number, "100", "0"),
                Pair("combat.evasion.base", BalanceValueKind.Number, "100", "0"),
                Pair("combat.attack_speed.base_rating", BalanceValueKind.Number, "1", "0"),
                Pair("combat.attack_speed.agi_factor", BalanceValueKind.Number, "1", "-2"),
                Pair("combat.attack_speed.rating_scale", BalanceValueKind.Number, "2", "0"),
                Pair("combat.regen.base", BalanceValueKind.Number, "1", "0"),
                Pair("combat.regen.vit_factor", BalanceValueKind.Number, "1", "-1"),
                Pair("combat.hit.min", BalanceValueKind.Number, "5", "-2"),
                Pair("combat.hit.max", BalanceValueKind.Number, "95", "-2")
            };
            var balance = new BalanceCatalog("test", values);
            var attributes = new PrimaryAttributes(GameNumber.One, GameNumber.One, GameNumber.One, GameNumber.One);
            var stats = CombatMath.Baseline(attributes, balance);
            Assert.That(stats.MinDamage.ToBoundedDouble(), Is.EqualTo(5.5d));
            Assert.That(stats.MaxDamage.ToBoundedDouble(), Is.EqualTo(7.5d));
            Assert.That(stats.MaxHp.ToBoundedDouble(), Is.EqualTo(120d));
            Assert.That(stats.Block.ToBoundedDouble(), Is.EqualTo(0.6d));
            Assert.That(stats.Accuracy.ToBoundedDouble(), Is.EqualTo(101d));
            Assert.That(stats.Evasion.ToBoundedDouble(), Is.EqualTo(101d));
            Assert.That(stats.RegenPerSecond.ToBoundedDouble(), Is.EqualTo(1.1d));
            Assert.That(stats.AttacksPerSecond, Is.EqualTo(Math.Log(2.02d, 2d)).Within(1e-10));
            Assert.That(CombatMath.HitChance(stats, stats, balance), Is.EqualTo(0.95d));
            Assert.That(CombatMath.ApplyBlock(GameNumber.FromInt64(100), stats.Block, balance).ToBoundedDouble(),
                Is.EqualTo(100d * 100d / 100.6d).Within(1e-8));
        }

        private static KeyValuePair<ContentId, BalanceValue> Pair(string id, BalanceValueKind kind, string coefficient, string exponent) =>
            new KeyValuePair<ContentId, BalanceValue>(new ContentId(id), new BalanceValue(kind, GameNumber.Parse(coefficient, exponent)));

        private static GameSession NewSession(ContentCatalog catalog = null, VersionStamp? versions = null)
        {
            catalog = catalog ?? Catalog();
            var selected = versions ?? new VersionStamp(1, 1, 1, Pcg32.AlgorithmVersion, "test-content");
            return new GameSession(new GameState(new AccountState("profile-1"), new RunState("run-1", Stage),
                new CombatState(), selected, SessionRandomState.Create(42), new OfflineAccountingState()), catalog);
        }

        private static ContentCatalog Catalog() => new ContentCatalog(
            new[] { new LocationDefinition(Location, new[] { Stage }) },
            new[] { new StageDefinition(Stage, Location, new[] { Encounter }, 3, false) },
            new[] { new EncounterDefinition(Encounter, new[] { Enemy }, GameNumber.FromInt64(10), default) },
            new[] { new EnemyDefinition(Enemy, GameNumber.FromInt64(100), GameNumber.One, GameNumber.FromInt64(2),
                SimDuration.FromSeconds(1), Array.Empty<string>(), default) },
            Array.Empty<EffectDefinition>(), Array.Empty<PowerDefinition>(), Array.Empty<ItemDefinition>(), Array.Empty<LootTableDefinition>());

        private sealed class RecordingHandler : IScheduledEventHandler
        {
            public readonly List<ulong> Sequence = new List<ulong>();
            public void AdvanceContinuous(SimTime from, SimTime to) { }
            public void Handle(ScheduledEvent scheduled, SimulationScheduler scheduler) => Sequence.Add(scheduled.Sequence);
        }
    }
}
