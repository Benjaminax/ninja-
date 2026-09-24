using UnityEngine;

public class MushroomAI : Enemy
{
    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float idleTime = 2f;
    [SerializeField] float walkTime = 3f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform wallCheck;
    [SerializeField] bool isSpriteFacingRight = false; // Trong ảnh của bạn nấm đang nhìn TRÁI, nên để false
    [SerializeField] bool randomizeStart = true;

    private float stateTimer;
    private bool isWalking;
    private int walkDirection = -1; // -1: Trái, 1: Phải

    protected override void Start()
    {
        base.Start();

        if (randomizeStart)
        {
            walkDirection = Random.value > 0.5f ? 1 : -1;
            // Desynchronize start time so they don't all move at once
            stateTimer = Random.Range(0f, idleTime);
        }
        else
        {
            // Tự động xác định hướng đi ban đầu dựa trên vị trí của WallCheckPoint
            walkDirection = (wallCheck.localPosition.x > 0) ? 1 : -1;
            stateTimer = idleTime;
        }

        isWalking = false;
        UpdateSpriteDirection();
    }

    void Update()
    {
        HandleState();
    }

    void HandleState()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            isWalking = !isWalking;
            stateTimer = isWalking ? walkTime : idleTime;
            animator.SetBool("IsRunning", isWalking);
            if (!isWalking) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        if (isWalking) MoveLogic();
    }

    void MoveLogic()
    {
        // 1. Kiểm tra đất và tường để quay đầu
        bool isFrontGrounded = Physics2D.Raycast(wallCheck.position, Vector2.down, 0.5f, groundLayer);
        bool isHittingWall = Physics2D.Raycast(wallCheck.position, transform.right * walkDirection, 0.2f, groundLayer);
        bool isActuallyOnGround = Physics2D.Raycast(transform.position, Vector2.down, 0.6f, groundLayer);

        if (isActuallyOnGround && (!isFrontGrounded || isHittingWall))
        {
            Flip();
        }

        // 2. Áp dụng vận tốc
        rb.linearVelocity = new Vector2(moveSpeed * walkDirection, rb.linearVelocity.y);
    }

    void Flip()
    {
        walkDirection *= -1;
        UpdateSpriteDirection();
    }

    void UpdateSpriteDirection()
    {
        // Nếu sprite gốc nhìn phải, scale X = walkDirection
        // Nếu sprite gốc nhìn trái, scale X = -walkDirection
        float scaleX = isSpriteFacingRight ? walkDirection : -walkDirection;
        transform.localScale = new Vector3(scaleX, 1f, 1f);
    }

    // --- LOGIC XỬ LÝ KHI BỊ PLAYER NHẢY LÊN ĐẦU ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Lấy điểm tiếp xúc đầu tiên
            ContactPoint2D contact = collision.GetContact(0);

            // Nếu góc va chạm từ trên xuống (Normal.y âm)
            if (contact.normal.y < -0.5f)
            {
                Hit(); // Gọi hàm bị trúng đòn

                // Đẩy người chơi nảy lên một chút
                Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 12f);
                }
            }
        }
    }

    public void Hit()
    {
        // 1. Chạy Animation Hit
        animator.SetTrigger("IsHitting");
        animator.SetBool("IsRunning", false);

        // 2. Ngừng di chuyển và tắt va chạm để không làm phiền Player nữa
        isWalking = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic; // Biến thành vật thể không vật lý
        GetComponent<Collider2D>().enabled = false;

        // 3. Biến mất sau 0.5 giây (để kịp xem animation Hit)
        Invoke("Die", 0.5f);
    }

    // --- HÀM BỔ SUNG: VẼ TIA RAYCAST ĐỂ MÔ PHỎNG ---
    private void OnDrawGizmos()
    {
        // Nếu chưa kéo vị trí wallCheck vào Inspector thì không vẽ để tránh lỗi
        if (wallCheck == null) return;

        // Lưu ý: Chạy lại các tia Raycast trong Gizmos bằng thông số y hệt như MoveLogic để đảm bảo hiển thị chính xác 100%

        // 1. Tia kiểm tra ĐẤT PHÍA TRƯỚC (Chiều dài 0.5f)
        bool isFrontGrounded = Physics2D.Raycast(wallCheck.position, Vector2.down, 0.5f, groundLayer);
        // Nếu chạm đất -> Màu Xanh (An toàn). Nếu hụt đất (vực) -> Màu Đỏ (Quay đầu).
        Gizmos.color = isFrontGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.down * 0.5f);
        // Vẽ thêm một chấm nhỏ ở đầu tia để dễ nhìn độ dài
        Gizmos.DrawSphere(wallCheck.position + Vector3.down * 0.5f, 0.05f);


        // 2. Tia kiểm tra TƯỜNG PHÍA TRƯỚC (Chiều dài 0.2f)
        Vector3 wallDir = transform.right * walkDirection;
        bool isHittingWall = Physics2D.Raycast(wallCheck.position, wallDir, 0.2f, groundLayer);
        // Nếu không vướng tường -> Màu Xanh. Nếu đâm vào tường -> Màu Đỏ (Quay đầu).
        Gizmos.color = isHittingWall ? Color.red : Color.green;
        Gizmos.DrawLine(wallCheck.position, wallCheck.position + wallDir * 0.2f);
        Gizmos.DrawSphere(wallCheck.position + wallDir * 0.2f, 0.04f);


        // 3. Tia kiểm tra DƯỚI CHÂN NẤM (Chiều dài 0.6f)
        bool isActuallyOnGround = Physics2D.Raycast(transform.position, Vector2.down, 0.6f, groundLayer);
        // Màu vàng chanh nếu đang đứng trên đất, màu trắng nếu đang ở trên không
        Gizmos.color = isActuallyOnGround ? Color.yellow : Color.white;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 0.6f);
    }
}