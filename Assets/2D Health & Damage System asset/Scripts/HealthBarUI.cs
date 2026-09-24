using UnityEngine;
using UnityEngine.UI;
using ThomasDev.HealthDamageSystem;

namespace ThomasDev.HealthSystem
{
    [DisallowMultipleComponent]
    public class HealthBarUI : MonoBehaviour
    {

        [SerializeField] private Image image;
        [SerializeField] private GameObject gameobject;
        [SerializeField] private Health targetHealth;


        private Health health;
        private bool subscribed;
        private bool thresholdColorsEnabled;
        private Color healthyColor = Color.green;
        private Color lowHealthColor = Color.red;
        private float lowHealthThreshold = 0.5f;

        public void Bind(Health target)
        {
            if (health != null)
            {
                health.OnDamaged.RemoveListener(OnHealthChanged);
                health.OnHealed.RemoveListener(OnHealthChanged);
            }

            health = target;
            if (isActiveAndEnabled)
                Subscribe();
        }

        public void UseThresholdColors(Color healthy, Color low, float threshold)
        {
            thresholdColorsEnabled = true;
            healthyColor = healthy;
            lowHealthColor = low;
            lowHealthThreshold = Mathf.Clamp01(threshold);
            UpdateColor();
        }

        private void Awake()
        {
            health = targetHealth;
            if (health == null && gameobject != null)
                health = gameobject.GetComponent<Health>();
            if (health == null)
                health = GetComponentInParent<Health>();
        }

        private void Start()
        {
            Subscribe();
        }

        private void Subscribe()
        {
            if (health == null || image == null)
                return;

            if (!subscribed)
            {
                health.OnDamaged.AddListener(OnHealthChanged);
                health.OnHealed.AddListener(OnHealthChanged);
                subscribed = true;
            }
            image.fillAmount = health.MaxHealth <= 0f ? 0f : health.CurrentHealth / health.MaxHealth;
            UpdateColor();
        }

        private void OnHealthChanged(float healthCurr, float healthMax)
        {
            if (image != null && healthMax > 0f)
            {
                image.fillAmount = Mathf.Clamp01(healthCurr / healthMax);
                UpdateColor();
            }
        }

        private void UpdateColor()
        {
            if (!thresholdColorsEnabled || image == null)
                return;

            image.color = image.fillAmount <= lowHealthThreshold ? lowHealthColor : healthyColor;
        }

        private void OnDestroy()
        {
            if (health == null || !subscribed)
                return;
            health.OnDamaged.RemoveListener(OnHealthChanged);
            health.OnHealed.RemoveListener(OnHealthChanged);
            subscribed = false;
        }

    }
}