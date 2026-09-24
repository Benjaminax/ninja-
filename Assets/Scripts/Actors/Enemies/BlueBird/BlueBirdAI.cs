using UnityEngine;

public class BlueBirdAI : Enemy
{
    [Header("Patrol Settings")]
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float patrolRange = 5f; // Khoảng cách bay sang mỗi bên tính từ tâm
    [SerializeField] bool isSpriteFacingRight = false; // BlueBird trong ảnh nhìn TRÁI nên để false
    [SerializeField] bool randomizeStart = true;

    [Header("Simulation Visuals (Nâng Cấp)")]
    [SerializeField] float pathThickness = 6f; // Độ dày của đường tuần tra xanh lá (Chỉnh trong Inspector)

    private Vector3 startPosition;
    private int walkDirection = 1; // 1: Phải, -1: Trái

    protected override void Start()
    {
        base.Start();
        rb.gravityScale = 0; // Chim không rơi
        startPosition = transform.position; // Lưu vị trí gốc làm tâm

        if (randomizeStart)
        {
            walkDirection = Random.value > 0.5f ? 1 : -1;
        }
        else
        {
            walkDirection = 1;
        }

        UpdateSpriteDirection();
    }

    void Update()
    {
        PatrolLogic();
    }

    void PatrolLogic()
    {
        // 1. Tính toán khoảng cách hiện tại so với tâm (điểm bắt đầu)
        float distanceFromStart = transform.position.x - startPosition.x;

        // 2. Nếu bay quá phạm vi bên phải (phạm vi dương)
        if (distanceFromStart >= patrolRange && walkDirection == 1)
        {
            Flip();
        }
        // 3. Nếu bay quá phạm vi bên trái (phạm vi âm)
        else if (distanceFromStart <= -patrolRange && walkDirection == -1)
        {
            Flip();
        }

        // 4. Luôn bay thẳng theo hướng hiện tại
        rb.linearVelocity = new Vector2(moveSpeed * walkDirection, 0);
    }

    void Flip()
    {
        walkDirection *= -1;
        UpdateSpriteDirection();
    }

    void UpdateSpriteDirection()
    {
        float scaleX = isSpriteFacingRight ? walkDirection : -walkDirection;
        transform.localScale = new Vector3(scaleX, 1f, 1f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.GetContact(0).normal.y < -0.5f)
            {
                animator.SetTrigger("IsHitting");
                animator.SetBool("IsFlying", false);
                collision.gameObject.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(collision.gameObject.GetComponent<Rigidbody2D>().linearVelocity.x, 10f);
                rb.linearVelocity = Vector2.zero;
                GetComponent<Collider2D>().enabled = false;
                Invoke("Die", 0.5f);
            }
        }
    }

    // --- HÀM VẼ GIZMOS NÂNG CẤP: VẼ ĐƯỜNG BAY DÀY TÙY CHỈNH ---
    private void OnDrawGizmos()
    {
        // Xác định vị trí tâm (Nếu game đang chạy thì lấy startPosition gốc, nếu chưa chạy thì lấy vị trí hiện tại trong Scene)
        Vector3 centerPos = (Application.isPlaying) ? startPosition : transform.position;

        // Tính toán vị trí 2 đầu mút của đường tuần tra
        Vector3 leftPoint = centerPos + Vector3.left * patrolRange;
        Vector3 rightPoint = centerPos + Vector3.right * patrolRange;

        // 1. Vẽ đường thẳng nét dày màu xanh lá thể hiện vùng tuần tra
        DrawThickLine(leftPoint, rightPoint, pathThickness, Color.green);

        // 2. Vẽ thêm 2 nút tròn nhỏ ở 2 đầu giới hạn để nhìn rõ điểm quay đầu của chim
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(leftPoint, 0.1f);
        Gizmos.DrawSphere(rightPoint, 0.1f);
    }

    // Hàm phụ trợ dùng để vẽ nét dày (Đảm bảo an toàn không lỗi khi Build game)
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