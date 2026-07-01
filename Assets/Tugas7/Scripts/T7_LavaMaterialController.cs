using UnityEngine;

namespace Tugas7
{
    public sealed class T7_LavaMaterialController : MonoBehaviour
    {
        private static readonly int FlowOffsetA = Shader.PropertyToID("_FlowOffsetA");
        private static readonly int FlowOffsetB = Shader.PropertyToID("_FlowOffsetB");

        [SerializeField] private Material sharedMaterial;
        [SerializeField] private Vector2 flowSpeedA = new(0.025f, 0.01f);
        [SerializeField] private Vector2 flowSpeedB = new(-0.012f, 0.02f);

        public void Configure(Material material) => sharedMaterial = material;

        private void Update()
        {
            if (sharedMaterial == null)
                return;
            float time = Time.time;
            sharedMaterial.SetVector(FlowOffsetA, flowSpeedA * time);
            sharedMaterial.SetVector(FlowOffsetB, flowSpeedB * time);
        }
    }
}
