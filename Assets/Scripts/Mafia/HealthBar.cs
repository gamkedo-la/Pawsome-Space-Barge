using UnityEngine;
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
        public float changeAnimationSpeed = 0.5f;
        
        [Header("Connections")]
        public IntVariable missionType;
        public MinMaxIntVariable bargeHealth;
        public Image bar;

        private Rect originalRect;
        private float targetWidth;

        private void Awake()
        {
            // Remove this component if the mission type isn't Mafia
            if (missionType.Value != (int)MissionType.Mafia)
            {
                Destroy(this);
            }

            originalRect = bar.rectTransform.rect;
        }

        private void FixedUpdate()
        {
            targetWidth = originalRect.width * bargeHealth.Ratio;
            bar.color = healthGradient.Evaluate(1f-bargeHealth.Ratio);
            
            var width = Mathf.Lerp(bar.rectTransform.rect.width, targetWidth, changeAnimationSpeed);
            bar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }
    }
}