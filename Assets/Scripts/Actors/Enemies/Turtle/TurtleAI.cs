using UnityEngine;

public class TurtleAI : Enemy
{
    private enum TurtleState { Safe, Dangerous, Hiding }

    [Header("Cycle Settings")]
    [SerializeField] float noSpikesDuration = 3f;   // Thời gian an toàn (idle_02)
    [SerializeField] float withSpikesDuration = 3f; // Thời gian giữ gai (idle_01)
    [SerializeField] float transitionDuration = 0.5f; // Thời gian chạy animation (spikesOut/spikesIn)

    // --- BỔ SUNG CÀI ĐẶT BẤT ĐỒNG BỘ ---
    [Header("Simulation Settings")]
    [SerializeField] bool randomizeStart = true;    // Tích chọn để các con rùa tự lệch pha nhau

    private TurtleState currentState = TurtleState.Safe;
    private float stateTimer;

    protected override void Start()
    {
        base.Start();

        // Đảm bảo Body Type là Dynamic để rùa nằm trên mặt đất
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // --- XỬ LÝ LOGIC BẤT ĐỒNG BỘ KHI VỪA BẮT ĐẦU ---
        if (randomizeStart)
        {
            // Ngẫu nhiên chọn 1 trong 3 trạng thái: 0 (Safe), 1 (Dangerous), 2 (Hiding)
            currentState = (TurtleState)Random.Range(0, 3);

            // Dựa vào trạng thái được chọn ngẫu nhiên để thiết lập thời gian chờ và animation tương ứng
            switch (currentState)
            {
                case TurtleState.Safe:
                    stateTimer = Random.Range(0f, noSpikesDuration);
                    animator.SetBool("IsSpikesOutting", false);
                    break;

                case TurtleState.Dangerous:
                    // Tổng thời gian của Dangerous là thời gian mọc + thời gian giữ gai
                    stateTimer = Random.Range(0f, transitionDuration + withSpikesDuration);
                    animator.SetBool("IsSpikesOutting", true);
                    break;

                case TurtleState.Hiding:
                    stateTimer = Random.Range(0f, transitionDuration);
                    animator.SetBool("IsSpikesOutting", false);
                    break;
            }
        }
        else
        {
            // Nếu không dùng random thì quay về logic mặc định (Đồng bộ)
            currentState = TurtleState.Safe;
            stateTimer = noSpikesDuration;
            animator.SetBool("IsSpikesOutting", false);
        }
    }

    void Update()
    {
        if (stateTimer > 0)
            stateTimer -= Time.deltaTime;

        if (stateTimer <= 0)
        {
            UpdateState();
        }
    }

    void UpdateState()
    {
        switch (currentState)
        {
            case TurtleState.Safe:
                // Từ Safe -> Bắt đầu mọc gai (Dangerous)
                currentState = TurtleState.Dangerous;
                animator.SetBool("IsSpikesOutting", true);
                stateTimer = transitionDuration + withSpikesDuration;
                break;

            case TurtleState.Dangerous:
                // Hết thời gian giữ gai -> Bắt đầu thu gai (Hiding)
                currentState = TurtleState.Hiding;
                animator.SetBool("IsSpikesOutting", false);
                stateTimer = transitionDuration;
                break;

            case TurtleState.Hiding:
                // Thu gai xong -> Quay lại Safe
                currentState = TurtleState.Safe;
                stateTimer = noSpikesDuration;
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ContactPoint2D contact = collision.GetContact(0);
            var playerMove = collision.gameObject.GetComponent<PlayMove>();

            // 1. Trạng thái NGUY HIỂM (Đang mọc gai, đang giữ gai, hoặc đang thu gai)
            if (currentState == TurtleState.Dangerous || currentState == TurtleState.Hiding)
            {
                // Có gai -> Chạm đâu cũng chết
                // if (playerMove != null) playerMove.Die();
            }
            // 2. Trạng thái AN TOÀN (idle_02)
            else
            {
                // Nếu nhảy từ trên xuống -> Rùa chết
                if (contact.normal.y < -0.5f)
                {
                    HitByPlayer(collision.gameObject);
                }
                // Nếu chạm ngang hoặc dưới -> Player vẫn chết
                else
                {
                    // if (playerMove != null) playerMove.Die();
                }
            }
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

        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
        Invoke("Die", 0.5f);
    }
}