using System;
using Combat.Core;
using NUnit.Framework;

namespace Tests.EditMode
{
    [TestFixture]
    public class StatModifierTests
    {
        private MockModifierSource _source;

        [SetUp]
        public void SetUp()
        {
            _source = new MockModifierSource("TestSource");
        }

        [Test]
        public void StatModifier_Creation_StoresCorrectValues()
        {
            var modifier = new StatModifier(25f, StatModifierType.Multiplicative, _source);

            Assert.AreEqual(25f, modifier.Value);
            Assert.AreEqual(StatModifierType.Multiplicative, modifier.Type);
            Assert.AreEqual(_source.Id, modifier.SourceId);
        }

        [Test]
        public void StatModifier_NullSource_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new StatModifier(10f, StatModifierType.Additive, null);
            });
        }

        [Test]
        public void StatModifier_NaNValue_DefaultsToZero()
        {
            var modifier = new StatModifier(float.NaN, StatModifierType.Additive, _source);

            Assert.AreEqual(0f, modifier.Value);
        }

        [Test]
        public void StatModifier_InfinityValue_DefaultsToZero()
        {
            var positiveInfinity = new StatModifier(float.PositiveInfinity, StatModifierType.Additive, _source);
            var negativeInfinity = new StatModifier(float.NegativeInfinity, StatModifierType.Additive, _source);

            Assert.AreEqual(0f, positiveInfinity.Value);
            Assert.AreEqual(0f, negativeInfinity.Value);
        }

        #region Mock Classes

        private class MockModifierSource : IModifierSource
        {
            public string Id { get; }

            public MockModifierSource(string id)
            {
                Id = id;
            }
        }

        #endregion
    }
}
