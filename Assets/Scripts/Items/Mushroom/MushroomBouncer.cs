using UnityEngine;

public class MushroomBouncer : MonoBehaviour
{
    Animator myAnimator;

    void Start()
    {
        myAnimator = GetComponent<Animator>();
    }

    // Hàm này tự động gọi khi có vật thể va chạm với cây nấm
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Kiểm tra xem vật va chạm có phải là Player không
        if (collision.gameObject.CompareTag("Player"))
        {
            // Kiểm tra xem Player có phải đang dẫm từ trên xuống không (normal.y < -0.5 nghĩa là hướng va chạm từ trên đỉnh nấm xuống )
            if (collision.GetContact(0).normal.y < -0.5f)
            {
                // Dùng Trigger thay vì bool để phản hồi tức thì
                myAnimator.SetTrigger("Bounce");
            }
        }
    }
}