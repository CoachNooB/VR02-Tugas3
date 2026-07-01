using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Tugas7.Tests
{
    public class T7_UITests
    {
        private GameObject root;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("HUD");
            root.AddComponent<RectTransform>();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(root);

        [Test]
        public void HudForcesWorldSpaceCanvas()
        {
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var hud = root.AddComponent<T7_SpatialFeedbackUI>();
            hud.EnsureWorldSpace();
            Assert.That(canvas.renderMode, Is.EqualTo(RenderMode.WorldSpace));
        }

        [Test]
        public void DamageAndHealingSetExpectedFeedbackColors()
        {
            var hud = root.AddComponent<T7_SpatialFeedbackUI>();
            hud.ShowDamageFeedback(10f);
            Assert.That(hud.CurrentFeedbackColor.r, Is.GreaterThan(hud.CurrentFeedbackColor.g));
            Assert.That(hud.CurrentFeedbackColor.a, Is.LessThanOrEqualTo(0.15f));
            hud.ShowHealingFeedback(10f);
            Assert.That(hud.CurrentFeedbackColor.g, Is.GreaterThan(hud.CurrentFeedbackColor.r));
            Assert.That(hud.CurrentFeedbackColor.a, Is.LessThanOrEqualTo(0.15f));
        }

        [Test]
        public void HealthFillTracksCurrentHealthRatio()
        {
            root.AddComponent<Canvas>();
            var fillObject = new GameObject("HealthFill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fillObject.transform.SetParent(root.transform);
            var fill = fillObject.GetComponent<Image>();
            var hud = root.AddComponent<T7_SpatialFeedbackUI>();
            hud.Configure(null, fill, null, null, null, null, null, null);

            hud.SetHealth(35f, 100f);

            Assert.That(fill.fillAmount, Is.EqualTo(0.35f).Within(0.001f));
        }

        [Test]
        public void HudCanBePlacedAwayFromTheCrosshair()
        {
            var hud = root.AddComponent<T7_SpatialFeedbackUI>();
            hud.ConfigurePlacement(1.2f, new Vector2(-0.34f, 0.27f));

            Assert.That(hud.FollowOffset.x, Is.LessThan(0f));
            Assert.That(hud.FollowOffset.y, Is.GreaterThan(0f));
        }
    }
}
