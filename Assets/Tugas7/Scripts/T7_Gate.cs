using UnityEngine;

namespace Tugas7
{
    public sealed class T7_Gate : MonoBehaviour
    {
        [SerializeField] private Vector3 closedLocalPosition;
        [SerializeField] private Vector3 openLocalPosition = new(0f, 4f, 0f);
        [SerializeField, Min(0.1f)] private float movementSpeed = 3f;
        [SerializeField] private T7_PressurePlate pressurePlate;
        public bool IsOpen { get; private set; }
        public Vector3 TargetLocalPosition => IsOpen ? openLocalPosition : closedLocalPosition;

        private void Awake()
        {
            if (closedLocalPosition == Vector3.zero) closedLocalPosition = transform.localPosition;
        }

        private void OnEnable()
        {
            if (pressurePlate == null) return;
            pressurePlate.Pressed += Open;
            pressurePlate.Released += Close;
            SetOpen(pressurePlate.IsPressed);
        }

        private void OnDisable()
        {
            if (pressurePlate == null) return;
            pressurePlate.Pressed -= Open;
            pressurePlate.Released -= Close;
        }

        public void Configure(Vector3 closed, Vector3 open, float speed)
        {
            closedLocalPosition = closed;
            openLocalPosition = open;
            movementSpeed = Mathf.Max(0.1f, speed);
        }

        public void Bind(T7_PressurePlate plate)
        {
            if (isActiveAndEnabled && pressurePlate != null)
            {
                pressurePlate.Pressed -= Open;
                pressurePlate.Released -= Close;
            }
            pressurePlate = plate;
            if (isActiveAndEnabled)
            {
                pressurePlate.Pressed += Open;
                pressurePlate.Released += Close;
            }
            SetOpen(plate.IsPressed);
        }

        public void SetOpen(bool open) => IsOpen = open;
        public void Open() => SetOpen(true);
        public void Close() => SetOpen(false);

        private void Update() =>
            transform.localPosition = Vector3.MoveTowards(transform.localPosition,
                TargetLocalPosition, movementSpeed * Time.deltaTime);
    }
}
