using UnityEngine;

namespace Tugas7
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class T7_MovingPlatform : MonoBehaviour
    {
        [SerializeField] private Vector3 localPointA;
        [SerializeField] private Vector3 localPointB = new(4f, 0f, 0f);
        [SerializeField, Min(0.1f)] private float travelDuration = 3f;
        private Vector3 origin;
        private Rigidbody body;

        private void Awake()
        {
            origin = transform.localPosition;
            body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public void Configure(Vector3 pointA, Vector3 pointB, float duration)
        {
            localPointA = pointA;
            localPointB = pointB;
            travelDuration = Mathf.Max(0.1f, duration);
        }

        private void FixedUpdate()
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.PingPong(Time.time / travelDuration, 1f));
            Vector3 localTarget = origin + Vector3.Lerp(localPointA, localPointB, t);
            Vector3 target = transform.parent != null ? transform.parent.TransformPoint(localTarget) : localTarget;
            body.MovePosition(target);
        }
    }
}
