using UnityEngine;

namespace Variables
{
    public class GenericVariable<T> : ScriptableObject
    {
        [SerializeField] private T value;

        public T Value
        {
            get => value;
            set => this.value = value;
        }

        private void OnEnable()
        {
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }
    }
}