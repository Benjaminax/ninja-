using UnityEngine;

public class ChickenAI : Enemy
{
    [Header("Detection Settings")]
    [SerializeField] float detectionRange = 5f;
    [SerializeField] float maxDetectionHeight = 2f; // Chỉ phát hiện nếu Player không quá cao/thấp
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform wallCheck;
    [SerializeField] bool isSpriteFacingRight = false;

    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 4f;

    [Header("Simulation Visuals (Nâng Cấp Siêu Nét)")]
    [SerializeField] float rayThickness = 6f;          // Độ dày của các tia kiểm tra đất/tường/lazer
    [SerializeField] bool showRadarZone = true;          // Bật/Tắt vẽ vùng Radar
    [SerializeField] float radarBorderThickness = 4f;   // ĐỘ DÀY VIỀN KHUNG RADAR (Chỉnh viền to nhỏ ở đây)

    [Space(5)]
    [Header("Radar Colors Customize")]
    [SerializeField] Color radarIdleColor = new Color(0f, 0.8f, 1f, 0.1f);   // Màu nền khi đứng yên (Mặc định: Xanh Cyan mờ)
    [SerializeField] Color borderIdleColor = new Color(0f, 0.8f, 1f, 0.8f);  // Màu viền khi đứng yên (Mặc định: Xanh Cyan rõ)
    [SerializeField] Color radarChaseColor = new Color(1f, 0f, 0.2f, 0.15f); // Màu nền khi đuổi (Mặc định: Đỏ Neon mờ)
    [SerializeField] Color borderChaseColor = new Color(1f, 0f, 0.2f, 0.9f); // Màu viền khi đuổi (Mặc định: Đỏ Neon rõ)

    private bool isChasing = false;
    private Transform playerTransform;

    protected override void Start()
    {
        base.Start();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        if (playerTransform == null) return;

        float xDistance = Mathf.Abs(transform.position.x - playerTransform.position.x);
        float yDistance = Mathf.Abs(transform.position.y - playerTransform.position.y);

        if (xDistance <= detectionRange && yDistance <= maxDetectionHeight)
        {
            isChasing = true;
        }
        else
        {
            isChasing = false;
        }

        HandleMovement();
    }

    void HandleMovement()
    {
        if (isChasing)
        {
            int moveDir = (playerTransform.position.x > transform.position.x) ? 1 : -1;

            bool isFrontGrounded = Physics2D.Raycast(wallCheck.position, Vector2.down, 0.7f, groundLayer);
            bool isHittingWall = Physics2D.Raycast(wallCheck.position, Vector2.right * moveDir, 0.3f, groundLayer);

            if (isFrontGrounded && !isHittingWall)
            {
                animator.SetBool("IsRunning", true);
                rb.linearVelocity = new Vector2(moveDir * moveSpeed, rb.linearVelocity.y);
                UpdateFacing(moveDir);
            }
            else
            {
                StopMoving();
                UpdateFacing(moveDir);
            }
        }
        else
        {
            StopMoving();
        }
    }

    void StopMoving()
    {
        animator.SetBool("IsRunning", false);
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    void UpdateFacing(int dir)
    {
        float scaleX = isSpriteFacingRight ? dir : -dir;
        transform.localScale = new Vector3(scaleX, 1f, 1f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.GetContact(0).normal.y < -0.5f)
            {
                DieByJump(collision.gameObject);
            }
        }
    }

    void DieByJump(GameObject player)
    {
        animator.SetBool("IsRunning", false);
        animator.SetTrigger("IsHitting");
        player.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(player.GetComponent<Rigidbody2D>().linearVelocity.x, 10f);
        rb.bodyType = RigidbodyType2D.Kinematic;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
        Invoke("Die", 0.5f);
    }

    // --- HÀM VẼ GIZMOS KHUNG RADAR SIÊU ĐẬM NÉT ---
    private void OnDrawGizmos()
    {
        // I. VẼ VÙNG HỘP RADAR QUÉT PLAYER
        if (showRadarZone)
        {
            Vector3 center = transform.position;
            Vector3 size = new Vector3(detectionRange * 2f, maxDetectionHeight * 2f, 0f);

            // 1. Vẽ ruột mờ bên trong
            Gizmos.color = isChasing ? radarChaseColor : radarIdleColor;
            Gizmos.DrawCube(center, size);

            // 2. Vẽ 4 đường viền TO RÕ bao quanh hộp bằng Handles
            Color currentBorderColor = isChasing ? borderChaseColor : borderIdleColor;

            Vector3 topLeft = center + new Vector3(-detectionRange, maxDetectionHeight, 0);
            Vector3 topRight = center + new Vector3(detectionRange, maxDetectionHeight, 0);
            Vector3 bottomLeft = center + new Vector3(-detectionRange, -maxDetectionHeight, 0);
            Vector3 bottomRight = center + new Vector3(detectionRange, -maxDetectionHeight, 0);

            DrawThickLine(topLeft, topRight, radarBorderThickness, currentBorderColor);       // Cạnh trên
            DrawThickLine(topRight, bottomRight, radarBorderThickness, currentBorderColor);   // Cạnh phải
            DrawThickLine(bottomRight, bottomLeft, radarBorderThickness, currentBorderColor); // Cạnh dưới
            DrawThickLine(bottomLeft, topLeft, radarBorderThickness, currentBorderColor);     // Cạnh trái
        }

        // II. VẼ TIA LAZER KHÓA MỤC TIÊU NỐI ĐẾN TRỰC TIẾP PLAYER
        if (Application.isPlaying && isChasing && playerTransform != null)
        {
            DrawThickLine(transform.position, playerTransform.position, rayThickness + 2f, Color.magenta);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(playerTransform.position, 0.08f);
        }

        // III. VẼ TIA QUÉT ĐẤT VÀ TƯỜNG
        if (wallCheck == null) return;

        int moveDir = 1;
        if (Application.isPlaying && playerTransform != null)
        {
            moveDir = (playerTransform.position.x > transform.position.x) ? 1 : -1;
        }
        else
        {
            moveDir = (transform.localScale.x > 0) ? 1 : -1;
            if (!isSpriteFacingRight) moveDir *= -1;
        }

        bool isFrontGrounded = Physics2D.Raycast(wallCheck.position, Vector2.down, 0.7f, groundLayer);
        bool isHittingWall = Physics2D.Raycast(wallCheck.position, Vector2.right * moveDir, 0.3f, groundLayer);

        // Tia đất
        Color groundColor = isFrontGrounded ? Color.green : Color.red;
        DrawThickLine(wallCheck.position, wallCheck.position + Vector3.down * 0.7f, rayThickness, groundColor);
        Gizmos.color = groundColor;
        Gizmos.DrawSphere(wallCheck.position + Vector3.down * 0.7f, 0.05f);

        // Tia tường
        Vector3 wallDir = Vector3.right * moveDir;
        Color wallColor = isHittingWall ? Color.red : Color.green;
        DrawThickLine(wallCheck.position, wallCheck.position + wallDir * 0.3f, rayThickness, wallColor);
        Gizmos.color = wallColor;
        Gizmos.DrawSphere(wallCheck.position + wallDir * 0.3f, 0.04f);
    }

    void DrawThickLine(Vector3 start, Vector3 end, float thickness, Color color)
    {
#if UNITY_EDITOR
        UnityEditor.Handles.color = color;
        UnityEditor.Handles.DrawAAPolyLine(thickness, start, end);
#else
        Gizmos.color = color;
        Gizmos.DrawLine(start, end);
#endif
    }
}