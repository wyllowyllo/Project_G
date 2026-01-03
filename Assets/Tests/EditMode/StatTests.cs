using Combat.Core;
using NUnit.Framework;

namespace Tests.EditMode
{
    [TestFixture]
    public class StatTests
    {
        private Stat _stat;
        private MockModifierSource _source;

        [SetUp]
        public void SetUp()
        {
            _stat = new Stat(100f);
            _source = new MockModifierSource("TestSource");
        }

        #region Initial Value Tests

        [Test]
        public void Stat_InitialValue_ReturnsBaseValue()
        {
            var stat = new Stat(50f);

            Assert.AreEqual(50f, stat.Value);
        }

        [Test]
        public void Stat_SetBaseValue_UpdatesFinalValue()
        {
            _stat.BaseValue = 200f;

            Assert.AreEqual(200f, _stat.Value);
        }

        [Test]
        public void Stat_SetBaseValue_WithModifiers_RecalculatesCorrectly()
        {
            var modifier = new StatModifier(10f, StatModifierType.Additive, _source);
            _stat.AddModifier(modifier);

            _stat.BaseValue = 50f;

            Assert.AreEqual(60f, _stat.Value);
        }

        #endregion

        #region Additive Modifier Tests

        [Test]
        public void Stat_AddAdditiveModifier_IncreasesValue()
        {
            var modifier = new StatModifier(25f, StatModifierType.Additive, _source);

            _stat.AddModifier(modifier);

            Assert.AreEqual(125f, _stat.Value);
        }

        #endregion

        #region Multiplicative Modifier Tests

        [Test]
        public void Stat_AddMultiplicativeModifier_MultipliesValue()
        {
            var modifier = new StatModifier(0.5f, StatModifierType.Multiplicative, _source);

            _stat.AddModifier(modifier);

            Assert.AreEqual(150f, _stat.Value);
        }

        #endregion

        #region Multiple Modifiers Tests

        [Test]
        public void Stat_MultipleModifiers_CalculatesCorrectOrder()
        {
            var additive = new StatModifier(50f, StatModifierType.Additive, _source);
            var multiplicative = new StatModifier(0.2f, StatModifierType.Multiplicative, _source);

            _stat.AddModifier(additive);
            _stat.AddModifier(multiplicative);

            // (100 + 50) * (1 + 0.2) = 150 * 1.2 = 180
            Assert.AreEqual(180f, _stat.Value);
        }

        #endregion

        #region Null Modifier Tests

        [Test]
        public void Stat_AddNullModifier_LogsWarningAndReturns()
        {
            float valueBefore = _stat.Value;

            _stat.AddModifier(null);

            Assert.AreEqual(valueBefore, _stat.Value);
        }

        #endregion

        #region Remove Modifier Tests

        [Test]
        public void Stat_RemoveModifier_RestoresValue()
        {
            var modifier = new StatModifier(50f, StatModifierType.Additive, _source);
            _stat.AddModifier(modifier);

            bool removed = _stat.RemoveModifier(modifier);

            Assert.IsTrue(removed);
            Assert.AreEqual(100f, _stat.Value);
        }

        [Test]
        public void Stat_RemoveNonexistentModifier_ReturnsFalse()
        {
            var modifier = new StatModifier(50f, StatModifierType.Additive, _source);

            bool removed = _stat.RemoveModifier(modifier);

            Assert.IsFalse(removed);
        }

        [Test]
        public void Stat_RemoveModifiersBySource_RemovesOnlyTargetSource()
        {
            var source1 = new MockModifierSource("Source1");
            var source2 = new MockModifierSource("Source2");
            var modifier1 = new StatModifier(10f, StatModifierType.Additive, source1);
            var modifier2 = new StatModifier(20f, StatModifierType.Additive, source2);
            _stat.AddModifier(modifier1);
            _stat.AddModifier(modifier2);

            bool removed = _stat.RemoveAllModifiersFromSource(source1);

            Assert.IsTrue(removed);
            Assert.AreEqual(120f, _stat.Value);
        }

        #endregion

        #region Clear Modifiers Tests

        [Test]
        public void Stat_ClearModifiers_RestoresToBaseValue()
        {
            var additive = new StatModifier(50f, StatModifierType.Additive, _source);
            var multiplicative = new StatModifier(0.5f, StatModifierType.Multiplicative, _source);
            _stat.AddModifier(additive);
            _stat.AddModifier(multiplicative);

            _stat.ClearModifiers();

            Assert.AreEqual(100f, _stat.Value);
        }

        #endregion

        #region Clamp Tests

        [Test]
        public void Stat_Clamp_RespectsMinMaxBounds()
        {
            var stat = new Stat(50f, minValue: 0f, maxValue: 100f);
            var modifier = new StatModifier(100f, StatModifierType.Additive, _source);

            stat.AddModifier(modifier);

            Assert.AreEqual(100f, stat.Value);
        }

        #endregion

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
