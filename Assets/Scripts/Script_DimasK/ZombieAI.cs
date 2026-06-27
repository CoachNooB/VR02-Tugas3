using UnityEngine;

public class ZombieAI : MonoBehaviour
{
    public float walkSpeed = 2f;
    public float detectionRange = 15f;
    public float attackRange = 1.5f;
    public int damage = 10;
    public float deathDelay = 3f; // waktu sebelum zombie hilang setelah mati

    private Transform player;
    private Rigidbody rb;
    private Animator animator;
    private bool isDead = false;
    private float deathTimer = 0f;

    // Cek parameter yang tersedia di Animator
    private bool hasWalkingParam = false;
    private bool hasAttackingParam = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // Cek parameter yang tersedia di Animator
        if (animator != null)
        {
            foreach (var param in animator.parameters)
            {
                if (param.name == "isWalking") hasWalkingParam = true;
                if (param.name == "isAttacking") hasAttackingParam = true;
            }
            
            // Set parameter awal hanya jika ada
            if (hasWalkingParam) animator.SetBool("isWalking", false);
            if (hasAttackingParam) animator.SetBool("isAttacking", false);
        }

        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 80f;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Update()
    {
        // Jika zombie mati, jalankan timer untuk menghilang
        if (isDead)
        {
            deathTimer += Time.deltaTime;
            if (deathTimer >= deathDelay)
            {
                Destroy(gameObject);
            }
            return;
        }
    }

    void FixedUpdate()
    {
        if (isDead || player == null) return;

        Vector3 direction = player.position - transform.position;
        float distance = direction.magnitude;

        if (distance > detectionRange)
        {
            // Diam
            if (animator != null && hasWalkingParam) 
                animator.SetBool("isWalking", false);
            return;
        }

        if (distance < attackRange)
        {
            // Serang (berhenti)
            if (animator != null)
            {
                if (hasWalkingParam) animator.SetBool("isWalking", false);
                if (hasAttackingParam) animator.SetBool("isAttacking", true);
            }
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
            if (hasWalkingParam) animator.SetBool("isWalking", true);
            if (hasAttackingParam) animator.SetBool("isAttacking", false);
        }
    }

    public void TakeDamage(float force, Vector3 hitPoint, Vector3 direction)
    {
        if (isDead) return; // Jangan terima damage jika sudah mati

        // Beri dorongan
        rb.AddForceAtPosition(direction * force, hitPoint, ForceMode.Impulse);
        
        // Matikan zombie
        Die();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        
        // Matikan animasi
        if (animator != null)
        {
            if (hasWalkingParam) animator.SetBool("isWalking", false);
            if (hasAttackingParam) animator.SetBool("isAttacking", false);
        }
        
        // Nonaktifkan collider agar tidak menghalangi
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        // Biarkan rigidbody tetap aktif agar efek dorongan terlihat
        // tapi kita tidak ingin zombie bergerak sendiri lagi
        // (sudah diatasi dengan isDead di FixedUpdate)
        
        // Mulai timer untuk menghilang (di Update)
        deathTimer = 0f;
    }
}