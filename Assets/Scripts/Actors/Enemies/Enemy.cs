using UnityEngine;
using ThomasDev.HealthDamageSystem;
using ThomasDev.HealthSystem;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] protected float health = 1f;
    [SerializeField] protected int damage = 1;

    protected Rigidbody2D rb;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    Health healthComponent;
    [SerializeField] GameObject healthBarPrefab;
    GameObject healthBarInstance;
    Color[] originalSpriteColors;
    Coroutine flashCoroutine;

    [SerializeField] float hitFlashDuration = 0.08f;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalSpriteColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalSpriteColors[i] = renderers[i].color;

        healthComponent = GetComponent<Health>();
        if (healthComponent == null)
        {
            healthComponent = gameObject.AddComponent<Health>();
            healthComponent.Initialize(health);
        }

        healthComponent.OnDamaged.AddListener(OnHealthDamaged);
        healthComponent.OnDeath.AddListener(OnHealthDeath);
        EnemyContactDamage contactDamage = GetComponent<EnemyContactDamage>();
        if (contactDamage == null)
            contactDamage = gameObject.AddComponent<EnemyContactDamage>();
        contactDamage.Configure(damage);
        CreateHealthBar();
    }

    public virtual void TakeDamage(float amount)
    {
        if (healthComponent == null)
        {
            health -= amount;
            if (health <= 0f)
                Die();
            return;
        }

        healthComponent.TakeDamage(amount);
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    void OnHealthDamaged(float current, float maximum)
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashWhite());
    }

    public class EnemyContactDamage : MonoBehaviour
    {
        int damage = 1;
        float nextDamageTime;

        public void Configure(int amount)
        {
            damage = Mathf.Max(1, amount);
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            if (!collision.collider.CompareTag("Player") ||
                collision.contactCount == 0 ||
                Time.time < nextDamageTime)
                return;

            Collider2D enemyCollider = GetComponent<Collider2D>();
            if (enemyCollider == null ||
                Physics2D.Distance(enemyCollider, collision.collider).distance > 0f)
                return;

            Enemy enemy = GetComponent<Enemy>();
            ContactPoint2D contact = collision.GetContact(0);
            if (enemy != null)
                enemy.ReactToHit();

            if (contact.normal.y < -0.5f)
            {
                Rigidbody2D playerBody = collision.rigidbody;
                if (playerBody != null)
                    playerBody.linearVelocity = new Vector2(playerBody.linearVelocity.x, 12f);
                Enemy hitEnemy = enemy;
                if (hitEnemy != null)
                    hitEnemy.TakeDamage(hitEnemy.CurrentHealth);
                nextDamageTime = Time.time + 1f;
                return;
            }

            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth == null)
            {
                playerHealth = collision.gameObject.AddComponent<Health>();
                playerHealth.Initialize(5f);
            }

            playerHealth.TakeDamage(damage);
            nextDamageTime = Time.time + 1f;
        }
    }

    void OnHealthDeath()
    {
        Die();
    }

    System.Collections.IEnumerator FlashWhite()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].color = Color.white;

        yield return new WaitForSeconds(hitFlashDuration);

        for (int i = 0; i < renderers.Length && i < originalSpriteColors.Length; i++)
            if (renderers[i] != null)
                renderers[i].color = originalSpriteColors[i];
        flashCoroutine = null;
    }

    void CreateHealthBar()
    {
        if (healthBarPrefab == null)
            return;

        Canvas canvas = new GameObject("EnemyHealthBar").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingLayerID = SortingLayer.NameToID("Enermies");
        canvas.sortingOrder = 500;
        canvas.worldCamera = Camera.main;
        canvas.transform.SetParent(transform, false);
        canvas.transform.localPosition = Vector3.up * 1.25f;
        canvas.transform.localScale = Vector3.one * 0.032f;
        healthBarInstance = Instantiate(healthBarPrefab, canvas.transform);
        healthBarInstance.name = "HealthBar";
        RectTransform rect = healthBarInstance.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.localPosition = Vector3.zero;
        rect.localScale = Vector3.one;
        rect.sizeDelta = new Vector2(460f, 72f);
        rect.localScale = Vector3.one * 1.3f;
        HealthBarUI healthBar = healthBarInstance.GetComponentInChildren<HealthBarUI>(true);
        if (healthBar != null)
            healthBar.Bind(healthComponent);
    }

    public void ReactToHit()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashWhite());
    }

    protected float CurrentHealth => healthComponent != null ? healthComponent.CurrentHealth : health;

    public void PositionHealthBarAboveRenderers(float extraHeight)
    {
        if (healthBarInstance == null)
            return;

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        if (renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        Transform barCanvas = healthBarInstance.transform.parent;
        barCanvas.position = new Vector3(bounds.center.x, bounds.max.y + extraHeight, transform.position.z - 0.1f);
    }
}