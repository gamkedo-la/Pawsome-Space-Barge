using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Variables;

namespace Mafia
{
    public class HealthBar : MonoBehaviour
    {
        [Header("Appearance")] 
        [Tooltip("Colour of the health bar, full health to the left")]
        public Gradient healthGradient;

        [Range(0.001f, 1)] 
        public float colourAnimationSpeed = 0.1f;
        
        [Range(0.001f, 1)] 
        public float numberAnimationSpeed = 0.7f;
        
        [Header("Connections")]
        public IntVariable missionType;
        public MinMaxIntVariable bargeHealth;
        public Image bar;
        public TMP_Text numberDisplay;

        private Rect originalRect;
        private float displayedWidth;
        private float targetWidth;
        private float displayedHealth;
        private int targetHealth;

        private void Awake()
        {
            // Disable this display if the mission type isn't Mafia
            if (missionType.Value != (int)MissionType.Mafia)
            {
                gameObject.SetActive(false);
                return;
            }

            originalRect = bar.rectTransform.rect;
        }

        private void FixedUpdate()
        {
            targetWidth = originalRect.width * bargeHealth.Ratio;
            targetHealth = bargeHealth.Value;
            bar.color = healthGradient.Evaluate(1f-bargeHealth.Ratio);
            
            displayedWidth = Mathf.Lerp(displayedWidth, targetWidth, colourAnimationSpeed);
            bar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, displayedWidth);

            if (Mathf.RoundToInt(displayedHealth) != targetHealth)
            {
                UpdateHealthNumber();
            }
        }

        private void UpdateHealthNumber()
        {
            displayedHealth = Mathf.Lerp(displayedHealth, targetHealth, numberAnimationSpeed);
            numberDisplay.text = Mathf.RoundToInt(displayedHealth).ToString();
        }
    }
}