using UnityEngine;

public class ZombieAI : MonoBehaviour
{
    public float walkSpeed = 2f;
    public float detectionRange = 15f;
    public float attackRange = 1.5f;
    public int damage = 10;

    private Transform player;
    private Rigidbody rb;
    private Animator animator;
    private bool isDead = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>(); // Ambil Animator dari model
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 80f;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Set parameter awal
        if (animator != null)
            animator.SetBool("isWalking", false);
    }

    void FixedUpdate()
    {
        if (isDead || player == null) return;

        Vector3 direction = player.position - transform.position;
        float distance = direction.magnitude;

        if (distance > detectionRange)
        {
            // Diam
            if (animator != null) animator.SetBool("isWalking", false);
            return;
        }

        if (distance < attackRange)
        {
            // Serang (berhenti)
            if (animator != null)
            {
                animator.SetBool("isWalking", false);
                animator.SetBool("isAttacking", true);
            }
            // Tambahkan logika serang jika perlu
            return;
        }

        // Bergerak
        Vector3 move = direction.normalized * walkSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        // Rotasi
        Vector3 lookDir = direction.normalized;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * 5f));
        }

        if (animator != null)
        {
            animator.SetBool("isWalking", true);
            animator.SetBool("isAttacking", false);
        }
    }

    public void TakeDamage(float force, Vector3 hitPoint, Vector3 direction)
    {
        rb.AddForceAtPosition(direction * force, hitPoint, ForceMode.Impulse);
    }

    public void Die()
    {
        isDead = true;
        if (animator != null) animator.SetBool("isWalking", false);
        // Mati
    }
}