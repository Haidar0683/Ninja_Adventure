using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Attack Settings")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private float attackRate = 1f;

    private Rigidbody2D rb;
    private Animator anim;
    private bool facingRight = true;
    private bool isGrounded;
    private bool isMoving;
    private float nextAttackTime = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        HandleMovement();
        HandleJump();
        HandleAttack();
    }

    private void FixedUpdate()
    {
        CheckGround();
    }

    private void HandleMovement()
    {
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.A)) moveInput = -1f;
        else if (Input.GetKey(KeyCode.D)) moveInput = 1f;

        isMoving = moveInput != 0f;
        anim?.SetBool("RunState", isMoving);

        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        if (moveInput > 0 && !facingRight)
        {
            facingRight = true;
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else if (moveInput < 0 && facingRight)
        {
            facingRight = false;
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
    }

   private void HandleJump()
{
    // Only allow jumping when grounded
    if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        anim?.SetTrigger("Jump");
        
        // Immediately set IsGrounded to false so we don't transition back too soon
        anim?.SetBool("IsGrounded", false);
    }
}
   private void HandleAttack()
{
    if (Time.time < nextAttackTime) return;
    
    if (Input.GetMouseButtonDown(0))
    {
        anim?.SetTrigger("Attack");
        
        // Call Attack() method during the animation using Animation Event
        // Or call it directly here if you prefer:
        Attack();
        
        nextAttackTime = Time.time + 1f / attackRate;
    }
}

// This will be called by the Animation Event
public void OnAttackAnimationHit()
{
    Attack();
}

   private void Attack()
{
    // Draw a circle at attackPoint position to detect enemies
    Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
    
    // Apply damage to detected enemies
    foreach (Collider2D enemy in hitEnemies)
    {
        enemy.GetComponent<EnemyHealth>()?.TakeDamage(attackDamage);
    }
}

 private void CheckGround() 
{
    isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundLayer);
    
    anim?.SetBool("IsGrounded", isGrounded);
    
}

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}

