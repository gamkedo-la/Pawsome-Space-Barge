using UnityEngine;

public class PlayModeShaderSupport : MonoBehaviour
{
    private void OnEnable()
    {
        if (Application.isPlaying)
        {
            Shader.EnableKeyword("_PLAY_MODE");
        }
    }

    private void OnDisable()
    {
        Shader.DisableKeyword("_PLAY_MODE");
    }
}