using UnityEngine;

public class FatBirdAI : Enemy
{
    private enum FatBirdState { Hover, Fall, Ground, Return }

    [Header("Hover Settings")]
    [SerializeField] float hoverRange = 1.5f;
    [SerializeField] float hoverSpeed = 2f;

    [Header("Attack Settings")]
    [SerializeField] float fallSpeed = 25f;
    [SerializeField] float detectionWidth = 1.5f;
    [SerializeField] float detectionHeight = 10f;
    [SerializeField] LayerMask playerLayer;

    [Header("Return Settings")]
    [SerializeField] float groundWaitTime = 1f;
    [SerializeField] float returnSpeed = 4f;

    private FatBirdState currentState = FatBirdState.Hover;
    private Vector3 startPosition;
    private float hoverOffset;
    private float stateTimer;
    private Transform playerTransform;

    protected override void Start()
    {
        base.Start();
        startPosition = transform.position;
        // Mỗi con có một độ lệch ngẫu nhiên để bay lên xuống không đồng bộ
        hoverOffset = Random.Range(0f, 100f);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        rb.bodyType = RigidbodyType2D.Kinematic; // Dùng Kinematic để bay lơ lửng không bị rơi tự do
    }

    void Update()
    {
        switch (currentState)
        {
            case FatBirdState.Hover:
                HoverLogic();
                CheckForPlayer();
                break;
            case FatBirdState.Fall:
                // Logic rơi được xử lý chủ yếu bằng vận tốc Rigidbody
                break;
            case FatBirdState.Ground:
                GroundLogic();
                break;
            case FatBirdState.Return:
                ReturnLogic();
                break;
        }
    }

    void HoverLogic()
    {
        // Bay lên xuống mượt mà dùng Sin
        float newY = startPosition.y + Mathf.Sin((Time.time + hoverOffset) * hoverSpeed) * hoverRange;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        animator.Play("Idle"); // Đảm bảo chạy animation Idle
    }

    void CheckForPlayer()
    {
        if (playerTransform == null) return;

        // Kiểm tra xem Player có đang đứng dưới FatBird không
        float xDistance = Mathf.Abs(transform.position.x - playerTransform.position.x);
        float yDistance = transform.position.y - playerTransform.position.y;

        if (xDistance < detectionWidth && yDistance > 0 && yDistance < detectionHeight)
        {
            StartFalling();
        }
    }

    void StartFalling()
    {
        currentState = FatBirdState.Fall;
        rb.bodyType = RigidbodyType2D.Dynamic; // Chuyển sang Dynamic để rơi thật
        rb.gravityScale = 3f; // Tăng trọng lực để rơi nhanh
        rb.linearVelocity = new Vector2(0, -fallSpeed);

        animator.Play("Fall");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ContactPoint2D contact = collision.GetContact(0);

            // 1. Kiểm tra nếu Player nhảy lên đầu (va chạm từ trên xuống)
            if (contact.normal.y < -0.5f)
            {
                HitByPlayer(collision.gameObject);
                return;
            }

            // 2. Nếu đang rơi mà chạm cạnh Player -> Player chết
            if (currentState == FatBirdState.Fall)
            {
                var playerMove = collision.gameObject.GetComponent<PlayMove>();
                if (playerMove != null)
                {
                    // playerMove.Die();
                }
                StartReturn();
            }
        }
        else if (currentState == FatBirdState.Fall && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // Nếu chạm đất
            currentState = FatBirdState.Ground;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            stateTimer = groundWaitTime;

            animator.Play("Ground");
        }
    }

    void HitByPlayer(GameObject player)
    {
        animator.SetTrigger("IsHitting");

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 12f);
        }

        currentState = FatBirdState.Return;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;

        Invoke("Die", 0.5f);
    }

    void GroundLogic()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            currentState = FatBirdState.Return;
            animator.Play("Idle");
        }
    }

    void ReturnLogic()
    {
        // Bay từ từ về vị trí gốc
        transform.position = Vector3.MoveTowards(transform.position, startPosition, returnSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, startPosition) < 0.01f)
        {
            // Reset offset để tránh bị giật khi chuyển về Hover
            hoverOffset = -Time.time;
            currentState = FatBirdState.Hover;
        }
    }


    void StartReturn()
    {
        currentState = FatBirdState.Return;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        animator.Play("Idle");
    }

    // Vẽ vùng phát hiện trong Scene để bạn dễ căn chỉnh
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position + Vector3.down * (detectionHeight / 2f);
        Gizmos.DrawWireCube(center, new Vector3(detectionWidth * 2f, detectionHeight, 0));
    }
}
