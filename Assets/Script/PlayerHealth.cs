using System.Collections;
using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 10;
    private int currentHealth;

    [Header("Damage Visual Feedback")]
    [SerializeField] private float hitFlashDuration = 0.5f;
    [SerializeField] private Color hitFlashColor = Color.red;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private GameObject deathPanel;

    [Header("Extra Trigger")]
    [Tooltip("Panel juga aktif jika objek ini dihancurkan.")]
    [SerializeField] private GameObject importantTarget;  // Tambahkan ini

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isInvulnerable = false;
    private Animator animator;
    private bool hasTriggeredPanel = false;

    public System.Action OnPlayerDeath;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator       = GetComponent<Animator>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        currentHealth = maxHealth;
        UpdateHealthUI();

        if (deathPanel != null)
            deathPanel.SetActive(false);
    }

    private void Update()
    {
        // Cek jika objek target sudah dihancurkan
        if (!hasTriggeredPanel && importantTarget == null)
        {
            Debug.Log("Important target destroyed!");
            hasTriggeredPanel = true;
            ActivateDeathPanel();
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable) return;

        currentHealth -= damage;
        UpdateHealthUI();

        animator?.SetTrigger("Hit");
        StartCoroutine(FlashRoutine());

        if (currentHealth <= 0)
            Die();
    }

    private IEnumerator FlashRoutine()
    {
        isInvulnerable = true;
        spriteRenderer.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashDuration);
        spriteRenderer.color = originalColor;
        isInvulnerable = false;
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = $"{currentHealth} / {maxHealth}";
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthUI();
    }

    private void Die()
    {
        Debug.Log("Player died!");
        animator?.SetTrigger("Die");
        OnPlayerDeath?.Invoke();

        this.enabled = false;

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        float dieAnimLength = 1.5f;
        yield return new WaitForSeconds(dieAnimLength);
        ActivateDeathPanel();
    }

    private void ActivateDeathPanel()
    {
        if (deathPanel != null)
            deathPanel.SetActive(true);
    }
}
