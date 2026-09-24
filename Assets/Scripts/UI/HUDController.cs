using UnityEngine;
using UnityEngine.UIElements;

public class HUDController : MonoBehaviour
{
    private Label scoreLabel;

    private void OnEnable()
    {
        // 1. Lấy tham chiếu đến Label
        var root = GetComponent<UIDocument>().rootVisualElement;
        scoreLabel = root.Q<Label>("scoreLabel");

        // 2. Đăng ký lắng nghe sự kiện thay đổi điểm
        GameData.OnScoreChanged += UpdateScoreUI;

        // Cập nhật lần đầu tiên khi bắt đầu
        UpdateScoreUI();
    }

    private void OnDisable()
    {
        // Hủy đăng ký để tránh lỗi bộ nhớ
        GameData.OnScoreChanged -= UpdateScoreUI;
    }

    void UpdateScoreUI()
    {
        if (scoreLabel != null && GameData.instance != null)
        {
            // Cập nhật con số lên màn hình
            scoreLabel.text = GameData.instance.Score.ToString("00");
        }
    }
}