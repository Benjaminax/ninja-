using UnityEngine;
using ThomasDev.HealthDamageSystem;

public class ItemCollector : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem vật va chạm có mang Tag là "Fruit" không
        if (collision.gameObject.CompareTag("Fruit"))
        {
            SpriteRenderer itemRenderer = collision.gameObject.GetComponent<SpriteRenderer>();
            bool isCherryGem = collision.gameObject.name.ToLowerInvariant().Contains("cherry")
                || (itemRenderer != null && itemRenderer.sprite != null
                    && itemRenderer.sprite.name == "Pack-ElementsGEMS_12");
            if (GameData.instance != null && isCherryGem)
                GameData.instance.CollectCherryGem();

            Health playerHealth = GetComponent<Health>();
            if (playerHealth != null && (collision.gameObject.name.ToLowerInvariant().Contains("kiwi")
                || collision.gameObject.name.ToLowerInvariant().Contains("gem")))
                playerHealth.Heal(1f);

            // 1. Tăng điểm số trong GameData
            if (GameData.instance != null)
            {
                GameData.instance.Score++;
            }
            else
            {
                Debug.LogWarning("Không tìm thấy GameData instance trong Scene!");
            }

            // 2. Xóa vật phẩm khỏi Scene
            Destroy(collision.gameObject);
            
            // 3. Log để kiểm tra
            int gemCount = GameData.instance != null ? GameData.instance.CherryGemCount : 0;
            Debug.Log($"Đã nhặt trái cây! Điểm hiện tại: {GameData.instance?.Score ?? 0}; Gem power: {gemCount}/5");
        }
    }
}
