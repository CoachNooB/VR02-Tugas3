using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration = 45f;
    public float reverseAcceleration = 40f;
    public float maxSpeed = 125f;
    public float turnSpeed = 80f;
    public float brakeForce = 45f;

    [Header("Grip")]
    public float groundDrag = 1f;
    public float airDrag = 0.1f;
    public float sideGrip = 6f;

    [Header("Ground Check")]
    public float groundCheckDistance = 2f;
    public LayerMask groundLayer;

    [Header("Slope / Downforce")]
    public float downForce = 10f;
    public float slopeAlignSpeed = 8f;

    private Rigidbody rb;

    private Vector2 moveInput;
    private bool brakePressed;

    private bool isGrounded;
    private RaycastHit groundHit;
    private Vector3 groundNormal = Vector3.up;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = true;
        rb.isKinematic = false;

        rb.mass = 1000f;
        rb.linearDamping = groundDrag;
        rb.angularDamping = 2f;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Lower center of mass helps prevent flipping.
        rb.centerOfMass = new Vector3(0f, -0.7f, 0f);
    }

    private void FixedUpdate()
    {
        CheckGround();
        ApplyDownForce();
        Drive();
        Steer();
        AlignToSlope();
        Brake();
        LimitSpeed();
        ApplyDrag();
        ApplySideGrip();
    }

    private void CheckGround()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            groundNormal = Vector3.up;
            return;
        }

        isGrounded = Physics.Raycast(
            groundCheck.position,
            Vector3.down,
            out groundHit,
            groundCheckDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        groundNormal = isGrounded ? groundHit.normal : Vector3.up;

        Debug.DrawRay(
            groundCheck.position,
            Vector3.down * groundCheckDistance,
            isGrounded ? Color.green : Color.red
        );

        if (isGrounded)
        {
            Debug.DrawRay(
                groundHit.point,
                groundNormal * 2f,
                Color.magenta
            );
        }
    }

    private void ApplyDownForce()
    {
        if (!isGrounded)
        {
            return;
        }

        rb.AddForce(-groundNormal * downForce, ForceMode.Acceleration);
    }

    private void Drive()
    {
        float throttle = moveInput.y;

        if (Mathf.Abs(throttle) < 0.01f)
        {
            return;
        }

        if (!isGrounded)
        {
            return;
        }

        Vector3 driveForward = Vector3.ProjectOnPlane(transform.forward, groundNormal);

        if (driveForward.sqrMagnitude < 0.01f)
        {
            driveForward = transform.forward;
        }

        driveForward.Normalize();

        float power = throttle > 0f ? acceleration : reverseAcceleration;

        Vector3 force = driveForward * throttle * power;

        rb.AddForce(force, ForceMode.Acceleration);

        Debug.DrawRay(
            transform.position + Vector3.up * 0.5f,
            force.normalized * 5f,
            Color.blue
        );
    }

    private void Steer()
    {
        if (!isGrounded)
        {
            return;
        }

        float steering = moveInput.x;

        if (Mathf.Abs(steering) < 0.01f)
        {
            return;
        }

        float speedFactor = Mathf.Clamp01(rb.linearVelocity.magnitude / 5f);

        if (speedFactor <= 0.01f)
        {
            return;
        }

        float turnAmount = steering * turnSpeed * speedFactor * Time.fixedDeltaTime;

        Quaternion turnRotation = Quaternion.AngleAxis(turnAmount, groundNormal);

        rb.MoveRotation(turnRotation * rb.rotation);
    }

    private void AlignToSlope()
    {
        if (!isGrounded)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.FromToRotation(transform.up, groundNormal) * rb.rotation;

        rb.MoveRotation(
            Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                slopeAlignSpeed * Time.fixedDeltaTime
            )
        );
    }

    private void Brake()
    {
        if (!isGrounded || !brakePressed)
        {
            return;
        }

        Vector3 forwardVelocity = Vector3.Project(rb.linearVelocity, transform.forward);

        rb.AddForce(
            -forwardVelocity * brakeForce,
            ForceMode.Acceleration
        );
    }

    private void LimitSpeed()
    {
        Vector3 flatVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        if (flatVelocity.magnitude > maxSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * maxSpeed;

            rb.linearVelocity = new Vector3(
                limitedVelocity.x,
                rb.linearVelocity.y,
                limitedVelocity.z
            );
        }
    }

    private void ApplyDrag()
    {
        rb.linearDamping = isGrounded ? groundDrag : airDrag;
    }

    private void ApplySideGrip()
    {
        if (!isGrounded)
        {
            return;
        }

        Vector3 forwardVelocity = Vector3.Project(rb.linearVelocity, transform.forward);
        Vector3 sideVelocity = Vector3.Project(rb.linearVelocity, transform.right);

        rb.linearVelocity = forwardVelocity + sideVelocity / sideGrip;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnBrake(InputValue value)
    {
        brakePressed = value.isPressed;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            rayStart,
            rayStart + Vector3.down * groundCheckDistance
        );
    }
}