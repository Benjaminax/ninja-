using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInput))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Sword")]
    [SerializeField] float swordRange = 0.85f;
    [SerializeField] float swordDamage = 1f;
    [SerializeField] float attackCooldown = 0.35f;

    [Header("Fireball")]
    [SerializeField] GameObject fireballPrefab;
    [SerializeField] float fireballOffset = 0.55f;
    [SerializeField] Sprite[] noSwordCastFrames;
    [SerializeField] float noSwordCastFrameRate = 12f;

    Animator animator;
    Rigidbody2D body;
    SpriteRenderer spriteRenderer;
    PlayerInput playerInput;
    InputAction fireballAction;
    float nextAttackTime;
    float castAnimationTimer;
    int castAnimationFrame;
    bool castingWithoutSword;
    Sprite normalSprite;
    Vector3 normalScale;

    void Awake()
    {
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        normalSprite = spriteRenderer != null ? spriteRenderer.sprite : null;
        normalScale = transform.localScale;
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.actions.Enable();
            fireballAction = playerInput.actions.FindAction("Fireball");
        }
    }

    void Update()
    {
        if (castingWithoutSword && noSwordCastFrames != null && noSwordCastFrames.Length > 0)
        {
            castAnimationTimer += Time.deltaTime;
            if (castAnimationTimer >= 1f / noSwordCastFrameRate)
            {
                castAnimationTimer = 0f;
                castAnimationFrame = (castAnimationFrame + 1) % noSwordCastFrames.Length;
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = noSwordCastFrames[castAnimationFrame];
                    NormalizeCastSpriteSize();
                }
            }
        }

        if (Time.time < nextAttackTime)
            return;

        bool swordPressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        swordPressed |= MobileControls.Instance != null && MobileControls.Instance.SwordPressedThisFrame;
        if (swordPressed && IsPointerOverButton())
            swordPressed = false;

        bool fireballPressed = fireballAction != null && fireballAction.WasPressedThisFrame();
        if (Mouse.current != null)
            fireballPressed |= Mouse.current.rightButton.wasPressedThisFrame;
        if (Keyboard.current != null)
            fireballPressed |= Keyboard.current.fKey.wasPressedThisFrame;
        fireballPressed |= MobileControls.Instance != null && MobileControls.Instance.FireballPressedThisFrame;

        if (swordPressed)
        {
            SwordStrike();
        }
        else if (fireballPressed)
        {
            CastFireball();
        }
    }

    bool IsPointerOverButton()
    {
        if (EventSystem.current == null || Mouse.current == null)
            return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponentInParent<Button>() != null)
                return true;
        }

        return false;
    }

    void SwordStrike()
    {
        nextAttackTime = Time.time + attackCooldown;
        animator.Play("JumpSkill", 0, 0f);
        CancelInvoke(nameof(ResumeMovementAnimation));
        Invoke(nameof(ResumeMovementAnimation), 0.5f);

        float direction = transform.localScale.x >= 0f ? 1f : -1f;
        Vector2 center = (Vector2)transform.position + Vector2.right * direction * swordRange;
        Collider2D[] targets = Physics2D.OverlapCircleAll(center, swordRange);

        foreach (Collider2D target in targets)
        {
            Enemy enemy = target.GetComponentInParent<Enemy>();
            if (enemy != null)
                enemy.TakeDamage(swordDamage);
        }
    }

    void ResumeMovementAnimation()
    {
        if (body.linearVelocity.y > 0.1f)
            animator.Play("Jump", 0, 0f);
        else if (body.linearVelocity.y < -0.1f)
            animator.Play("Fall", 0, 0f);
        else if (Mathf.Abs(body.linearVelocity.x) > 0.1f)
            animator.Play("Running", 0, 0f);
        else
            animator.Play("Idlling", 0, 0f);
    }

    void CastFireball()
    {
        if (fireballPrefab == null)
        {
            Debug.LogError("PlayerCombat requires a fireball prefab.", this);
            return;
        }

        nextAttackTime = Time.time + attackCooldown;
        StartNoSwordCastAnimation();
        float direction = transform.localScale.x >= 0f ? 1f : -1f;
        Vector3 spawnPosition = transform.position + Vector3.right * direction * fireballOffset;
        GameObject fireball = Instantiate(fireballPrefab, spawnPosition, Quaternion.identity);
        FireballProjectile projectile = fireball.GetComponent<FireballProjectile>();
        if (projectile == null)
        {
            Destroy(fireball);
            Debug.LogError("The fireball prefab requires FireballProjectile.", fireball);
            return;
        }

        projectile.Launch(direction);
    }

    void EndFireballAnimation()
    {
        castingWithoutSword = false;
        transform.localScale = normalScale;
        if (animator != null)
            animator.enabled = true;
        ResumeMovementAnimation();
    }

    void NormalizeCastSpriteSize()
    {
        if (spriteRenderer == null || normalSprite == null || spriteRenderer.sprite == null)
            return;

        Vector2 normalSize = normalSprite.bounds.size;
        Vector2 castSize = spriteRenderer.sprite.bounds.size;
        if (castSize.x <= 0f || castSize.y <= 0f)
            return;

        Vector3 scale = transform.localScale;
        float widthRatio = normalSize.x / castSize.x;
        float heightRatio = normalSize.y / castSize.y;
        float ratio = Mathf.Min(widthRatio, heightRatio);
        scale.x = Mathf.Sign(scale.x == 0f ? normalScale.x : scale.x)
            * Mathf.Abs(normalScale.x) * ratio;
        scale.y = Mathf.Sign(scale.y == 0f ? normalScale.y : scale.y)
            * Mathf.Abs(normalScale.y) * ratio;
        scale.z = normalScale.z;
        transform.localScale = scale;
    }

    void StartNoSwordCastAnimation()
    {
        if (noSwordCastFrames == null || noSwordCastFrames.Length == 0)
        {
            animator.Play("JumpSkill", 0, 0f);
            CancelInvoke(nameof(EndFireballAnimation));
            Invoke(nameof(EndFireballAnimation), 0.45f);
            return;
        }

        castingWithoutSword = true;
        castAnimationTimer = 0f;
        castAnimationFrame = 0;
        if (animator != null)
            animator.enabled = false;
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = noSwordCastFrames[0];
            NormalizeCastSpriteSize();
        }
        CancelInvoke(nameof(EndFireballAnimation));
        Invoke(nameof(EndFireballAnimation), 0.45f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.right * Mathf.Sign(transform.localScale.x) * swordRange, swordRange);
    }
}
