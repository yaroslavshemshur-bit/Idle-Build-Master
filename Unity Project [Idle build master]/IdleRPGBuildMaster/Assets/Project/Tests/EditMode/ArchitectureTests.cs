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
                asset.startingStageId = Stage.Value;
                asset.locations = new[] { new LocationRecord { id = Location.Value, stageIds = new[] { Stage.Value }, progressStageCount = 1 } };
                asset.stages = new[] { new StageRecord { id = Stage.Value, locationId = Location.Value, encounterIds = new[] { Encounter.Value }, baseRequiredEncounters = 3 } };
                asset.encounters = new[] { new EncounterRecord { id = Encounter.Value, enemyIds = new[] { Enemy.Value }, expBudget = new AuthoredNumber { coefficient = "10", exponent = "0" } } };
                asset.enemies = new[] { new EnemyRecord { id = Enemy.Value, maxHp = new AuthoredNumber { coefficient = "100", exponent = "0" },
                    minDamage = new AuthoredNumber { coefficient = "1", exponent = "0" },
                    maxDamage = new AuthoredNumber { coefficient = "2", exponent = "0" }, attackIntervalSeconds = 1 } };
                var catalog = asset.Compile();
                Assert.That(catalog.Stages[Stage].BaseRequiredEncounters, Is.EqualTo(3));
                Assert.That(catalog.MechanicalHash, Has.Length.EqualTo(64));
                var firstBalance = new BalanceCatalog("first", new[] { Pair("combat.min_damage.base", BalanceValueKind.Number, "5", "0") });
                var secondBalance = new BalanceCatalog("second", new[] { Pair("combat.min_damage.base", BalanceValueKind.Number, "6", "0") });
                Assert.That(asset.Compile(null, firstBalance).MechanicalHash,
                    Is.Not.EqualTo(asset.Compile(null, secondBalance).MechanicalHash));
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
                var restoredSession = new GameSession(saves.RestoreOrThrow(), catalog);
                Assert.That(restoredSession.Execute(new SetAutoPushCommand("toggle", 1, true)).Status,
                    Is.EqualTo(CommandStatus.AlreadyApplied));
                Assert.That(restoredSession.ReadView().AutoPush, Is.False);
                string newest = Directory.GetFiles(directory, "save-*.bin").OrderBy(x => x).Last();
                File.WriteAllText(newest, "corrupt");
                var recovered = saves.RestoreOrThrow();
                Assert.That(recovered.Revision, Is.EqualTo(0));
                Assert.That(recovered.Run.AutoPush, Is.True);
            }
            finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
        }

        [Test]
        public void BalanceChangeRequiresExplicitSaveCompatibilityHandler()
        {
            var first = new BalanceCatalog("first", new[] { Pair("combat.min_damage.base", BalanceValueKind.Number, "5", "0") });
            var second = new BalanceCatalog("second", new[] { Pair("combat.min_damage.base", BalanceValueKind.Number, "6", "0") });
            var oldCatalog = Catalog(first);
            var newCatalog = Catalog(second);
            var versions = new VersionStamp(1, 1, 1, Pcg32.AlgorithmVersion, "test-content");
            var bytes = new UnityJsonSaveCodec().Encode(NewSession(oldCatalog, versions).CaptureAtSafePoint(), oldCatalog);
            Assert.Throws<IncompatibleSaveException>(() => new UnityJsonSaveCodec().Decode(bytes, newCatalog, versions));
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
                Pair("combat.death_regen.multiplier", BalanceValueKind.Number, "10", "0"),
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
            var modifiers = new[]
            {
                new StatModifier(CombatStat.Accuracy, StatModifierKind.Multiplier, GameNumber.FromInt64(5)),
                new StatModifier(CombatStat.MinDamage, StatModifierKind.Conversion, GameNumber.Parse("1", "-1"), CombatStat.Accuracy),
                new StatModifier(CombatStat.MinDamage, StatModifierKind.Multiplier, GameNumber.FromInt64(2)),
                new StatModifier(CombatStat.AttackSpeedRating, StatModifierKind.Multiplier, GameNumber.Parse("5", "-1"))
            };
            var resolved = new CombatStatResolver(attributes, balance, modifiers).ResolveDerived();
            Assert.That(resolved.Accuracy.ToBoundedDouble(), Is.EqualTo(505d));
            Assert.That(resolved.MinDamage.ToBoundedDouble(), Is.EqualTo(112d));
            Assert.That(resolved.AttacksPerSecond, Is.EqualTo(Math.Log(1.01d, 2d)).Within(1e-10));
            Assert.That(CombatMath.RebaseCurrentHp(GameNumber.FromInt64(60), GameNumber.FromInt64(120),
                GameNumber.FromInt64(240)).ToBoundedDouble(), Is.EqualTo(120d));
            Assert.That(CombatMath.DeathRegenerationPerSecond(stats, balance).ToBoundedDouble(), Is.EqualTo(11d));
            var damage = CombatMath.ResolveDamage(GameNumber.FromInt64(100), stats.Block,
                GameNumber.FromInt64(60), stats.MaxHp, balance, proposal =>
                {
                    Assert.That(proposal.ProposedHp.IsZero, Is.True);
                    Assert.That(proposal.ResolvedDamage.ToBoundedDouble(), Is.GreaterThan(60d));
                    return GameNumber.One;
                });
            Assert.That(damage.FatalPreventionApplied, Is.True);
            Assert.That(damage.FinalHp.ToBoundedDouble(), Is.EqualTo(1d));
            Assert.That(damage.ActualHpLost.ToBoundedDouble(), Is.EqualTo(59d));
            var hitRandom = new FixedRandomStream(0d, uint.MaxValue);
            var hit = BasicAttackResolver.Resolve(stats, stats, stats.MaxHp, stats.Block, hitRandom, balance);
            Assert.That(hit.Hit, Is.True);
            Assert.That(hit.Damage.FinalHp.CompareTo(stats.MaxHp), Is.LessThan(0));
            Assert.That(hitRandom.DamageRolls, Is.EqualTo(1));
            var missRandom = new FixedRandomStream(0.99d, uint.MaxValue);
            var miss = BasicAttackResolver.Resolve(stats, stats, stats.MaxHp, stats.Block, missRandom, balance);
            Assert.That(miss.Hit, Is.False);
            Assert.That(missRandom.DamageRolls, Is.Zero);
            var cycle = new[]
            {
                new StatModifier(CombatStat.MinDamage, StatModifierKind.Conversion, GameNumber.One, CombatStat.Accuracy),
                new StatModifier(CombatStat.Accuracy, StatModifierKind.Conversion, GameNumber.One, CombatStat.MinDamage)
            };
            Assert.Throws<InvalidOperationException>(() => new CombatStatResolver(attributes, balance, cycle).Resolve(CombatStat.MinDamage));
        }

        [Test]
        public void CompressedStageCrossesAuthoredMilestonesInProgressOrder()
        {
            var stages = Enumerable.Range(1, 10).Select(index => new ContentId("stage." + index)).ToArray();
            var location = new LocationDefinition(new ContentId("location"), stages, 100, new[]
            {
                new ProgressMilestone(new ContentId("choice.44"), 44, ProgressMilestoneKind.PowerChoice, default, true),
                new ProgressMilestone(new ContentId("unlock.43"), 43, ProgressMilestoneKind.UnlockPower,
                    new ContentId("power.ghost-step"), false),
                new ProgressMilestone(new ContentId("gear.14"), 14, ProgressMilestoneKind.GearDropsBegin, default, false)
            });
            var firstRun = LocationProgression.Crossed(location, 4, 1, 1, true);
            Assert.That(firstRun.Select(x => x.Id.Value), Is.EqualTo(new[] { "unlock.43", "choice.44" }));
            var laterRun = LocationProgression.Crossed(location, 4, 1, 1, false);
            Assert.That(laterRun.Select(x => x.Id.Value), Is.EqualTo(new[] { "unlock.43" }));
            Assert.That(LocationProgression.Crossed(location, 1, 10, 4, true).Select(x => x.Id.Value),
                Is.EqualTo(new[] { "gear.14" }));
        }

        [Test]
        public void OrdinaryAttackTimelineIsPartitionAndSaveInvariant()
        {
            var tuning = UnityEditor.AssetDatabase.LoadAssetAtPath<BalanceTuningAsset>(
                "Assets/Project/Content/Authoring/Tuning/BalanceTuning.asset");
            Assert.That(tuning, Is.Not.Null);
            var balance = tuning.Compile();
            var stats = CombatMath.Baseline(new PrimaryAttributes(GameNumber.One, GameNumber.One,
                GameNumber.One, GameNumber.One), balance);
            var provider = new ConstantStatsProvider(stats);
            var targets = new FirstOpposingTargetPolicy();
            var initial = new CombatState(Encounter, new SimTime(0), new[]
            {
                new ActorState { InstanceId = 1, DefinitionId = new ContentId("hero"), IsHero = true,
                    Hp = stats.MaxHp, MaxHp = stats.MaxHp, Life = ActorLifeState.Alive },
                new ActorState { InstanceId = 2, DefinitionId = Enemy, IsHero = false,
                    Hp = stats.MaxHp, MaxHp = stats.MaxHp, Life = ActorLifeState.Alive }
            });
            var started = CombatTimeline.StartOrdinaryAttacks(initial, provider, 0);
            var random = SessionRandomState.Create(42);
            var single = CombatTimeline.Advance(started, random, new SimTime(3000000), 100,
                provider, targets, balance, 0, 1, HeroDownedAttackClockPolicy.PauseRemaining);
            var first = CombatTimeline.Advance(started, random, new SimTime(1000000), 100,
                provider, targets, balance, 0, 1, HeroDownedAttackClockPolicy.PauseRemaining);
            var second = CombatTimeline.Advance(first.Combat, first.Random, new SimTime(2000000), 100,
                provider, targets, balance, 0, 1, HeroDownedAttackClockPolicy.PauseRemaining);
            var third = CombatTimeline.Advance(second.Combat, second.Random, new SimTime(3000000), 100,
                provider, targets, balance, 0, 1, HeroDownedAttackClockPolicy.PauseRemaining);
            Assert.That(third.Combat.Actors.Select(x => x.Hp.ToString()),
                Is.EqualTo(single.Combat.Actors.Select(x => x.Hp.ToString())));
            Assert.That(third.Random.Combat.State, Is.EqualTo(single.Random.Combat.State));
            Assert.That(third.Combat.PendingEvents.Select(x => x.Sequence),
                Is.EqualTo(single.Combat.PendingEvents.Select(x => x.Sequence)));
            Assert.That(first.Attacks.Concat(second.Attacks).Concat(third.Attacks)
                    .Select(x => x.Time.Microseconds + ":" + x.AttackerId + ":" + x.TargetId + ":" + x.Result.Hit),
                Is.EqualTo(single.Attacks.Select(x => x.Time.Microseconds + ":" + x.AttackerId + ":" + x.TargetId + ":" + x.Result.Hit)));

            var yielded = CombatTimeline.Advance(started, random, new SimTime(3000000), 2,
                provider, targets, balance, 0, 1, HeroDownedAttackClockPolicy.PauseRemaining);
            Assert.That(yielded.Advance.Status, Is.EqualTo(AdvanceStatus.Yielded));
            var resumed = CombatTimeline.Advance(yielded.Combat, yielded.Random, new SimTime(3000000), 100,
                provider, targets, balance, 0, 1, HeroDownedAttackClockPolicy.PauseRemaining);
            Assert.That(resumed.Combat.Actors.Select(x => x.Hp.ToString()),
                Is.EqualTo(single.Combat.Actors.Select(x => x.Hp.ToString())));

            var catalog = Catalog(balance);
            var versions = new VersionStamp(1, 1, 1, Pcg32.AlgorithmVersion, "test-content");
            var state = new GameState(new AccountState("profile"), new RunState("run", Stage),
                first.Combat, versions, first.Random, new OfflineAccountingState());
            var codec = new UnityJsonSaveCodec();
            var restored = codec.Decode(codec.Encode(state, catalog), catalog, versions);
            var afterLoad = CombatTimeline.Advance(restored.Combat, restored.Random, new SimTime(3000000), 100,
                provider, targets, balance, 0, 1, HeroDownedAttackClockPolicy.PauseRemaining);
            Assert.That(afterLoad.Combat.Actors.Select(x => x.Hp.ToString()),
                Is.EqualTo(single.Combat.Actors.Select(x => x.Hp.ToString())));
            Assert.That(afterLoad.Random.Combat.State, Is.EqualTo(single.Random.Combat.State));
            var session = new GameSession(new GameState(new AccountState("profile-2"),
                new RunState("run-2", Stage), started, versions, random, new OfflineAccountingState()), catalog);
            session.AdvanceCombat(new SimTime(3000000), 100, provider, targets, balance, 0, 1,
                HeroDownedAttackClockPolicy.PauseRemaining);
            Assert.That(session.CaptureAtSafePoint().Combat.Actors.Select(x => x.Hp.ToString()),
                Is.EqualTo(single.Combat.Actors.Select(x => x.Hp.ToString())));
        }

        [Test]
        public void DownedHeroPausesEnemyClocksAndRevivesAfterExactRecovery()
        {
            var balance = UnityEditor.AssetDatabase.LoadAssetAtPath<BalanceTuningAsset>(
                "Assets/Project/Content/Authoring/Tuning/BalanceTuning.asset").Compile();
            var heroStats = new DerivedStats(GameNumber.Zero, GameNumber.Zero, GameNumber.FromInt64(120),
                GameNumber.Zero, GameNumber.FromInt64(100), GameNumber.FromInt64(100), GameNumber.One, 1d);
            var enemyStats = new DerivedStats(GameNumber.FromInt64(200), GameNumber.FromInt64(200),
                GameNumber.FromInt64(1000), GameNumber.Zero, GameNumber.FromInt64(100),
                GameNumber.FromInt64(100), GameNumber.Zero, 1d);
            var provider = new PerActorStatsProvider(heroStats, enemyStats);
            var targets = new FirstOpposingTargetPolicy();
            var initial = new CombatState(Encounter, new SimTime(0), new[]
            {
                new ActorState { InstanceId = 1, DefinitionId = new ContentId("hero"), IsHero = true,
                    Hp = heroStats.MaxHp, MaxHp = heroStats.MaxHp, Life = ActorLifeState.Alive },
                new ActorState { InstanceId = 2, DefinitionId = Enemy, IsHero = false,
                    Hp = enemyStats.MaxHp, MaxHp = enemyStats.MaxHp, Life = ActorLifeState.Alive }
            });
            var started = CombatTimeline.StartOrdinaryAttacks(initial, provider, 0);
            var random = SessionRandomState.Create(42);
            var downed = CombatTimeline.Advance(started, random, new SimTime(10000000), 100,
                provider, targets, balance, 0, 1, HeroDownedAttackClockPolicy.PauseRemaining);
            Assert.That(downed.Combat.Actors[0].Life, Is.EqualTo(ActorLifeState.Downed));
            Assert.That(downed.Combat.PausedEvents.Count, Is.EqualTo(2));
            Assert.That(downed.Combat.PendingEvents.Count(x => x.Kind == ScheduledEventKind.Revive), Is.EqualTo(1));
            Assert.That(downed.Combat.Actors[1].Hp.ToString(), Is.EqualTo(enemyStats.MaxHp.ToString()));
            var reviveDue = downed.Combat.PendingEvents.Single(x => x.Kind == ScheduledEventKind.Revive).Due;
            var beforeRevive = CombatTimeline.Advance(downed.Combat, downed.Random,
                new SimTime(reviveDue.Microseconds - 1), 100, provider, targets, balance, 0, 1,
                HeroDownedAttackClockPolicy.PauseRemaining);
            Assert.That(beforeRevive.Combat.Actors[0].Life, Is.EqualTo(ActorLifeState.Downed));
            Assert.That(beforeRevive.Combat.Actors[0].Hp.CompareTo(heroStats.MaxHp), Is.LessThan(0));
            var revived = CombatTimeline.Advance(beforeRevive.Combat, beforeRevive.Random, reviveDue, 100,
                provider, targets, balance, 0, 1, HeroDownedAttackClockPolicy.PauseRemaining);
            Assert.That(revived.Combat.Actors[0].Life, Is.EqualTo(ActorLifeState.Alive));
            Assert.That(revived.Combat.Actors[0].Hp.ToString(), Is.EqualTo(heroStats.MaxHp.ToString()));
            Assert.That(revived.Combat.PausedEvents, Is.Empty);
            Assert.That(revived.Combat.PendingEvents.Count(x => x.Kind == ScheduledEventKind.Attack), Is.EqualTo(2));
            var whole = CombatTimeline.Advance(started, random, reviveDue, 100,
                provider, targets, balance, 0, 1, HeroDownedAttackClockPolicy.PauseRemaining);
            Assert.That(whole.Combat.Actors.Select(x => x.Hp.ToString()),
                Is.EqualTo(revived.Combat.Actors.Select(x => x.Hp.ToString())));
            Assert.That(whole.Random.Combat.State, Is.EqualTo(revived.Random.Combat.State));

            var catalog = Catalog(balance);
            var versions = new VersionStamp(1, 1, 1, Pcg32.AlgorithmVersion, "test-content");
            var state = new GameState(new AccountState("profile"), new RunState("run", Stage),
                downed.Combat, versions, downed.Random, new OfflineAccountingState());
            var codec = new UnityJsonSaveCodec();
            var restored = codec.Decode(codec.Encode(state, catalog), catalog, versions);
            var afterLoad = CombatTimeline.Advance(restored.Combat, restored.Random, reviveDue, 100,
                provider, targets, balance, 0, 1, HeroDownedAttackClockPolicy.PauseRemaining);
            Assert.That(afterLoad.Combat.Actors.Select(x => x.Hp.ToString()),
                Is.EqualTo(revived.Combat.Actors.Select(x => x.Hp.ToString())));
        }

        [Test]
        public void EncounterClearAwardsOnceAndCrossesAuthoredMilestones()
        {
            var secondStage = new ContentId("test.stage.two");
            var secondEncounter = new ContentId("test.encounter.two");
            var unlockedPower = new ContentId("test.power.unlocked");
            var choiceId = new ContentId("test.choice.25");
            var unlockId = new ContentId("test.unlock.50");
            var location = new LocationDefinition(Location, new[] { Stage, secondStage }, 100,
                new[]
                {
                    new ProgressMilestone(choiceId, 25, ProgressMilestoneKind.PowerChoice, default, false),
                    new ProgressMilestone(unlockId, 50, ProgressMilestoneKind.UnlockPower, unlockedPower, false)
                }, 2);
            var catalog = new ContentCatalog(new[] { location },
                new[]
                {
                    new StageDefinition(Stage, Location, new[] { Encounter }, 2, false),
                    new StageDefinition(secondStage, Location, new[] { secondEncounter }, 2, false)
                },
                new[]
                {
                    new EncounterDefinition(Encounter, new[] { Enemy }, GameNumber.FromInt64(10), default),
                    new EncounterDefinition(secondEncounter, new[] { Enemy }, GameNumber.FromInt64(20), default)
                },
                new[] { new EnemyDefinition(Enemy, GameNumber.FromInt64(100), GameNumber.One,
                    GameNumber.FromInt64(2), SimDuration.FromSeconds(1), Array.Empty<string>(), default) },
                Array.Empty<EffectDefinition>(), new[] { new PowerDefinition(unlockedPower, Array.Empty<ContentId>()) },
                Array.Empty<ItemDefinition>(), Array.Empty<LootTableDefinition>(), Stage);
            var versions = new VersionStamp(1, 1, 1, Pcg32.AlgorithmVersion, "test-content");
            var firstState = new GameState(new AccountState("profile"), new RunState("run", Stage),
                ClearedEncounter(Encounter), versions, SessionRandomState.Create(55), new OfflineAccountingState());
            var firstSession = new GameSession(firstState, catalog);
            var first = firstSession.ResolveEncounterClear();
            Assert.That(first.Applied, Is.True);
            Assert.That(first.ExpGranted.ToBoundedDouble(), Is.EqualTo(10d));
            Assert.That(first.Milestones.Select(x => x.Id), Is.EqualTo(new[] { choiceId }));
            Assert.That(firstSession.CaptureAtSafePoint().Run.PendingPowerChoiceMilestones,
                Is.EqualTo(new[] { choiceId }));
            Assert.That(firstSession.ResolveEncounterClear().Applied, Is.False);
            Assert.That(firstSession.CaptureAtSafePoint().Run.Exp.ToBoundedDouble(), Is.EqualTo(10d));
            var codec = new UnityJsonSaveCodec();
            var restored = codec.Decode(codec.Encode(firstSession.CaptureAtSafePoint(), catalog), catalog, versions);
            Assert.That(restored.Run.StageRequiredClears[Stage], Is.EqualTo(1));
            Assert.That(restored.Run.PendingPowerChoiceMilestones, Is.EqualTo(new[] { choiceId }));

            var secondState = new GameState(restored.Account, restored.Run, ClearedEncounter(Encounter),
                versions, restored.Random, restored.Offline);
            var secondSession = new GameSession(secondState, catalog);
            var second = secondSession.ResolveEncounterClear();
            Assert.That(second.StageCompleted, Is.True);
            Assert.That(second.SelectedStageId, Is.EqualTo(secondStage));
            Assert.That(second.Milestones.Select(x => x.Id), Is.EqualTo(new[] { unlockId }));
            Assert.That(secondSession.CaptureAtSafePoint().Account.UnlockedPowers.Contains(unlockedPower), Is.True);
            Assert.That(secondSession.CaptureAtSafePoint().Run.Exp.ToBoundedDouble(), Is.EqualTo(20d));
            Assert.That(secondSession.CaptureAtSafePoint().Run.CompletedStages.Contains(Stage), Is.True);

            var activeState = new GameState(secondSession.CaptureAtSafePoint().Account,
                secondSession.CaptureAtSafePoint().Run,
                new CombatState(secondEncounter, new SimTime(0), new[]
                {
                    new ActorState { InstanceId = 1, DefinitionId = new ContentId("hero"), IsHero = true,
                        Hp = GameNumber.FromInt64(100), MaxHp = GameNumber.FromInt64(100), Life = ActorLifeState.Alive },
                    new ActorState { InstanceId = 2, DefinitionId = Enemy, IsHero = false,
                        Hp = GameNumber.FromInt64(100), MaxHp = GameNumber.FromInt64(100), Life = ActorLifeState.Alive }
                }), versions, SessionRandomState.Create(55), new OfflineAccountingState());
            var navigationSession = new GameSession(activeState, catalog);
            Assert.That(navigationSession.Execute(new SelectStageCommand("abandon", 0, Stage)).Status,
                Is.EqualTo(CommandStatus.Applied));
            Assert.That(navigationSession.CaptureAtSafePoint().Combat.Active, Is.False);
            Assert.That(navigationSession.CaptureAtSafePoint().Combat.EncounterRewardClaimed, Is.False);
        }

        [Test]
        public void FixedPowerOfferSurvivesSaveAndChoiceConsumesOnlyItsCredit()
        {
            var first = new ContentId("test.power.first");
            var second = new ContentId("test.power.second");
            var choice = new ContentId("test.choice.fixed");
            var later = new ContentId("test.choice.later");
            var catalog = new ContentCatalog(
                new[] { new LocationDefinition(Location, new[] { Stage }, 100,
                    new[] { new ProgressMilestone(choice, 25, ProgressMilestoneKind.PowerChoice, default,
                        false, new[] { first, second }),
                        new ProgressMilestone(later, 50, ProgressMilestoneKind.PowerChoice, default, false) }) },
                new[] { new StageDefinition(Stage, Location, new[] { Encounter }, 3, false) },
                new[] { new EncounterDefinition(Encounter, new[] { Enemy }, GameNumber.FromInt64(10), default) },
                new[] { new EnemyDefinition(Enemy, GameNumber.FromInt64(100), GameNumber.One,
                    GameNumber.FromInt64(2), SimDuration.FromSeconds(1), Array.Empty<string>(), default) },
                Array.Empty<EffectDefinition>(),
                new[] { new PowerDefinition(first, Array.Empty<ContentId>()),
                    new PowerDefinition(second, Array.Empty<ContentId>()) },
                Array.Empty<ItemDefinition>(), Array.Empty<LootTableDefinition>(), Stage);
            var account = new AccountState("profile");
            account.UnlockedPowers.Add(first); account.UnlockedPowers.Add(second);
            var run = new RunState("run", Stage);
            run.TriggeredMilestones.Add(choice); run.TriggeredMilestones.Add(later);
            run.PendingPowerChoiceMilestones.Add(choice); run.PendingPowerChoiceMilestones.Add(later);
            var versions = new VersionStamp(1, 1, 1, Pcg32.AlgorithmVersion, "test-content");
            var session = new GameSession(new GameState(account, run, new CombatState(), versions,
                SessionRandomState.Create(55), new OfflineAccountingState()), catalog);
            var beforeRng = session.CaptureAtSafePoint().Random.Offers.State;
            var offer = session.GenerateNextPowerOffer();
            Assert.That(offer.Generated, Is.True);
            Assert.That(offer.Options, Is.EqualTo(new[] { first, second }));
            Assert.That(session.CaptureAtSafePoint().Random.Offers.State, Is.EqualTo(beforeRng));
            Assert.That(session.GenerateNextPowerOffer().Generated, Is.False);
            var codec = new UnityJsonSaveCodec();
            var restored = codec.Decode(codec.Encode(session.CaptureAtSafePoint(), catalog), catalog, versions);
            var loaded = new GameSession(restored, catalog);
            Assert.That(loaded.GenerateNextPowerOffer().Options, Is.EqualTo(new[] { first, second }));
            Assert.That(loaded.Execute(new ChoosePowerCommand("invalid", offer.Revision, new ContentId("test.power.no"))).Status,
                Is.EqualTo(CommandStatus.Rejected));
            Assert.That(loaded.Execute(new ChoosePowerCommand("choose", offer.Revision, first)).Status,
                Is.EqualTo(CommandStatus.Applied));
            Assert.That(loaded.Execute(new ChoosePowerCommand("choose", offer.Revision, first)).Status,
                Is.EqualTo(CommandStatus.AlreadyApplied));
            var after = loaded.CaptureAtSafePoint();
            Assert.That(after.Run.OwnedPowers.Contains(first), Is.True);
            Assert.That(after.Run.PendingPowerChoiceMilestones, Is.EqualTo(new[] { later }));
            Assert.That(after.Run.PendingPowerOffer, Is.Empty);
            Assert.That(after.Run.ActivePowerOfferMilestoneId.Value, Is.Null.Or.Empty);
            Assert.That(loaded.GenerateNextPowerOffer(new FirstEligibleOfferPolicy()).Options,
                Is.EqualTo(new[] { second }));
        }

        private static CombatState ClearedEncounter(ContentId id) => new CombatState(id, new SimTime(0), new[]
        {
            new ActorState { InstanceId = 1, DefinitionId = new ContentId("hero"), IsHero = true,
                Hp = GameNumber.FromInt64(100), MaxHp = GameNumber.FromInt64(100), Life = ActorLifeState.Alive },
            new ActorState { InstanceId = 2, DefinitionId = Enemy, IsHero = false,
                Hp = GameNumber.Zero, MaxHp = GameNumber.FromInt64(100), Life = ActorLifeState.Dead }
        }, false);

        private static KeyValuePair<ContentId, BalanceValue> Pair(string id, BalanceValueKind kind, string coefficient, string exponent) =>
            new KeyValuePair<ContentId, BalanceValue>(new ContentId(id), new BalanceValue(kind, GameNumber.Parse(coefficient, exponent)));

        private static GameSession NewSession(ContentCatalog catalog = null, VersionStamp? versions = null)
        {
            catalog = catalog ?? Catalog();
            var selected = versions ?? new VersionStamp(1, 1, 1, Pcg32.AlgorithmVersion, "test-content");
            return new GameSession(new GameState(new AccountState("profile-1"), new RunState("run-1", Stage),
                new CombatState(), selected, SessionRandomState.Create(42), new OfflineAccountingState()), catalog);
        }

        private static ContentCatalog Catalog(BalanceCatalog balance = null) => new ContentCatalog(
            new[] { new LocationDefinition(Location, new[] { Stage }) },
            new[] { new StageDefinition(Stage, Location, new[] { Encounter }, 3, false) },
            new[] { new EncounterDefinition(Encounter, new[] { Enemy }, GameNumber.FromInt64(10), default) },
            new[] { new EnemyDefinition(Enemy, GameNumber.FromInt64(100), GameNumber.One, GameNumber.FromInt64(2),
                SimDuration.FromSeconds(1), Array.Empty<string>(), default) },
            Array.Empty<EffectDefinition>(), Array.Empty<PowerDefinition>(), Array.Empty<ItemDefinition>(), Array.Empty<LootTableDefinition>(), Stage, balance);

        private sealed class RecordingHandler : IScheduledEventHandler
        {
            public readonly List<ulong> Sequence = new List<ulong>();
            public void AdvanceContinuous(SimTime from, SimTime to) { }
            public void Handle(ScheduledEvent scheduled, SimulationScheduler scheduler) => Sequence.Add(scheduled.Sequence);
        }

        private sealed class FixedRandomStream : IRandomStream
        {
            private readonly double _hitRoll;
            private readonly uint _damageRoll;
            public int DamageRolls { get; private set; }
            public FixedRandomStream(double hitRoll, uint damageRoll) { _hitRoll = hitRoll; _damageRoll = damageRoll; }
            public double NextUnitDouble() => _hitRoll;
            public uint NextUInt32() { DamageRolls++; return _damageRoll; }
            public uint NextBounded(uint exclusiveUpperBound) => throw new NotSupportedException();
        }

        private sealed class ConstantStatsProvider : ICombatStatsProvider
        {
            private readonly DerivedStats _stats;
            public ConstantStatsProvider(DerivedStats stats) => _stats = stats;
            public DerivedStats Resolve(ActorState actor) => _stats;
        }

        private sealed class PerActorStatsProvider : ICombatStatsProvider
        {
            private readonly DerivedStats _hero;
            private readonly DerivedStats _enemy;
            public PerActorStatsProvider(DerivedStats hero, DerivedStats enemy) { _hero = hero; _enemy = enemy; }
            public DerivedStats Resolve(ActorState actor) => actor.IsHero ? _hero : _enemy;
        }

        private sealed class FirstOpposingTargetPolicy : ICombatTargetPolicy
        {
            public ulong ChooseTarget(ActorState attacker, IReadOnlyList<ActorState> actors)
            {
                foreach (var actor in actors)
                    if (actor.IsHero != attacker.IsHero && actor.Life == ActorLifeState.Alive) return actor.InstanceId;
                throw new InvalidOperationException("No opposing actor remains.");
            }
        }

        private sealed class FirstEligibleOfferPolicy : IPowerOfferPolicy
        {
            public IReadOnlyList<ContentId> CreateOffer(ProgressMilestone milestone,
                IReadOnlyList<ContentId> eligiblePowers, IRandomStream random) =>
                new[] { eligiblePowers[0] };
        }
    }
}
