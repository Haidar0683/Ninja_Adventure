using UnityEngine;
using TMPro;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Invul & Flash")]
    [SerializeField] private float flashDuration = 0.5f;
    [SerializeField] private float invulDuration  = 1f;

    [Header("UI (Screen‑Space)")]
    [Tooltip("Drag UI TextMeshPro (under Canvas) di sini")]
    [SerializeField] private TextMeshProUGUI healthTextUI;
    [Tooltip("Offset di world untuk di‐screen pos (misal: (0,1.5,0))")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 1.5f, 0);
    [Tooltip("Offset tambahan di layar (pixel)")]
    [SerializeField] private Vector2 screenOffset = new Vector2(0, 20);

    [Header("Defeat Panel")]
    [Tooltip("Drag UI Panel (under Canvas) yang ingin di‐activate saat musuh mati")]
    [SerializeField] private GameObject defeatPanel;

    //––– internal
    private Camera mainCam;
    private RectTransform textRect;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isFlashing;
    private float flashTimer;
    private bool isInvulnerable;
    private float invulTimer;
    private Animator animator;

    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator       = GetComponent<Animator>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        mainCam = Camera.main;
        if (healthTextUI != null)
            textRect = healthTextUI.GetComponent<RectTransform>();

        UpdateHealthText();

        // Pastikan defeat panel tidak aktif di awal
        if (defeatPanel != null)
            defeatPanel.SetActive(false);
    }

    private void Update()
    {
        // flash red logic
        if (isFlashing && (flashTimer -= Time.deltaTime) <= 0f)
        {
            spriteRenderer.color = originalColor;
            isFlashing = false;
        }
        // invul logic
        if (isInvulnerable && (invulTimer -= Time.deltaTime) <= 0f)
            isInvulnerable = false;
    }

    private void LateUpdate()
    {
        if (healthTextUI == null || textRect == null) return;

        // 1) hitung world pos di atas kepala
        Vector3 worldPos = transform.position + worldOffset;
        // 2) convert ke screen point
        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);
        // 3) tempatkan UI text di sana + offset pixel
        textRect.position = screenPos + (Vector3)screenOffset;
    }

    public void TakeDamage(int dmg)
    {
        if (isInvulnerable) return;

        currentHealth -= dmg;
        UpdateHealthText();

        // flash & anim
        spriteRenderer.color = Color.red;
        flashTimer    = flashDuration;
        isFlashing    = true;
        animator?.SetTrigger("d_take_hit");

        // invul
        isInvulnerable = true;
        invulTimer     = invulDuration;

        if (currentHealth <= 0) Die();
    }

    private void UpdateHealthText()
    {
        if (healthTextUI != null)
            healthTextUI.text = $"{currentHealth} / {maxHealth}";
    }

    private void Die()
    {
        animator?.SetTrigger("death");
        if (healthTextUI != null)
            healthTextUI.gameObject.SetActive(false);

        // Mulai coroutine: tunggu animasi selesai, lalu aktifkan panel
        StartCoroutine(DeathSequence());
        // Hancurkan object setelah delay jika mau:
        Destroy(gameObject, GetDeathAnimLength());

        
    }

    private System.Collections.IEnumerator DeathSequence()
    {
        // tunggu sampai animasi death selesai
        float length = GetDeathAnimLength();
        yield return new WaitForSeconds(length);

        if (defeatPanel != null)
            defeatPanel.SetActive(true);
    }

    private float GetDeathAnimLength()
    {
        if (animator != null)
        {
            var clips = animator.GetCurrentAnimatorClipInfo(0);
            foreach (var info in clips)
                if (info.clip.name.ToLower().Contains("death"))
                    return info.clip.length;
        }
        return 1f;
    }
}
