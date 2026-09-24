using ThomasDev.HealthDamageSystem;
using ThomasDev.HealthSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameFlowController : MonoBehaviour
{
    [SerializeField] GameObject skeletonPrefab;
    [SerializeField] GameObject playerHealthBarPrefab;
    [SerializeField] Sprite pauseButtonSprite;
    [SerializeField] Sprite actionButtonSprite;
    [SerializeField] float playerMaxHealth = 5f;

    Canvas canvas;
    GameObject pausePanel;
    GameObject victoryPanel;
    GameObject deathPanel;
    GameObject pauseButton;
    bool paused;
    bool victorious;

    void Start()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null && mainCamera.GetComponent<MobileCameraView>() == null)
            mainCamera.gameObject.AddComponent<MobileCameraView>();

        GameObject oldInterface = GameObject.Find("GameFlowUI");
        if (oldInterface != null)
            Destroy(oldInterface);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Health health = player.GetComponent<Health>();
            if (health == null)
            {
                health = player.AddComponent<Health>();
                health.Initialize(playerMaxHealth);
            }
            health.OnDeath.AddListener(OnPlayerDeath);
        }

        CreateInterface(player);
    }

    void Update()
    {
        if (victorious)
            return;

        if (Keyboard.current != null &&
            (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame))
            SetPaused(!paused);
    }

    public void ConfigureVictoryTrigger()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        if (trigger != null)
            trigger.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            ShowVictory();
    }

    void SetPaused(bool value)
    {
        paused = value;
        Time.timeScale = paused ? 0f : 1f;
        if (pausePanel != null)
            pausePanel.SetActive(paused);
        if (pauseButton != null)
            pauseButton.SetActive(!paused);
    }

    void OnPlayerDeath()
    {
        paused = false;
        Time.timeScale = 0f;
        if (pauseButton != null)
            pauseButton.SetActive(false);
        if (deathPanel != null)
            deathPanel.SetActive(true);
    }

    void ShowVictory()
    {
        victorious = true;
        SetPaused(false);
        if (victoryPanel != null)
            victoryPanel.SetActive(true);
    }

    void CreateInterface(GameObject player)
    {
        GameObject canvasObject = new GameObject("GameFlowUI");
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();
        EnsureEventSystem();
        CreateMobileControls();

        pausePanel = CreatePanel("PAUSED", new Color(0f, 0f, 0f, 0.78f));
        pausePanel.SetActive(false);
        victoryPanel = CreatePanel("VICTORY!", new Color(0.05f, 0.3f, 0.05f, 0.88f));
        victoryPanel.SetActive(false);
        deathPanel = CreatePanel("YOU DIED", new Color(0.25f, 0.02f, 0.02f, 0.92f));
        AddOverlayMessage(deathPanel, "RETRY", new Vector2(0.5f, 0.57f));
        deathPanel.SetActive(false);
        pauseButton = CreatePauseButton();
        if (player != null && playerHealthBarPrefab != null)
        {
            GameObject playerBar = Instantiate(playerHealthBarPrefab, canvas.transform);
            playerBar.transform.SetAsFirstSibling();
            HealthBarUI healthBar = playerBar.GetComponentInChildren<HealthBarUI>(true);
            if (healthBar != null)
            {
                healthBar.Bind(player.GetComponent<Health>());
                healthBar.UseThresholdColors(Color.green, Color.red, 0.5f);
            }

            RectTransform barRect = playerBar.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 0f);
            barRect.anchorMax = new Vector2(0f, 0f);
            barRect.pivot = new Vector2(0f, 0f);
            barRect.anchoredPosition = new Vector2(32f, 40f);
            barRect.sizeDelta = new Vector2(203f, 27f);
            barRect.localScale = Vector3.one * 3f;
        }
    }

    GameObject CreatePanel(string message, Color color)
    {
        GameObject panel = new GameObject(message + "Panel");
        panel.transform.SetParent(canvas.transform, false);
        Image background = panel.AddComponent<Image>();
        background.color = color;
        background.raycastTarget = false;
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        GameObject textObject = new GameObject("Message");
        textObject.transform.SetParent(panel.transform, false);
        Text text = textObject.AddComponent<Text>();
        text.text = message;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 64;
        text.color = Color.white;
        text.raycastTarget = false;
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        if (message == "PAUSED" || message == "YOU DIED")
        {
            string buttonName = message == "PAUSED" ? "ResumeButton" : "RetryButton";
            GameObject buttonObject = new GameObject(buttonName);
            buttonObject.transform.SetParent(panel.transform, false);
            Button button = buttonObject.AddComponent<Button>();
            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.sprite = actionButtonSprite;
            buttonImage.type = Image.Type.Simple;
            buttonImage.preserveAspect = true;
            buttonImage.color = Color.white;
            buttonImage.raycastTarget = true;
            button.targetGraphic = buttonImage;
            button.interactable = true;
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.3f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.3f);
            buttonRect.sizeDelta = new Vector2(128f, 128f);
            buttonRect.anchoredPosition = Vector2.zero;
            buttonObject.SetActive(true);
            if (message == "PAUSED")
                button.onClick.AddListener(() => SetPaused(false));
            else
                button.onClick.AddListener(RestartLevel);
        }

        return panel;
    }

    void AddOverlayMessage(GameObject panel, string message, Vector2 anchor)
    {
        GameObject textObject = new GameObject(message + "Message");
        textObject.transform.SetParent(panel.transform, false);
        Text text = textObject.AddComponent<Text>();
        text.text = message;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 36;
        text.fontStyle = FontStyle.Bold;
        text.color = Color.white;
        text.raycastTarget = false;
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = anchor;
        textRect.anchorMax = anchor;
        textRect.sizeDelta = new Vector2(500f, 70f);
        textRect.anchoredPosition = Vector2.zero;
    }

    void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    GameObject CreatePauseButton()
    {
        GameObject buttonObject = new GameObject("PauseButton");
        buttonObject.transform.SetParent(canvas.transform, false);
        Image image = buttonObject.AddComponent<Image>();
        image.sprite = pauseButtonSprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = true;
        image.color = Color.white;
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.03f, 0.93f);
        rect.anchorMax = new Vector2(0.03f, 0.93f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(88f, 88f);
        rect.anchoredPosition = Vector2.zero;
        button.onClick.AddListener(TogglePause);
        return buttonObject;
    }

    void CreateMobileControls()
    {
        if (!Application.isMobilePlatform && !Input.touchSupported)
            return;

        MobileControls controls = canvas.gameObject.AddComponent<MobileControls>();
        MobileControls.CreateButton(
            canvas.transform, "MoveLeftButton", "<", new Vector2(0.08f, 0.18f),
            new Vector2(120f, 120f), new Color(0f, 0f, 0f, 0.5f),
            () => controls.SetMove(-1f), true);
        MobileControls.CreateButton(
            canvas.transform, "MoveRightButton", ">", new Vector2(0.22f, 0.18f),
            new Vector2(120f, 120f), new Color(0f, 0f, 0f, 0.5f),
            () => controls.SetMove(1f), true);
        MobileControls.CreateButton(
            canvas.transform, "JumpButton", "JUMP", new Vector2(0.78f, 0.18f),
            new Vector2(170f, 120f), new Color(0.1f, 0.35f, 0.8f, 0.7f),
            controls.PressJump);
        MobileControls.CreateButton(
            canvas.transform, "SwordButton", "SWORD", new Vector2(0.9f, 0.29f),
            new Vector2(180f, 120f), new Color(0.75f, 0.15f, 0.1f, 0.7f),
            controls.PressSword);
        MobileControls.CreateButton(
            canvas.transform, "FireballButton", "FIRE", new Vector2(0.9f, 0.13f),
            new Vector2(180f, 100f), new Color(0.7f, 0.25f, 0.05f, 0.7f),
            controls.PressFireball);
    }

    void TogglePause()
    {
        SetPaused(!paused);
    }

    void EnsureEventSystem()
    {
        EventSystem eventSystem = FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
        }

        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            StandaloneInputModule legacyModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (legacyModule != null)
                Destroy(legacyModule);
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        inputModule.enabled = true;
        eventSystem.enabled = true;
    }

    void OnDestroy()
    {
        if (Time.timeScale == 0f)
            Time.timeScale = 1f;
    }
}
