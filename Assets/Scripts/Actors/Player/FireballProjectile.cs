using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class FireballProjectile : MonoBehaviour
{
    [SerializeField] float speed = 4f;
    [SerializeField] float damage = 1f;
    [SerializeField] float lifetime = 3f;
    [SerializeField] Sprite[] animationFrames;
    [SerializeField] float animationRate = 12f;

    Rigidbody2D body;
    SpriteRenderer spriteRenderer;
    BoxCollider2D hitCollider;
    float animationTimer;
    int animationFrame;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        hitCollider = GetComponent<BoxCollider2D>();
        if (animationFrames != null && animationFrames.Length > 0 && spriteRenderer != null)
        {
            spriteRenderer.sprite = animationFrames[0];
            spriteRenderer.enabled = true;
            MatchColliderToSprite();
        }
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (animationFrames == null || animationFrames.Length == 0 || spriteRenderer == null)
            return;

        animationTimer += Time.deltaTime;
        if (animationTimer >= 1f / animationRate)
        {
            animationTimer = 0f;
            animationFrame = (animationFrame + 1) % animationFrames.Length;
            spriteRenderer.sprite = animationFrames[animationFrame];
            MatchColliderToSprite();
        }
    }

    void MatchColliderToSprite()
    {
        if (hitCollider == null || spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        Bounds spriteBounds = spriteRenderer.sprite.bounds;
        hitCollider.offset = spriteBounds.center;
        hitCollider.size = spriteBounds.size;
    }

    public void Launch(float direction)
    {
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * Mathf.Sign(direction), transform.localScale.y, transform.localScale.z);
        body.linearVelocity = Vector2.right * direction * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (IsBreakableBlock(other.gameObject) &&
            GameData.instance != null &&
            GameData.instance.CherryGemCount >= 5)
        {
            Destroy(FindBreakableObject(other.gameObject));
            Destroy(gameObject);
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
            Destroy(gameObject);
    }

    bool IsBreakableBlock(GameObject candidate)
    {
        return FindBreakableObject(candidate) != null;
    }

    GameObject FindBreakableObject(GameObject candidate)
    {
        Transform current = candidate.transform;
        while (current != null)
        {
            if (current.name.StartsWith("Block_01"))
                return current.gameObject;
            current = current.parent;
        }

        return null;
    }
}
