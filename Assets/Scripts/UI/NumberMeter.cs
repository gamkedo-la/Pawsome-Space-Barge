using TMPro;
using UnityEngine;
using Variables;

namespace UI
{
    public class NumberMeter : MonoBehaviour
    {
        [Header("Variable")] [Tooltip("Variable to track")]
        public MinMaxIntVariable variable;
        
        [Header("Appearance")] 
        [Range(0.001f, 1)] 
        public float numberAnimationSpeed = 0.2f;

        [Tooltip("C# number formatting string")]
        public string format = "0";
        public bool displayAsPercent = false;

        [Header("Connection")]
        public TMP_Text numberDisplay;
        
        private float displayedNumber = -1; // needs to be -1 for initialization check

        private void FixedUpdate()
        {
            if (Mathf.RoundToInt(displayedNumber) != variable.Value)
            {
                UpdateHealthNumber();
            }
        }

        private void UpdateHealthNumber()
        {
            if (displayedNumber == -1)
            {
                // initialize value on first pass
                // for some reason doing this in monobehaviour start displays '000000'
                displayedNumber = variable.Value;
            }

            displayedNumber = Mathf.Lerp(displayedNumber, variable.Value, numberAnimationSpeed);

            if (displayAsPercent)
            {
                numberDisplay.text = $"{(displayedNumber / variable.MaxValue * 100).ToString("0")}%";
            }
            else
            {
                numberDisplay.text = (displayedNumber * 10f).ToString(format);
            }
        }
    }
}