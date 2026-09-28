using System;
using System.Numerics;
using IBM.Domain;
using IBM.Application;
using NUnit.Framework;

namespace IBM.Tests.EditMode
{
    public sealed class FoundationTests
    {
        [Test]
        public void Pcg32MatchesPublishedReferenceVectorAndRestoresState()
        {
            var random = new Pcg32(42, 54);
            Assert.That(random.NextUInt32(), Is.EqualTo(0xa15c02b7u));
            Assert.That(random.NextUInt32(), Is.EqualTo(0x7b47f409u));
            var restored = Pcg32.Restore(random.State, random.Increment);
            Assert.That(restored.NextUInt32(), Is.EqualTo(0xba1d3330u));
            Assert.That(random.NextUInt32(), Is.EqualTo(0xba1d3330u));
            Assert.That(random.NextUInt32(), Is.EqualTo(0x83d2f293u));
            Assert.That(random.NextUInt32(), Is.EqualTo(0xbfa4784bu));
        }

        [Test]
        public void StreamDerivationIsStableAndSeparate()
        {
            Assert.That(RandomSeeds.Derive(1, "combat"), Is.EqualTo(RandomSeeds.Derive(1, "combat")));
            Assert.That(RandomSeeds.Derive(1, "combat"), Is.Not.EqualTo(RandomSeeds.Derive(1, "loot")));
        }

        [Test]
        public void GameNumberHandlesHugeArithmeticWithoutDoubleConversion()
        {
            var huge = GameNumber.Parse("1", "1000");
            Assert.That(huge.Multiply(huge), Is.EqualTo(GameNumber.Parse("1", "2000")));
            Assert.That(huge.Divide(huge), Is.EqualTo(GameNumber.One));
            Assert.That(huge.Log2(), Is.EqualTo(1000 * Math.Log(10, 2)).Within(1e-10));
            Assert.Throws<OverflowException>(() => huge.ToBoundedDouble());
        }

        [Test]
        public void GameNumberCanonicalizesAndPreservesCloseSubtraction()
        {
            Assert.That(GameNumber.Parse("1000", "-3"), Is.EqualTo(GameNumber.One));
            var top = GameNumber.Parse("1000000000000000000000000000000001", "0");
            var bottom = GameNumber.Parse("1000000000000000000000000000000000", "0");
            Assert.That(top.Subtract(bottom), Is.EqualTo(GameNumber.One));
            Assert.That(GameNumber.Parse("1", "1000").Add(GameNumber.One), Is.EqualTo(GameNumber.Parse("1", "1000")));
        }

        [Test]
        public void GameNumberRoundsTiesToEvenAndRejectsInvalidOperations()
        {
            Assert.That(GameNumber.Create(BigInteger.Parse("10000000000000000000000000000000005"), 0),
                Is.EqualTo(GameNumber.Parse("1", "34")));
            Assert.That(GameNumber.Create(BigInteger.Parse("10000000000000000000000000000000015"), 0),
                Is.EqualTo(GameNumber.Parse("1000000000000000000000000000000002", "1")));
            Assert.Throws<DivideByZeroException>(() => GameNumber.One.Divide(GameNumber.Zero));
            Assert.Throws<OverflowException>(() => GameNumber.Parse("1", long.MaxValue.ToString()).Multiply(GameNumber.FromInt64(10)));
        }

        [Test]
        public void TimeConversionAndResidueAreExplicit()
        {
            Assert.That(SimDuration.FromSeconds(0.0000005).Microseconds, Is.EqualTo(1));
            var residue = new RealTimeResidue();
            Assert.That(residue.ConsumeSeconds(0.0000004), Is.EqualTo(0));
            Assert.That(residue.ConsumeSeconds(0.0000004), Is.EqualTo(0));
            Assert.That(residue.ConsumeSeconds(0.0000004), Is.EqualTo(1));
            Assert.Throws<OverflowException>(() => new SimTime(long.MaxValue).Add(new SimDuration(1)));
        }

        [Test]
        public void IdentifiersHaveDistinctDefinitionAndInstanceSemantics()
        {
            Assert.That(new ContentId("P001"), Is.EqualTo(new ContentId("P001")));
            Assert.That(new DefinitionId<PowerKind>("P001").Content, Is.EqualTo(new ContentId("P001")));
            Assert.Throws<ArgumentException>(() => new ContentId(" P001"));
            var sequence = new InstanceIdSequence(7);
            Assert.That(sequence.Next().Value, Is.EqualTo(7UL));
            Assert.That(sequence.Next().Value, Is.EqualTo(8UL));
        }

        [Test]
        public void VersionComponentsStayIndependent()
        {
            var original = new VersionStamp(1, 1, 1, Pcg32.AlgorithmVersion, "content-a");
            var balanceEdit = new VersionStamp(1, 1, 1, Pcg32.AlgorithmVersion, "content-b");
            Assert.That(original.Equals(balanceEdit), Is.False);
            Assert.Throws<ArgumentException>(() => new VersionStamp(1, 0, 1, 1, "content-a"));
        }
    }
}
