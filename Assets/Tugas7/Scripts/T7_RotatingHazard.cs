using UnityEngine;

namespace Tugas7
{
    public sealed class T7_RotatingHazard : MonoBehaviour
    {
        [SerializeField] private Vector3 localAxis = Vector3.up;
        [SerializeField] private float degreesPerSecond = 75f;

        public void Configure(Vector3 axis, float speed)
        {
            localAxis = axis.normalized;
            degreesPerSecond = speed;
        }

        private void FixedUpdate() =>
            transform.Rotate(localAxis, degreesPerSecond * Time.fixedDeltaTime, Space.Self);
    }
}
