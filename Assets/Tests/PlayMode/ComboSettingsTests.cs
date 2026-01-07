using Combat.Data;
using NUnit.Framework;
using UnityEngine;

namespace Tests.PlayMode
{
    [TestFixture]
    public class ComboSettingsTests
    {
        private ComboSettings _comboSettings;

        [SetUp]
        public void SetUp()
        {
            _comboSettings = ComboSettings.CreateForTest(new float[] { 1.0f, 1.1f, 1.3f });
        }

        [TearDown]
        public void TearDown()
        {
            if (_comboSettings != null)
            {
                Object.DestroyImmediate(_comboSettings);
            }
        }

        #region GetComboMultiplier Tests

        [Test]
        public void GetComboMultiplier_ValidStep_ReturnsCorrectValue()
        {
            Assert.AreEqual(1.0f, _comboSettings.GetComboMultiplier(1));
            Assert.AreEqual(1.1f, _comboSettings.GetComboMultiplier(2));
            Assert.AreEqual(1.3f, _comboSettings.GetComboMultiplier(3));
        }

        [Test]
        public void GetComboMultiplier_StepBeyondArrayLength_ClampsToLastElement()
        {
            float result = _comboSettings.GetComboMultiplier(100);

            Assert.AreEqual(1.3f, result);
        }

        [Test]
        public void GetComboMultiplier_ZeroStep_ReturnsOne()
        {
            float result = _comboSettings.GetComboMultiplier(0);

            Assert.AreEqual(1f, result);
        }

        [Test]
        public void GetComboMultiplier_NullOrEmptyArray_ReturnsOne()
        {
            var emptySettings = ComboSettings.CreateForTest(new float[0]);

            float result = emptySettings.GetComboMultiplier(1);

            Assert.AreEqual(1f, result);

            Object.DestroyImmediate(emptySettings);
        }

        #endregion
    }
}
