using UnityEngine;
using UnityEngine.UI;
using Variables;

namespace UI
{
    public class PercentageBar : MonoBehaviour
    {
        [Header("Variable")] [Tooltip("Variable to track")]
        public MinMaxIntVariable variable;
        
        [Header("Appearance")] 
        [Tooltip("Colour of the health bar, full health to the left")]
        public Gradient gradient;

        [Range(0.001f, 1)] 
        public float colourAnimationSpeed = 0.1f;

        [Header("Connection")]
        public Image bar;
        
        private Rect originalRect;
        private float displayedWidth;
        private float targetWidth;

        private void Start()
        {
            originalRect = bar.rectTransform.rect;
        }
        
        private void FixedUpdate()
        {
            targetWidth = originalRect.width * variable.Ratio;
            bar.color = gradient.Evaluate(1f-variable.Ratio);
            
            displayedWidth = Mathf.Lerp(displayedWidth, targetWidth, colourAnimationSpeed);
            bar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, displayedWidth);
        }
    }
}