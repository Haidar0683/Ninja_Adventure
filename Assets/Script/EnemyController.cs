using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 9.39f;
    [SerializeField] private Transform target;
    [SerializeField] private bool alwaysChaseTarget = true;
    
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 1.5f;
    [SerializeField] private LayerMask playerLayer;
    private float attackTimer = 0f;
    private bool isAttacking = false;
    
    [Header("Ground Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private Transform groundCheck;
    
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 movement;
    private bool isMoving = false;
    private bool isGrounded = false;
    
    private readonly string walkAnimParam = "d_walk";
    private readonly string idleAnimParam = "d_idle";
    private readonly string attackAnimParam = "d_cleave";

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // If groundCheck is not assigned, create one
        if (groundCheck == null)
        {
            // Create a child object for ground check
            GameObject checkObject = new GameObject("GroundCheck");
            checkObject.transform.parent = transform;
            // Position it at the bottom of your capsule collider
            checkObject.transform.localPosition = new Vector3(0, -0.45f, 0); 
            groundCheck = checkObject.transform;
        }
        
       // If attackPoint is not assigned, create one
        if (attackPoint == null)
        {
        GameObject attackObj = new GameObject("AttackPoint");
        attackObj.transform.parent = transform;
    // Position it at the point where the attack reaches using attackRange
        attackObj.transform.localPosition = new Vector3(attackRange, 0, 0); 
        attackPoint = attackObj.transform;
}
        
        // If no target is assigned, find the player
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }

    private void Update()
    {
        // Check if the enemy is grounded
        CheckGrounded();
        
        if (target == null)
            return;
            
        // Calculate distance to target
        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        
        // Handle attack timer
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;
        
        // Check if player is within attack range and cooldown is ready
        if (distanceToTarget <= attackRange && attackTimer <= 0 && isGrounded)
        {
            // Perform attack
            Attack();
        }
        // Check if target is within detection range but outside attack range
        else if ((alwaysChaseTarget || distanceToTarget <= detectionRange) && distanceToTarget > attackRange && isGrounded && !isAttacking)
        {
            // Only move horizontally, not vertically
            Vector2 direction = (target.position - transform.position).normalized;
            movement = new Vector2(direction.x, 0);
            isMoving = true;
            
            // Flip sprite based on movement direction
            if (spriteRenderer != null)
            {
                if (direction.x > 0)
                    spriteRenderer.flipX = false;
                else if (direction.x < 0)
                    spriteRenderer.flipX = true;
            }
            
            // Update attack point position based on facing direction
            UpdateAttackPointPosition();
        }
        else if (!isAttacking)
        {
            // Stop moving if target is out of range or not grounded
            movement = Vector2.zero;
            isMoving = false;
        }
        
        // Update animations
        UpdateAnimations();
    }
    
    private void UpdateAttackPointPosition()
    {
        if (attackPoint == null) return;
        
        // Update attack point position based on facing direction
        float xOffset = 0.6f;
        Vector3 localPos = attackPoint.localPosition;
        
        if (spriteRenderer.flipX)
        {
            // Facing left
            attackPoint.localPosition = new Vector3(Mathf.Abs(xOffset), localPos.y, localPos.z);
        }
        else
        {
            // Facing right
            attackPoint.localPosition = new Vector3(-Mathf.Abs(xOffset), localPos.y, localPos.z);
        }
    }
    
    private void FixedUpdate()
    {
        // Only move when grounded and not attacking
        if (isMoving && isGrounded && !isAttacking)
        {
            // Apply horizontal movement
            rb.velocity = new Vector2(movement.x * moveSpeed, rb.velocity.y);
        }
        else
        {
            // Stop horizontal movement when not moving or attacking
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        
        // Apply an extra downward force to keep the enemy grounded
        if (isGrounded)
        {
            rb.AddForce(Vector2.down * 5f);
        }
    }
    
    private void Attack()
    {
        // Start attack
        isAttacking = true;
        isMoving = false;
        movement = Vector2.zero;
        
        // Play attack animation
        animator.SetTrigger(attackAnimParam);
        
        // Ensure enemy is facing the target
        if (target.position.x > transform.position.x)
        {
            spriteRenderer.flipX = false;
        }
        else
        {
            spriteRenderer.flipX = true;
        }
        
        // Update attack point position
        UpdateAttackPointPosition();
        
        // Reset attack timer
        attackTimer = attackCooldown;
        
        // Schedule the end of attack animation
        Invoke("EndAttack", 1f); // Adjust time based on your animation length
    }
    
    // Called by animation event during the cleave animation
    public void DealDamage()
    {
        // Check for player in attack range
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);
        
        foreach (Collider2D playerCollider in hitPlayers)
        {
            // Get player health component and deal damage
            PlayerHealth playerHealth = playerCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log("Player hit for " + attackDamage + " damage!");
            }
        }
    }
    
    private void EndAttack()
    {
        isAttacking = false;
    }
    
    private void CheckGrounded()
    {
        // Using OverlapCircle for more reliable ground detection
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckDistance, groundLayer);
    }
    
    private void UpdateAnimations()
    {
        if (animator == null)
            return;
        
        // Attack animation is handled by triggers in the Attack() method
        
        // Don't change animations during attack
        if (isAttacking)
            return;
            
        // Set walking animation when moving
        if (isMoving && isGrounded)
        {
            animator.SetBool(walkAnimParam, true);
            animator.SetBool(idleAnimParam, false);
        }
        else
        {
            animator.SetBool(walkAnimParam, false);
            animator.SetBool(idleAnimParam, true);
        }
    }
    
    // Visualize the ground check and attack range
    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckDistance);
        }
        
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw attack point and radius
        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}