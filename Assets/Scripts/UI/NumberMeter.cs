using TMPro;
using UnityEngine;
using Variables;

namespace UI
{
    public class NumberMeter : MonoBehaviour
    {
        [Header("Variable")] [Tooltip("Variable to track")]
        public IntVariable variable;
        
        [Header("Appearance")] 
        [Range(0.001f, 1)] 
        public float numberAnimationSpeed = 0.2f;

        [Tooltip("C# number formatting string")]
        public string format = "0";
        
        [Header("Connection")]
        public TMP_Text numberDisplay;
        
        private float displayedNumber;

        private void FixedUpdate()
        {
            if (Mathf.RoundToInt(displayedNumber) != variable.Value)
            {
                UpdateHealthNumber();
            }
        }

        private void UpdateHealthNumber()
        {
            displayedNumber = Mathf.Lerp(displayedNumber, variable.Value, numberAnimationSpeed);
            numberDisplay.text = displayedNumber.ToString(format);
        }
    }
}