using UnityEngine;

namespace Variables
{
    [CreateAssetMenu(menuName = "Variables/Int (Min, Max)")]
    public class MinMaxIntVariable : IntVariable
    {
        [SerializeField]
        private int minValue;
        [SerializeField]
        private int maxValue;

        public int MinValue
        {
            get => minValue;
            set => minValue = value;
        }

        public int MaxValue
        {
            get => maxValue;
            set => maxValue = value;
        }

        public float Ratio => (float) (Value - MinValue) / (MaxValue - MinValue);

        public void Add(int delta)
        {
            Value = delta switch
            {
                > 0 => Mathf.Min(Value + delta, maxValue),
                < 0 => Mathf.Max(Value + delta, minValue),
                _ => Value
            };
        }

        public void Subtract(int delta)
        {
            Add(-delta);
        }
    }
}