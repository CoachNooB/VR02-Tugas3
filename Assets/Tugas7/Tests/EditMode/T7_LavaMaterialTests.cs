using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Tugas7.Tests
{
    public class T7_LavaMaterialTests
    {
        [Test]
        public void AnimatedLavaMaterialExposesRequiredProperties()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Tugas7/Materials/T7_AnimatedLava.mat");
            Assert.That(material, Is.Not.Null);
            string[] properties =
            {
                "_BaseMap", "_EmissionMap", "_NormalMap", "_HeightMap", "_RoughnessMap", "_AOMap",
                "_FlowSpeedA", "_FlowSpeedB", "_Tiling", "_EmissionIntensity", "_NormalStrength",
                "_DistortionStrength", "_DisplacementAmplitude", "_CrustColor", "_HotColor"
            };
            foreach (string property in properties)
                Assert.That(material.HasProperty(property), Is.True, property);
        }
    }
}
