using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayMove : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float jumpSpeed = 15f;
    [SerializeField] float doubleJumpMultiplier = 1.2f; // nhảy lần 2 cao hơn 20%

    [Header("Detection Settings")]
    [SerializeField] BoxCollider2D feetCollider;
    [SerializeField] float coyoteTime = 0.2f;
    [SerializeField] float jumpBufferTime = 0.2f;
    [SerializeField] Sprite slideSprite;
    [SerializeField] float slideDuration = 0.5f;
    [SerializeField] float slideSpeed = 10f;
    [SerializeField] int blockBreakGemRequirement = 5;


    // Khai báo các biến hỗ trợ logic
    float coyoteTimeCounter;
    float jumpBufferCounter;
    int jumpCount = 0;

    Vector2 moveInput;
    Rigidbody2D myRidibody;
    Animator myAnimator;
    PlayerInput playerInput;
    SpriteRenderer spriteRenderer;
    Sprite normalSprite;
    InputAction moveAction;
    InputAction jumpAction;
    InputAction crouchAction;
    bool isSliding;
    float slideTimer;

    void Start()
    {
        myRidibody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        normalSprite = spriteRenderer.sprite;
        if (playerInput != null)
        {
            if (playerInput.actions == null)
            {
                Debug.LogError("PlayMove requires a PlayerInput action asset.", this);
            }
            else
            {
                moveAction = playerInput.actions.FindAction("Move", false);
                jumpAction = playerInput.actions.FindAction("Jump", false);
                crouchAction = playerInput.actions.FindAction("Crouch", false);
            }
            playerInput.actions?.Enable();
        }
        if (feetCollider == null)
        {
            Transform feetCheck = transform.Find("FeetCheck");
            if (feetCheck != null)
                feetCollider = feetCheck.GetComponent<BoxCollider2D>();
        }
        if (feetCollider == null)
        {
            GameObject feetCheck = new GameObject("FeetCheck");
            feetCheck.transform.SetParent(transform, false);
            feetCheck.transform.localPosition = new Vector3(0f, -0.095f, 0f);
            feetCollider = feetCheck.AddComponent<BoxCollider2D>();
            feetCollider.isTrigger = true;
            feetCollider.size = new Vector2(0.12f, 0.02f);
        }
        if (playerInput != null && playerInput.actions != null && !playerInput.actions.enabled)
        {
            playerInput.actions.Enable();
        }
    }

    void Update()
    {
        MobileControls mobileControls = MobileControls.Instance;
        if (mobileControls != null && mobileControls.MoveX != 0f)
            moveInput = new Vector2(mobileControls.MoveX, 0f);

        if ((jumpAction != null && jumpAction.WasPressedThisFrame())
            || (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            || (mobileControls != null && mobileControls.JumpPressedThisFrame))
        {
            jumpBufferCounter = jumpBufferTime;
        }

        bool crouchPressed = crouchAction != null && crouchAction.WasPressedThisFrame();
        if (Keyboard.current != null)
            crouchPressed |= Keyboard.current.cKey.wasPressedThisFrame;

        if (crouchPressed && IsGrounded() && !isSliding && Mathf.Abs(moveInput.x) > 0.1f)
        {
            isSliding = true;
            slideTimer = slideDuration;
            float direction = Mathf.Sign(moveInput.x);
            Vector3 scale = transform.localScale;
            scale.x = direction * Mathf.Abs(scale.x);
            transform.localScale = scale;
            myRidibody.linearVelocity = new Vector2(direction * slideSpeed, 0f);
            if (slideSprite != null && spriteRenderer != null)
            {
                myAnimator.enabled = false;
                spriteRenderer.sprite = slideSprite;
            }
        }

        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            float direction = Mathf.Sign(transform.localScale.x);
            myRidibody.linearVelocity = new Vector2(direction * slideSpeed, 0f);
            if (slideTimer <= 0f)
            {
                isSliding = false;
                myAnimator.enabled = true;
                spriteRenderer.sprite = normalSprite;
            }
            else
            {
                return;
            }
        }

        if (moveAction != null)
        {
            moveInput = moveAction.ReadValue<Vector2>();
        }
        else if (Keyboard.current != null)
        {
            moveInput = Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed
                ? Vector2.up
                : Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed
                    ? Vector2.down
                    : Vector2.zero;
            moveInput.x = (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed ? 1f : 0f)
                - (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed ? 1f : 0f);
        }

        if (mobileControls != null && mobileControls.MoveX != 0f)
            moveInput = new Vector2(mobileControls.MoveX, 0f);

        UpdateTimers();
        Run();
        FlipScript();
        CheckJump();
        if (myRidibody.linearVelocity.y > 0.1f)
            BreakBlockWithJump();
        HandleAirAnimations();
    }

    void BreakBlockWithJump()
    {
        if (GameData.instance == null ||
            GameData.instance.CherryGemCount < blockBreakGemRequirement)
            return;

        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null)
            return;

        Bounds bounds = playerCollider.bounds;
        Vector2 center = new Vector2(bounds.center.x, bounds.max.y + 0.2f);
        Vector2 size = new Vector2(Mathf.Max(bounds.size.x * 1.2f, 0.45f), 0.65f);
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f);

        foreach (Collider2D hit in hits)
        {
            if (hit == null || hit.transform.IsChildOf(transform))
                continue;

            if (!IsBreakableBlock(hit.gameObject))
                continue;

            BreakBlock(hit.gameObject);
            break;
        }
    }

    bool IsBreakableBlock(GameObject candidate)
    {
        Transform current = candidate.transform;
        while (current != null)
        {
            if (current.name.StartsWith("Block_01"))
                return true;
            current = current.parent;
        }

        return false;
    }

    void BreakBlock(GameObject block)
    {
        if (block == null)
            return;

        Destroy(block);
        if (myAnimator != null)
            myAnimator.Play("JumpSkill", 0, 0f);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        TryBreakBlock(collision);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        TryBreakBlock(collision);
    }

    void TryBreakBlock(Collision2D collision)
    {
        if (GameData.instance == null ||
            GameData.instance.CherryGemCount < blockBreakGemRequirement)
            return;

        if (myRidibody.linearVelocity.y < -0.1f)
            return;

        if (IsBreakableBlock(collision.gameObject))
            BreakBlock(collision.gameObject);
    }

    void UpdateTimers()
    {
        // SỬA TẠI ĐÂY: Tách biệt logic Grounded và logic đếm ngược trên không
        if (IsGrounded())
        {
            // Khi chạm đất và tốc độ rơi gần như bằng 0 thì reset số lần nhảy
            if (myRidibody.linearVelocity.y <= 0.1f)
            {
                coyoteTimeCounter = coyoteTime; // Giữ đầy thanh Coyote Time khi đi trên đất
                jumpCount = 0;
            }
        }
        else
        {
            // Khi rời mặt đất (rơi tự do hoặc nhảy), thời gian Coyote Time bắt đầu đếm lùi
            coyoteTimeCounter -= Time.deltaTime;
        }

        // SỬA TẠI ĐÂY: Jump Buffer phải luôn luôn đếm ngược bất kể đang ở đâu
        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    bool IsGrounded()
    {
        return feetCollider != null && feetCollider.IsTouchingLayers(LayerMask.GetMask("Ground"));
    }

    void CheckJump()
    {
        if (jumpBufferCounter > 0)
        {
            // Nhảy lần 1: Có thể nhảy nếu còn trong thời gian Coyote Time (kể cả vừa rời đất)
            if (jumpCount == 0 && coyoteTimeCounter > 0)
            {
                ExecuteJump(jumpSpeed);
                myAnimator.SetBool("IsJumping", true);
            }
            // Nhảy lần 2 (Double Jump): Phải ở trên không và chưa nhảy quá số lần cho phép
            else if (jumpCount > 0 && jumpCount < 2)
            {
                ExecuteJump(jumpSpeed * doubleJumpMultiplier);
                myAnimator.SetTrigger("IsDoubleJumping");
            }
        }
    }

    void ExecuteJump(float force)
    {
        // SỬA TẠI ĐÂY: Sử dụng biến 'force' được truyền vào thay vì cố định 'jumpSpeed'
        myRidibody.linearVelocity = new Vector2(myRidibody.linearVelocity.x, force);

        jumpCount++;
        jumpBufferCounter = 0f; // Reset ngay sau khi đã thực hiện nhảy
        coyoteTimeCounter = 0f; // Khóa Coyote Time để không ăn lận nhảy lần 1 trên không
    }

    void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpBufferCounter = jumpBufferTime; // Kích hoạt bộ đệm chờ nhảy
        }
    }

    void HandleAirAnimations()
    {
        if (IsGrounded() && Mathf.Abs(myRidibody.linearVelocity.y) < 0.1f)
        {
            myAnimator.SetBool("IsJumping", false);
            myAnimator.SetBool("IsFalling", false);
            return;
        }

        if (myRidibody.linearVelocity.y > 0.1f)
        {
            myAnimator.SetBool("IsJumping", true);
            myAnimator.SetBool("IsFalling", false);
        }
        else if (myRidibody.linearVelocity.y < -0.1f)
        {
            myAnimator.SetBool("IsFalling", true);
            myAnimator.SetBool("IsJumping", false);
        }
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void Run()
    {
        myRidibody.linearVelocity = new Vector2(moveInput.x * moveSpeed, myRidibody.linearVelocity.y);
        bool hasRunning = Mathf.Abs(myRidibody.linearVelocity.x) > Mathf.Epsilon;
        myAnimator.SetBool("IsRunning", hasRunning);
    }

    void FlipScript()
    {
        bool hasHorizontalSpeed = Mathf.Abs(myRidibody.linearVelocity.x) > Mathf.Epsilon;
        if (hasHorizontalSpeed)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(myRidibody.linearVelocity.x) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    public void Die()
    {
        Debug.Log("Player đã tử trận!");
        this.enabled = false;
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        Invoke("ReloadLevel", 1f);
    }

    void ReloadLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}