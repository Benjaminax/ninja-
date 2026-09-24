using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class ChasingZombie : Enemy
{
    [SerializeField] float moveSpeed = 1.5f;
    [SerializeField] bool spriteFacesRight = true;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float groundProbeDistance = 0.12f;
    [SerializeField] float maximumStepDown = 0.15f;

    Transform player;
    Animator childAnimator;
    bool dying;
    bool sizeMatched;
    float? currentGroundHeight;
    float? lockedGroundHeight;
    Vector3 lastSafePosition;
    bool hasSafePosition;

    void OnEnable()
    {
        ConfigureVisuals();
        if (!Application.isPlaying && gameObject.scene.IsValid())
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                MatchPlayerSizeAndFloor(playerObject);
        }
    }

    protected override void Start()
    {
        ConfigureVisuals();
        if (!Application.isPlaying)
            return;

        base.Start();
        EnsurePhysicsComponents();
        if (groundLayer.value == 0)
            groundLayer = LayerMask.GetMask("Ground");
        CacheCurrentGroundHeight();
        lockedGroundHeight = currentGroundHeight;
        lastSafePosition = transform.position;
        hasSafePosition = true;
        // Set up the renderers before looking for the player.  A malformed or
        // temporarily unavailable player must not prevent the zombie visuals
        // from being enabled and sorted.
        childAnimator = GetComponentInChildren<Animator>(true);
        SortingGroup sortingGroup = GetComponent<SortingGroup>();
        if (sortingGroup == null)
            sortingGroup = gameObject.AddComponent<SortingGroup>();
        sortingGroup.sortingLayerName = "Enermies";
        sortingGroup.sortingOrder = 100;

        foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.sortingLayerName = "Enermies";
            renderer.sortingOrder = 100;
            renderer.enabled = true;
        }

        if (childAnimator != null)
            PlayAnimation("Idle");

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
            playerObject = FindAnyObjectByType<PlayMove>()?.gameObject;
        if (playerObject != null)
        {
            player = playerObject.transform;
            MatchPlayerSizeAndFloor(playerObject);
            PositionHealthBarAboveRenderers(0.12f);
        }

        if (rb != null)
        {
            rb.gravityScale = 5f;
            rb.freezeRotation = true;
        }
    }

    void ConfigureVisuals()
    {
        foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.enabled = true;
            renderer.sortingLayerName = "Enermies";
            renderer.sortingOrder = 100;
        }

        SortingGroup sortingGroup = GetComponent<SortingGroup>();
        if (sortingGroup == null)
            sortingGroup = gameObject.AddComponent<SortingGroup>();
        sortingGroup.sortingLayerName = "Enermies";
        sortingGroup.sortingOrder = 100;
    }

    void Update()
    {
        if (!Application.isPlaying)
            return;

        if (dying || rb == null)
            return;

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null)
                playerObject = FindAnyObjectByType<PlayMove>()?.gameObject;
            if (playerObject != null)
            {
                player = playerObject.transform;
                MatchPlayerSizeAndFloor(playerObject);
            }
            else
            {
                return;
            }
        }

        CacheCurrentGroundHeight();
        if (!IsOnAllowedGround())
        {
            StopAtLastSafePosition();
            return;
        }

        lastSafePosition = transform.position;
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        if (Mathf.Abs(player.position.x - transform.position.x) > 0.05f)
            SetFacing(direction);

        if (Mathf.Abs(player.position.x - transform.position.x) > 0.05f)
        {
            if (CanWalkOnGround(direction))
            {
                rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
                PlayAnimation("Walk");
            }
            else
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                PlayAnimation("Idle");
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            PlayAnimation("Idle");
        }
    }

    bool CanWalkOnGround(float direction)
    {
        if (groundLayer.value == 0)
            return false;

        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
            return false;

        Bounds bounds = collider.bounds;
        float probeX = direction > 0f
            ? bounds.max.x + groundProbeDistance
            : bounds.min.x - groundProbeDistance;
        Vector2 probeStart = new Vector2(probeX, bounds.max.y + 0.1f);
        float probeLength = bounds.size.y + 0.35f;
        RaycastHit2D hit = Physics2D.Raycast(probeStart, Vector2.down, probeLength, groundLayer);
        if (hit.collider == null || hit.normal.y < 0.5f)
            return false;

        if (lockedGroundHeight.HasValue &&
            hit.point.y < lockedGroundHeight.Value - maximumStepDown)
            return false;

        return true;
    }

    bool IsOnAllowedGround()
    {
        if (groundLayer.value == 0)
            return false;

        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
            return false;

        Bounds bounds = collider.bounds;
        RaycastHit2D hit = Physics2D.Raycast(
            new Vector2(bounds.center.x, bounds.min.y + 0.1f),
            Vector2.down,
            0.35f,
            groundLayer);
        if (hit.collider == null || hit.normal.y < 0.5f)
            return false;

        return !lockedGroundHeight.HasValue ||
            hit.point.y >= lockedGroundHeight.Value - maximumStepDown;
    }

    void StopAtLastSafePosition()
    {
        rb.linearVelocity = Vector2.zero;
        if (hasSafePosition)
            transform.position = lastSafePosition;
        PlayAnimation("Idle");
    }

    void CacheCurrentGroundHeight()
    {
        if (groundLayer.value == 0)
            return;

        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
            return;

        Bounds bounds = collider.bounds;
        RaycastHit2D hit = Physics2D.Raycast(
            new Vector2(bounds.center.x, bounds.min.y + 0.1f),
            Vector2.down,
            0.3f,
            groundLayer);
        if (hit.collider != null && hit.normal.y >= 0.5f)
            currentGroundHeight = hit.point.y;
    }

    void SetFacing(float direction)
    {
        float x = spriteFacesRight ? direction : -direction;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * x;
        transform.localScale = scale;
    }

    void MatchPlayerSizeAndFloor(GameObject playerObject)
    {
        if (sizeMatched)
            return;

        SpriteRenderer playerRenderer = playerObject.GetComponent<SpriteRenderer>();
        SpriteRenderer[] zombieRenderers = GetComponentsInChildren<SpriteRenderer>();
        if (playerRenderer == null || zombieRenderers.Length == 0)
            return;

        Bounds visualBounds = GetVisualBounds(zombieRenderers);
        if (visualBounds.size.y > 0.001f)
        {
            float scaleFactor = playerRenderer.bounds.size.y / visualBounds.size.y;
            transform.localScale *= scaleFactor;
            sizeMatched = true;
        }

    }

    void EnsurePhysicsComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        CapsuleCollider2D collider = GetComponent<CapsuleCollider2D>();
        if (collider == null)
            collider = gameObject.AddComponent<CapsuleCollider2D>();

        collider.direction = CapsuleDirection2D.Vertical;
        collider.isTrigger = false;
        gameObject.tag = "Enemy";
    }

    Bounds GetVisualBounds(SpriteRenderer[] renderers)
    {
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    protected override void Die()
    {
        if (dying)
            return;

        dying = true;
        rb.linearVelocity = Vector2.zero;
        if (childAnimator != null)
            PlayAnimation("Death Skeleton");
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;
        Destroy(gameObject, 0.8f);
    }

    void PlayAnimation(string stateName)
    {
        if (childAnimator == null || !childAnimator.HasState(0, Animator.StringToHash(stateName)))
            return;

        if (!childAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            childAnimator.Play(stateName, 0, 0f);
    }
}
