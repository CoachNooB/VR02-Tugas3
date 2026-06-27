using UnityEngine;

public class ZombieAI : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2f;
    public float detectionRange = 15f;
    public float attackRange = 1.8f;
    public int damage = 10;
    public float attackCooldown = 1.5f;

    private Transform player;
    private Rigidbody rb;
    private Animator animator;
    private bool isDead = false;
    private bool isAttacking = false;
    private float attackTimer = 0f;
    private PlayerSystemController playerHealth;

    // Parameter animator
    private bool hasWalkingParam = false;
    private bool hasAttackingParam = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // Cari PlayerSystemController di player (atau di anaknya)
        if (player != null)
        {
            playerHealth = player.GetComponentInChildren<PlayerSystemController>();
            if (playerHealth == null)
                playerHealth = player.GetComponent<PlayerSystemController>();
        }

        // Cek parameter animator
        if (animator != null)
        {
            foreach (var param in animator.parameters)
            {
                if (param.name == "isWalking") hasWalkingParam = true;
                if (param.name == "isAttacking") hasAttackingParam = true;
            }
            
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
        // Cooldown serangan
        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                isAttacking = false;
                if (animator != null && hasAttackingParam)
                    animator.SetBool("isAttacking", false);
            }
        }
    }

    void FixedUpdate()
    {
        if (isDead || player == null) return;

        Vector3 direction = player.position - transform.position;
        float distance = direction.magnitude;

        // Jika terlalu jauh, diam
        if (distance > detectionRange)
        {
            if (animator != null && hasWalkingParam) 
                animator.SetBool("isWalking", false);
            return;
        }

        // Jika jarak dekat dan tidak sedang menyerang, mulai serang
        if (distance < attackRange && !isAttacking && !isDead)
        {
            isAttacking = true;
            attackTimer = attackCooldown;
            if (animator != null)
            {
                if (hasWalkingParam) animator.SetBool("isWalking", false);
                if (hasAttackingParam) animator.SetBool("isAttacking", true);
            }

            // Serang player! Kurangi health
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log("Zombie menyerang! Damage: " + damage);
            }
            return;
        }

        // Jika sedang menyerang, jangan bergerak
        if (isAttacking)
        {
            if (animator != null && hasWalkingParam) animator.SetBool("isWalking", false);
            return;
        }

        // ===== BERJALAN MENDEKATI PLAYER =====
        Vector3 move = direction.normalized * walkSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        // Rotasi menghadap player
        Vector3 lookDir = direction.normalized;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * 5f));
        }

        // Aktifkan animasi berjalan
        if (animator != null)
        {
            if (hasWalkingParam) animator.SetBool("isWalking", true);
            if (hasAttackingParam) animator.SetBool("isAttacking", false);
        }
    }

    public void TakeDamage(float force, Vector3 hitPoint, Vector3 direction)
    {
        if (isDead) return;

        rb.AddForceAtPosition(direction * force, hitPoint, ForceMode.Impulse);
        Die();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        if (animator != null)
        {
            if (hasWalkingParam) animator.SetBool("isWalking", false);
            if (hasAttackingParam) animator.SetBool("isAttacking", false);
        }
        Destroy(gameObject, 2f);
    }
}