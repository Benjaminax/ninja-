using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class MobileControls : MonoBehaviour
{
    public static MobileControls Instance { get; private set; }

    public float MoveX { get; private set; }
    public bool JumpPressedThisFrame { get; private set; }
    public bool SwordPressedThisFrame { get; private set; }
    public bool FireballPressedThisFrame { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void LateUpdate()
    {
        JumpPressedThisFrame = false;
        SwordPressedThisFrame = false;
        FireballPressedThisFrame = false;
    }

    public void SetMove(float value)
    {
        MoveX = Mathf.Clamp(value, -1f, 1f);
    }

    public void StopMove()
    {
        MoveX = 0f;
    }

    public void PressJump()
    {
        JumpPressedThisFrame = true;
    }

    public void PressSword()
    {
        SwordPressedThisFrame = true;
    }

    public void PressFireball()
    {
        FireballPressedThisFrame = true;
    }

    public static GameObject CreateButton(
        Transform parent,
        string name,
        string label,
        Vector2 anchor,
        Vector2 size,
        Color color,
        UnityEngine.Events.UnityAction action,
        bool repeatWhileHeld = false)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = true;

        MobileTouchButton touchButton = buttonObject.AddComponent<MobileTouchButton>();
        touchButton.Action = action;
        touchButton.RepeatWhileHeld = repeatWhileHeld;

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;

        GameObject textObject = new GameObject("Label");
        textObject.transform.SetParent(buttonObject.transform, false);
        Text text = textObject.AddComponent<Text>();
        text.text = label;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 30;
        text.fontStyle = FontStyle.Bold;
        text.color = Color.white;
        text.raycastTarget = false;
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        return buttonObject;
    }

    public sealed class MobileTouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public UnityEngine.Events.UnityAction Action { get; set; }
        public bool RepeatWhileHeld { get; set; }
        bool held;

        void Update()
        {
            if (held && RepeatWhileHeld)
                Action?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            held = true;
            Action?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            held = false;
            if (RepeatWhileHeld && MobileControls.Instance != null)
                MobileControls.Instance.StopMove();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            held = false;
            if (RepeatWhileHeld && MobileControls.Instance != null)
                MobileControls.Instance.StopMove();
        }
    }
}
